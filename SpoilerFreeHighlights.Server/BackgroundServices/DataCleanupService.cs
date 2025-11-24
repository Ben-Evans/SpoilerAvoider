using SpoilerFreeHighlights.Core.Services.BackgroundServices;

namespace SpoilerFreeHighlights.Server.BackgroundServices;

public class DataCleanupService(
    IServiceProvider _serviceProvider,
    IConfiguration _configuration) : BackgroundService
{
    private static readonly ILogger _logger = Log.ForContext<DataCleanupService>();
    private readonly TimeSpan _pollingInterval = TimeSpan.FromDays(_configuration.GetValue("DataCleanupDaysBack", 14));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.Information("{ServiceName} service running...", nameof(DataCleanupService));

        using PeriodicTimer timer = new(_pollingInterval);
        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await DataCleanup.CleanupOldGamesAndVideos(_serviceProvider, _configuration, _logger);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to execute data cleanup.");
                throw;
            }
        }

        _logger.Information("{ServiceName} complete.", nameof(DataCleanupService));
    }
}
