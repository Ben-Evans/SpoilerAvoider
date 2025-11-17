namespace SpoilerFreeHighlights.Server.BackgroundServices;

public class YouTubeRssRefreshService(
    IServiceProvider _serviceProvider,
    IConfiguration _configuration) : BackgroundService
{
    private static readonly ILogger _logger = Log.ForContext<YouTubeRssRefreshService>();

    private readonly int[] _scheduledMinutes = _configuration.GetSection("YouTubeSchedulerMinuteIntervals").Get<int[]>();
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.Information("{ServiceName} service running...", nameof(YouTubeRssRefreshService));

        while (!stoppingToken.IsCancellationRequested)
        {
            TimeSpan delay = CalculateDelayToNextExecution(); // ex. {00:13:44.9366804}
            using PeriodicTimer timer = new(delay);

            if (!await timer.WaitForNextTickAsync(stoppingToken))
                break;

            try
            {
                await FetchAndCacheNewVideos();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to execute {ServiceName}.", nameof(YouTubeRssRefreshService));
                //throw;
            }
        }

        _logger.Information("{ServiceName} complete.", nameof(YouTubeRssRefreshService));
    }

    private async Task FetchAndCacheNewVideos()
    {
        // Because Background Services are singletons, we must create a new scope to get a scoped service like DbContext.
        using IServiceScope scope = _serviceProvider.CreateScope();
        YouTubeRssService youTubeRssService = scope.ServiceProvider.GetRequiredService<YouTubeRssService>();

        _logger.Information("Checking for new videos...");
        bool newVideos = await youTubeRssService.FetchAndCacheNewVideos();
        _logger.Information("Checking for new videos complete.");

        if (newVideos)
        {
            _logger.Information("Attempting to add new links to matchups...");
            await youTubeRssService.AddYouTubeLinksToAllMatches();
            _logger.Information("Attempting to add new links to matchups complete.");
        }
    }

    /// <summary>
    /// Calculates the time difference until the next 00, 15, 30, or 45 minute mark.
    /// 
    /// Check to see if we need to check for new videos. ie. Are any games even missing highlights?
    /// Using estimated end time for a game to try to calculate when we might want to check even more frequently?
    /// </summary>
    private TimeSpan CalculateDelayToNextExecution()
    {
        DateTime now = DateTime.UtcNow;
        int currentMinute = now.Minute;

        // Find the next scheduled minute
        int nextMinute = _scheduledMinutes
            .OrderBy(x => x)
            .FirstOrDefault(x => x > currentMinute);

        // (Ran at 16:55 MST) System.ArgumentOutOfRangeException: 'Hour, Minute, and Second parameters describe an un-representable DateTime.'
        DateTime nextRun = nextMinute > 0
            // 2025, 11, 6, 23 + 1, 0, 0, DateTimeKind.Utc
            ? new DateTime(now.Year, now.Month, now.Day, now.Hour, nextMinute, 0, DateTimeKind.Utc)
            : new DateTime(now.Year, now.Month, now.Day, now.Hour + 1, _scheduledMinutes.Min(), 0, DateTimeKind.Utc);

        return nextRun - now;
    }
}
