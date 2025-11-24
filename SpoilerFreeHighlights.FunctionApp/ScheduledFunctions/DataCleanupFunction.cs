using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using SpoilerFreeHighlights.Core.Services.BackgroundServices;

namespace SpoilerFreeHighlights.FunctionApp.ScheduledFunctions;

public class DataCleanupFunction(IServiceProvider _serviceProvider, IConfiguration _configuration)
{
    private readonly ILogger _logger = Log.ForContext<DataCleanupFunction>();

    [Function(nameof(DataCleanupFunction))]
    public async Task Run([TimerTrigger("%CleanupSyncFrequency%")] TimerInfo myTimer)
    {
        _logger.Information("{ServiceName} service running...", nameof(DataCleanupFunction));

        try
        {
            await DataCleanup.CleanupOldGamesAndVideos(_serviceProvider, _configuration, _logger);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to execute data cleanup.");
            throw;
        }

        _logger.Information("{ServiceName} complete.", nameof(DataCleanupFunction));
    }
}
