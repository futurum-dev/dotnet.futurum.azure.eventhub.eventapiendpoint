using Futurum.Microsoft.Extensions.DependencyInjection;

namespace Futurum.Azure.EventHub.EventApiEndpoint.Sample;

public class ApplicationModule : IModule
{
    public void Load(IServiceCollection services)
    {
        services.AddSingleton<Worker>();
    }
}