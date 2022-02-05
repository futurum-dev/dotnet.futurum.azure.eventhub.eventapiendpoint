using Futurum.Azure.EventHub.EventApiEndpoint.Metadata;
using Futurum.Core.Option;

namespace Futurum.Azure.EventHub.EventApiEndpoint;

/// <summary>
/// Azure EventHub EventApiEndpoint connection
/// </summary>
public record AzureEventHubConnectionConfiguration(string ConnectionString, AzureStorageConfiguration StorageConfiguration, AzureEventHubMetadataBlobContainer BatchBlobContainer,
                                                   Option<AzureEventHubMetadataConsumerGroup> BatchConsumerGroup);