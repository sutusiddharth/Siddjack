namespace CLAPi.ExcelEngine.Api.BackGroundJob;

public class QueuedHostedService(IBackgroundTaskQueue taskQueue, ILogger<QueuedHostedService> logger) : BackgroundService
{
    private readonly IBackgroundTaskQueue _taskQueue = taskQueue;
    private readonly ILogger<QueuedHostedService> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var workItem = await _taskQueue.DequeueAsync(stoppingToken);
            try
            {
                await workItem(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing background work item.");
            }
        }
    }
}