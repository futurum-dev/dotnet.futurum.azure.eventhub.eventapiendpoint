using Futurum.ApiEndpoint;
using Futurum.Azure.EventHub.EventApiEndpoint.Metadata;
using Futurum.EventApiEndpoint;

namespace Futurum.Azure.EventHub.EventApiEndpoint.Sample;

public class ApiEndpointDefinition : IApiEndpointDefinition
{
    public void Configure(ApiEndpointDefinitionBuilder definitionBuilder)
    {
        definitionBuilder.Event()
                         .AzureEventHub()
                         .Event<TestEventApiEndpoint.ApiEndpoint>(builder => builder.Topic("sample.eventhub").BlobContainerName("samplestoragecontainer2"))
                         .EnvelopeEvent(builder => builder.FromTopic("sample.eventhub")
                                                          .Route<TestBatchRouteEventApiEndpoint.ApiEndpoint>("test-batch-route"));
    }
}