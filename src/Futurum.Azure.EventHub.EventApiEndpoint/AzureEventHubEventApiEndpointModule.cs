using System.Reflection;

using Futurum.EventApiEndpoint;
using Futurum.Microsoft.Extensions.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

namespace Futurum.Azure.EventHub.EventApiEndpoint;

public class AzureEventHubEventApiEndpointModule : IModule
{
    private readonly EventApiEndpointConfiguration _configuration;
    private readonly AzureEventHubConnectionConfiguration _connectionConfiguration;
    private readonly Assembly[] _assemblies;

    public AzureEventHubEventApiEndpointModule(EventApiEndpointConfiguration configuration,
                                               AzureEventHubConnectionConfiguration connectionConfiguration,
                                               params Assembly[] assemblies)
    {
        _configuration = configuration;
        _connectionConfiguration = connectionConfiguration;
        _assemblies = assemblies;
    }

    public AzureEventHubEventApiEndpointModule(AzureEventHubConnectionConfiguration connectionConfiguration,
                                               params Assembly[] assemblies)
        : this(EventApiEndpointConfiguration.Default, connectionConfiguration, assemblies)
    {
    }

    public void Load(IServiceCollection services)
    {
        services.RegisterModule(new EventApiEndpointModule(_configuration, _assemblies));

        services.AddSingleton(_connectionConfiguration);

        services.AddSingleton<IEventApiEndpointLogger, EventApiEndpointLogger>();
        services.AddSingleton<Futurum.EventApiEndpoint.IEventApiEndpointLogger, EventApiEndpointLogger>();
        services.AddSingleton<ApiEndpoint.IApiEndpointLogger, EventApiEndpointLogger>();

        services.AddSingleton<IAzureEventHubEventApiEndpointWorkerService, AzureEventHubEventApiEndpointWorkerService>();
    }
}