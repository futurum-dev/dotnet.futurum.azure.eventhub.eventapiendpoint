using Futurum.ApiEndpoint;
using Futurum.Core.Option;
using Futurum.EventApiEndpoint.Metadata;

namespace Futurum.Azure.EventHub.EventApiEndpoint.Metadata;

public record MetadataSubscriptionEventDefinition(MetadataTopic Topic, AzureEventHubMetadataBlobContainer BlobContainer, Option<AzureEventHubMetadataConsumerGroup> ConsumerGroup) : IMetadataDefinition;

public record AzureStorageConfiguration(string ConnectionString);

public record AzureEventHubMetadataConsumerGroup(string Value);

public record AzureEventHubMetadataBlobContainer(string Name);