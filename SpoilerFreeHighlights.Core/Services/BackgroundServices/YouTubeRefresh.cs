using Microsoft.Extensions.DependencyInjection;

namespace SpoilerFreeHighlights.Core.Services.BackgroundServices;

public static class YouTubeRefresh
{
    public static async Task FetchAndCacheNewVideos(IServiceProvider serviceProvider, ILogger logger)
    {
        // Because Background Services are singletons, we must create a new scope to get a scoped service like DbContext.
        using IServiceScope scope = serviceProvider.CreateScope();
        YouTubeService youtubeService = scope.ServiceProvider.GetRequiredService<YouTubeService>();

        logger.Information("Checking for new videos...");
        bool newVideos = await youtubeService.FetchAndCacheNewVideos();
        logger.Information("Checking for new videos complete.");

        if (newVideos)
        {
            logger.Information("Attempting to add new links to matchups...");
            await youtubeService.AddYouTubeLinksToAllMatches();
            logger.Information("Attempting to add new links to matchups complete.");
        }
    }
}
