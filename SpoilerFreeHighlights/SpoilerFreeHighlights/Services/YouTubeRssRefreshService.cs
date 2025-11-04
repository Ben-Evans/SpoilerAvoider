namespace SpoilerFreeHighlights.Services;

public class YouTubeRssRefreshService(
    IServiceProvider _serviceProvider,
    IConfiguration _configuration) : BackgroundService
{
    private static readonly ILogger _logger = Log.ForContext<YouTubeRssRefreshService>();
    private readonly TimeSpan _pollingInterval = TimeSpan.FromMinutes(_configuration.GetValue("YouTubeRefreshMinutes", 15));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.Information("{ServiceName} service running...", nameof(YouTubeRssRefreshService));

        using PeriodicTimer timer = new(_pollingInterval);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await FetchAndCacheNewVideos();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to execute {ServiceName}.", nameof(YouTubeRssRefreshService));
                throw;
            }
        }

        _logger.Information("{ServiceName} complete.", nameof(YouTubeRssRefreshService));
    }

    private async Task FetchAndCacheNewVideos()
    {
        // Because Background Services are singletons, we must create a new scope to get a scoped service like DbContext.
        using IServiceScope scope = _serviceProvider.CreateScope();
        YouTubeRssService youTubeRssService = scope.ServiceProvider.GetRequiredService<YouTubeRssService>();

        await youTubeRssService.FetchAndCacheNewVideos();

        await youTubeRssService.AddYouTubeLinksToAllMatches();
    }
}
