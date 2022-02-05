using Futurum.Azure.EventHub.EventApiEndpoint.Metadata;
using Futurum.Core.Result;

using Serilog;

namespace Futurum.Azure.EventHub.EventApiEndpoint;

public interface IEventApiEndpointLogger : Futurum.EventApiEndpoint.IEventApiEndpointLogger, ApiEndpoint.IApiEndpointLogger
{
    void EventProcessorClientConfigurationError(MetadataSubscriptionEventDefinition metadataSubscriptionDefinition, Exception exception);

    void EventProcessorClientProcessError(MetadataSubscriptionEventDefinition metadataSubscriptionDefinition, string eventHubPartitionId, string eventHubOperation, Exception exception);

    void EventProcessorClientProcessEventError(MetadataSubscriptionEventDefinition metadataSubscriptionDefinition, IResultError error);

    void EventProcessorClientStartProcessingError(Exception exception);

    void EventProcessorClientStopProcessingError(Exception exception);
}

public class EventApiEndpointLogger : IEventApiEndpointLogger
{
    private readonly ILogger _logger;

    public EventApiEndpointLogger(ILogger logger)
    {
        _logger = logger;
    }

    public void EventReceived<TEvent>(TEvent @event)
    {
        var eventData = new EventReceivedData<TEvent>(typeof(TEvent), @event);

        _logger.Debug("AzureEventHub EventApiEndpoint event received {@eventData}", eventData);
    }

    public void EventProcessorClientConfigurationError(MetadataSubscriptionEventDefinition metadataSubscriptionDefinition, Exception exception)
    {
        var eventData = new ConfigurationErrorData(metadataSubscriptionDefinition);

        _logger.Error(exception, "AzureEventHub EventProcessorClient Configuration error {@eventData}", eventData);
    }

    public void EventProcessorClientProcessError(MetadataSubscriptionEventDefinition metadataSubscriptionDefinition, string eventHubPartitionId, string eventHubOperation, Exception exception)
    {
        var eventData = new ProcessErrorData(metadataSubscriptionDefinition, eventHubPartitionId, eventHubOperation);

        _logger.Error(exception, "AzureEventHub EventProcessorClient ProcessError error {@eventData}", eventData);
    }

    public void EventProcessorClientProcessEventError(MetadataSubscriptionEventDefinition metadataSubscriptionDefinition, IResultError error)
    {
        var eventData = new ProcessEventErrorData(metadataSubscriptionDefinition, error.ToErrorString());

        _logger.Error("AzureEventHub EventProcessorClient ProcessEventError error {@eventData}", eventData);
    }

    public void EventProcessorClientStartProcessingError(Exception exception)
    {
        _logger.Error(exception, "AzureEventHub EventProcessorClient StartProcessing error");
    }

    public void EventProcessorClientStopProcessingError(Exception exception)
    {
        _logger.Error(exception, "AzureEventHub EventProcessorClient StopProcessing error");
    }

    public void ApiEndpointDebugLog(string apiEndpointDebugLog)
    {
        var eventData = new ApiEndpoints(apiEndpointDebugLog);

        _logger.Debug("WebApiEndpoint endpoints {@eventData}", eventData);
    }

    private readonly record struct EventReceivedData<TEvent>(Type EventType, TEvent Event);

    private readonly record struct ConfigurationErrorData(MetadataSubscriptionEventDefinition MetadataSubscriptionDefinition);

    private readonly record struct ProcessErrorData(MetadataSubscriptionEventDefinition MetadataSubscriptionDefinition, string EventHubPartitionId, string EventHubOperation);

    private readonly record struct ProcessEventErrorData(MetadataSubscriptionEventDefinition MetadataSubscriptionDefinition, string Error);

    private record struct ApiEndpoints(string Log);
}