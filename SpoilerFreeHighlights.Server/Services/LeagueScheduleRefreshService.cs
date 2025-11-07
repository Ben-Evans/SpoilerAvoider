namespace SpoilerFreeHighlights.Server.Services;

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
                await FetchAndCacheScheduledGames();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to execute {ServiceName}.", nameof(LeagueScheduleRefreshService));
                throw;
            }
        }

        _logger.Information("{ServiceName} complete.", nameof(LeagueScheduleRefreshService));
    }

    private Task FetchAndCacheScheduledGames()
    {
        // Because Background Services are singletons, we must create a new scope to get a scoped service like DbContext.
        using IServiceScope scope = _serviceProvider.CreateScope();
        LeaguesService leaguesService = scope.ServiceProvider.GetRequiredService<LeaguesService>();

        return leaguesService.FetchAndCacheScheduledGames();
    }
}
