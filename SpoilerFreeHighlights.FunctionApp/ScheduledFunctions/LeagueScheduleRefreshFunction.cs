using Microsoft.Azure.Functions.Worker;
using SpoilerFreeHighlights.Core.Services.BackgroundServices;

namespace SpoilerFreeHighlights.FunctionApp.ScheduledFunctions;

public class LeagueScheduleRefreshFunction(IServiceProvider _serviceProvider)
{
    private readonly ILogger _logger = Log.ForContext<LeagueScheduleRefreshFunction>();

    [Function(nameof(LeagueScheduleRefreshFunction))]
    public async Task Run([TimerTrigger("%ScheduleSyncFrequency%")] TimerInfo myTimer)
    {
        _logger.Information("{ServiceName} service running...", nameof(LeagueScheduleRefreshFunction));

        try
        {
            await LeagueScheduleRefresh.FetchAndCacheScheduledGames(_serviceProvider);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to execute {ServiceName}.", nameof(LeagueScheduleRefreshFunction));
            throw;
        }

        _logger.Information("{ServiceName} complete.", nameof(LeagueScheduleRefreshFunction));
    }
}
