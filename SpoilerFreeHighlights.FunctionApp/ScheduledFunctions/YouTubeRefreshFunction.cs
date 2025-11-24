using Microsoft.Azure.Functions.Worker;
using SpoilerFreeHighlights.Core.Services.BackgroundServices;

namespace SpoilerFreeHighlights.FunctionApp.ScheduledFunctions;

public class YouTubeRefreshFunction(IServiceProvider _serviceProvider)
{
    private readonly ILogger _logger = Log.ForContext<YouTubeRefreshFunction>();

    [Function(nameof(YouTubeRefreshFunction))]
    public async Task Run([TimerTrigger("%VideoSyncFrequency%")] TimerInfo myTimer)
    {
        _logger.Information("{ServiceName} service running...", nameof(YouTubeRefreshFunction));
        
        try
        {
            await YouTubeRefresh.FetchAndCacheNewVideos(_serviceProvider, _logger);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to execute {ServiceName}.", nameof(YouTubeRefreshFunction));
            throw;
        }

        _logger.Information("{ServiceName} complete.", nameof(YouTubeRefreshFunction));
    }
}
