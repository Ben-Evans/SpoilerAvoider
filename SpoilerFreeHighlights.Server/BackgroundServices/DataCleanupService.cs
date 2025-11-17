namespace SpoilerFreeHighlights.Server.BackgroundServices;

public class DataCleanupService(
    IServiceProvider _serviceProvider,
    IConfiguration _configuration) : BackgroundService
{
    private static readonly ILogger _logger = Log.ForContext<DataCleanupService>();
    private readonly TimeSpan _pollingInterval = TimeSpan.FromDays(_configuration.GetValue("DataCleanupDays", 14));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.Information("{ServiceName} service running...", nameof(DataCleanupService));

        using PeriodicTimer timer = new(_pollingInterval);
        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await CleanupOldGamesAndVideos();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to execute data cleanup.");
                throw;
            }
        }

        _logger.Information("{ServiceName} complete.", nameof(DataCleanupService));
    }

    private async Task CleanupOldGamesAndVideos()
    {
        // Because Background Services are singletons, we must create a new scope to get a scoped service like DbContext.
        using IServiceScope scope = _serviceProvider.CreateScope();
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        foreach (Leagues league in Leagues.GetAllLeagues())
        {
            DateTime cutoffDate = league.LeagueDateTimeToday.AddDays(-_configuration.GetValue("DataCleanupDaysBack", 7));

            _logger.Information("Starting cleanup. Deleting games older than: {CutoffDate}", cutoffDate.ToString("yyyy-MM-dd HH:mm"));

            // TODO: Delete these two statements after testing.
            Game[] gamesToDelete = await dbContext.Games.Where(x => x.LeagueId == league.Value && x.StartDateLeagueTime < cutoffDate).ToArrayAsync();
            
            YouTubePlaylist[] playlists = await dbContext.YouTubePlaylists.ToArrayAsync();
            YouTubeVideo[] videosToDelete = await dbContext.YouTubeVideos
                .Include(x => x.Playlist)
                .Where(x => x.Playlist.LeagueId == league.Value && x.PublishedDateTimeLeague < cutoffDate)
                .ToArrayAsync();

            int gamesDeleted = await dbContext.Games
                .Where(x => x.LeagueId == league.Value && x.StartDateLeagueTime < cutoffDate)
                .ExecuteDeleteAsync();

            int videosDeleted = await dbContext.YouTubeVideos
                .Where(x => x.PublishedDateTimeLeague < cutoffDate)
                .ExecuteDeleteAsync();

            _logger.Information("Data cleanup completed. Deleted {RowsAffected} games.", gamesDeleted);
        }
    }
}
