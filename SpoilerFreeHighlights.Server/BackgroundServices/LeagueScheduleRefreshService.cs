using SpoilerFreeHighlights.Core.Services.BackgroundServices;

namespace SpoilerFreeHighlights.Server.BackgroundServices;

public class LeagueScheduleRefreshService(
    IServiceProvider _serviceProvider,
    IConfiguration _configuration) : BackgroundService
{
    private static readonly ILogger _logger = Log.ForContext<LeagueScheduleRefreshService>();
    private readonly TimeSpan _pollingInterval = TimeSpan.FromHours(_configuration.GetValue("ScheduleRefreshHours", 12));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.Information("{ServiceName} service running...", nameof(LeagueScheduleRefreshService));

        using PeriodicTimer timer = new(_pollingInterval);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await LeagueScheduleRefresh.FetchAndCacheScheduledGames(_serviceProvider);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to execute {ServiceName}.", nameof(LeagueScheduleRefreshService));
                throw;
            }
        }

        _logger.Information("{ServiceName} complete.", nameof(LeagueScheduleRefreshService));
    }
}
