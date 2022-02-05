namespace Futurum.Azure.EventHub.EventApiEndpoint.Metadata;

public static class EventApiEndpointDefinitionExtensions
{
    public static EventApiEndpointDefinition AzureEventHub(this Futurum.EventApiEndpoint.EventApiEndpointDefinition eventApiEndpointDefinition)
    {
        var azureEventHubEventApiEndpointDefinition = new EventApiEndpointDefinition();
        
        eventApiEndpointDefinition.Add(azureEventHubEventApiEndpointDefinition);

        return azureEventHubEventApiEndpointDefinition;
    }
}