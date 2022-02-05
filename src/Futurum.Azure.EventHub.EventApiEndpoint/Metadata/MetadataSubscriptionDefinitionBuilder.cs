using Futurum.ApiEndpoint;
using Futurum.ApiEndpoint.DebugLogger;
using Futurum.Core.Option;
using Futurum.EventApiEndpoint.Metadata;

namespace Futurum.Azure.EventHub.EventApiEndpoint.Metadata;

public class MetadataSubscriptionDefinitionBuilder
{
    private readonly Type _apiEndpointType;
    private string _topic;
    private string _blobContainerName;
    private Option<string> _consumerGroup;

    public MetadataSubscriptionDefinitionBuilder(Type apiEndpointType)
    {
        _apiEndpointType = apiEndpointType;
    }

    public MetadataSubscriptionDefinitionBuilder Topic(string topic)
    {
        _topic = topic;

        return this;
    }

    public MetadataSubscriptionDefinitionBuilder BlobContainerName(string blobContainerName)
    {
        _blobContainerName = blobContainerName;

        return this;
    }

    public MetadataSubscriptionDefinitionBuilder ConsumerGroup(string consumerGroup)
    {
        _consumerGroup = consumerGroup;

        return this;
    }

    public IEnumerable<IMetadataDefinition> Build()
    {
        yield return new MetadataSubscriptionEventDefinition(new MetadataTopic(_topic),
                                                             new AzureEventHubMetadataBlobContainer(_blobContainerName),
                                                             _consumerGroup.Map(consumerGroup => new AzureEventHubMetadataConsumerGroup(consumerGroup)));
    }

    public ApiEndpointDebugNode Debug() =>
        new()
        {
            Name = $"{_topic} ({_apiEndpointType.FullName})"
        };
}