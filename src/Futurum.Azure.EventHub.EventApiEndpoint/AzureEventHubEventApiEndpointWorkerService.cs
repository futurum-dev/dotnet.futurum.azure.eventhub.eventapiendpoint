using System.Text;

using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;

using Futurum.Azure.EventHub.EventApiEndpoint.Metadata;
using Futurum.Core.Option;
using Futurum.Core.Result;
using Futurum.EventApiEndpoint;
using Futurum.EventApiEndpoint.Metadata;
using Futurum.Microsoft.Extensions.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

namespace Futurum.Azure.EventHub.EventApiEndpoint;

public interface IAzureEventHubEventApiEndpointWorkerService
{
    Task ExecuteAsync(CancellationToken stoppingToken);

    Task StopAsync(CancellationToken cancellationToken);
}

public class AzureEventHubEventApiEndpointWorkerService : IAzureEventHubEventApiEndpointWorkerService
{
    private readonly IEventApiEndpointLogger _logger;
    private readonly AzureEventHubConnectionConfiguration _connectionConfiguration;
    private readonly IServiceProvider _serviceProvider;
    private readonly IEventApiEndpointMetadataCache _metadataCache;

    private readonly List<EventProcessorClient> _eventProcessorClients = new();

    public AzureEventHubEventApiEndpointWorkerService(IEventApiEndpointLogger logger,
                                                      AzureEventHubConnectionConfiguration connectionConfiguration,
                                                      IServiceProvider serviceProvider,
                                                      IEventApiEndpointMetadataCache metadataCache)
    {
        _logger = logger;
        _connectionConfiguration = connectionConfiguration;
        _serviceProvider = serviceProvider;
        _metadataCache = metadataCache;
    }

    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ConfigureCommands(_connectionConfiguration.StorageConfiguration, _connectionConfiguration.ConnectionString);

        ConfigureBatchCommands(_connectionConfiguration.StorageConfiguration, _connectionConfiguration.ConnectionString, _connectionConfiguration.BatchBlobContainer,
                               _connectionConfiguration.BatchConsumerGroup);

        await StartAll(stoppingToken);
    }

    private void ConfigureCommands(AzureStorageConfiguration azureStorageConfiguration, string eventHubConnectionString)
    {
        var metadataDefinitions = _metadataCache.GetMetadataEventDefinitions();

        foreach (var (metadataSubscriptionDefinition, metadataTypeDefinition) in metadataDefinitions)
        {
            if (metadataSubscriptionDefinition is MetadataSubscriptionEventDefinition azureEventHubMetadataSubscriptionEventDefinition)
            {
                ConfigureSubscription(azureEventHubMetadataSubscriptionEventDefinition, metadataTypeDefinition.EventApiEndpointExecutorServiceType, azureStorageConfiguration.ConnectionString,
                                      eventHubConnectionString);
            }
        }
    }

    private void ConfigureBatchCommands(AzureStorageConfiguration azureStorageConfiguration, string eventHubConnectionString, AzureEventHubMetadataBlobContainer batchBlobContainer,
                                        Option<AzureEventHubMetadataConsumerGroup> batchConsumerGroup)
    {
        var metadataEnvelopeCommandDefinitions = _metadataCache.GetMetadataEnvelopeEventDefinitions();

        var metadataSubscriptionCommandDefinitions = metadataEnvelopeCommandDefinitions.Select(x => x.MetadataSubscriptionEventDefinition)
                                                                                       .Select(x => x.FromTopic)
                                                                                       .Distinct()
                                                                                       .Select(topic => new MetadataSubscriptionEventDefinition(topic, batchBlobContainer, batchConsumerGroup));

        foreach (var envelopeMetadataSubscriptionCommandDefinition in metadataSubscriptionCommandDefinitions)
        {
            var apiEndpointExecutorServiceType = typeof(EventApiEndpointExecutorService<,>).MakeGenericType(typeof(Batch.EventDto), typeof(Batch.Event));

            ConfigureSubscription(envelopeMetadataSubscriptionCommandDefinition, apiEndpointExecutorServiceType, azureStorageConfiguration.ConnectionString, eventHubConnectionString);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await StopAll(cancellationToken);
    }

    private void ConfigureSubscription(MetadataSubscriptionEventDefinition metadataSubscriptionDefinition, Type apiEndpointExecutorServiceType,
                                       string azureStorageConnectionString, string eventHubConnectionString)
    {
        var (topic, blobContainer, consumerGroupOption) = metadataSubscriptionDefinition;

        try
        {
            var consumerGroup = consumerGroupOption.GetValueOrDefault(new AzureEventHubMetadataConsumerGroup(EventHubConsumerClient.DefaultConsumerGroupName));

            var storageClient = new BlobContainerClient(azureStorageConnectionString, blobContainer.Name);

            var eventProcessorClient = new EventProcessorClient(storageClient, consumerGroup.Value, eventHubConnectionString, topic.Value);

            _eventProcessorClients.Add(eventProcessorClient);

            eventProcessorClient.ProcessEventAsync += processEventArgs => ProcessEventAsync(metadataSubscriptionDefinition, apiEndpointExecutorServiceType, processEventArgs);
            eventProcessorClient.ProcessErrorAsync += processErrorEventArgs => ProcessErrorAsync(metadataSubscriptionDefinition, processErrorEventArgs);
        }
        catch (Exception exception)
        {
            _logger.EventProcessorClientConfigurationError(metadataSubscriptionDefinition, exception);
        }
    }

    private async Task<Result> ProcessEventAsync(MetadataSubscriptionEventDefinition metadataSubscriptionDefinition, Type apiEndpointExecutorServiceType,
                                                 ProcessEventArgs processEventArgs)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();

        var message = Encoding.UTF8.GetString(processEventArgs.Data.Body.ToArray());

        return await scope.ServiceProvider.TryGetService<IEventApiEndpointExecutorService>(apiEndpointExecutorServiceType)
                          .ThenAsync(apiEndpointExecutorService => apiEndpointExecutorService.ExecuteAsync(metadataSubscriptionDefinition, message, processEventArgs.CancellationToken))
                          .DoAsync(_ => processEventArgs.UpdateCheckpointAsync(processEventArgs.CancellationToken))
                          .DoWhenFailureAsync(error => _logger.EventProcessorClientProcessEventError(metadataSubscriptionDefinition, error));
    }

    private Task ProcessErrorAsync(MetadataSubscriptionEventDefinition metadataSubscriptionDefinition, ProcessErrorEventArgs processErrorEventArgs)
    {
        _logger.EventProcessorClientProcessError(metadataSubscriptionDefinition, processErrorEventArgs.PartitionId, processErrorEventArgs.Operation, processErrorEventArgs.Exception);

        return Task.CompletedTask;
    }

    private async Task StartAll(CancellationToken stoppingToken)
    {
        foreach (var eventProcessorClient in _eventProcessorClients)
        {
            try
            {
                await eventProcessorClient.StartProcessingAsync(stoppingToken);
            }
            catch (Exception exception)
            {
                _logger.EventProcessorClientStartProcessingError(exception);
            }
        }
    }

    private async Task StopAll(CancellationToken cancellationToken)
    {
        foreach (var eventProcessorClient in _eventProcessorClients)
        {
            try
            {
                await eventProcessorClient.StopProcessingAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.EventProcessorClientStopProcessingError(exception);
            }
        }
    }
}