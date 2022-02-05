namespace Futurum.Azure.EventHub.EventApiEndpoint.Sample;

public class Worker : BackgroundService
{
    private readonly IAzureEventHubEventApiEndpointWorkerService _workerService;

    public Worker(IAzureEventHubEventApiEndpointWorkerService workerService)
    {
        _workerService = workerService;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        _workerService.ExecuteAsync(stoppingToken);

    public override Task StopAsync(CancellationToken cancellationToken) =>
        _workerService.StopAsync(cancellationToken);
}