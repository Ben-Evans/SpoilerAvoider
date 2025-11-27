using Microsoft.Extensions.DependencyInjection;

namespace SpoilerFreeHighlights.Core.Services.BackgroundServices;

public static class DataCleanup
{
    public static async Task CleanupOldGamesAndVideos(IServiceProvider serviceProvider, IConfiguration configuration, ILogger logger)
    {
        // Because Background Services are singletons, we must create a new scope to get a scoped service like DbContext.
        using IServiceScope scope = serviceProvider.CreateScope();
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        foreach (Leagues league in Leagues.GetAllLeagues())
        {
            DateTime cutoffDate = league.LeagueDateTimeToday.AddDays(-configuration.GetValue("DataCleanupDaysBack", 14));

            logger.Information("Starting cleanup. Deleting games older than: {CutoffDate}", cutoffDate.ToString("yyyy-MM-dd HH:mm"));

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

            logger.Information("Deleted {GameCount} games.", gamesDeleted);

            int videosDeleted = await dbContext.YouTubeVideos
                .Where(x => x.PublishedDateTimeLeague < cutoffDate)
                .ExecuteDeleteAsync();

            logger.Information("Deleted {VideoCount} videos.", videosDeleted);

            logger.Information("Data cleanup completed.");
        }
    }
}
