using Microsoft.Extensions.DependencyInjection;

namespace SpoilerFreeHighlights.Core.Services.BackgroundServices;

public static class LeagueScheduleRefresh
{
    // Changed to async Task to avoid the scope being disposed of prematurely
    public static async Task FetchAndCacheScheduledGames(IServiceProvider serviceProvider)
    {
        // Because Background Services are singletons, we must create a new scope to get a scoped service like DbContext.
        using IServiceScope scope = serviceProvider.CreateScope();
        LeaguesService leaguesService = scope.ServiceProvider.GetRequiredService<LeaguesService>();

        await leaguesService.FetchAndCacheScheduledGames();
    }
}
