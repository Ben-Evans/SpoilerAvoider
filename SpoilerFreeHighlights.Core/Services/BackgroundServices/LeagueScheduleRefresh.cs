using Microsoft.Extensions.DependencyInjection;

namespace SpoilerFreeHighlights.Core.Services.BackgroundServices;

public static class LeagueScheduleRefresh
{
    public static Task FetchAndCacheScheduledGames(IServiceProvider serviceProvider)
    {
        // Because Background Services are singletons, we must create a new scope to get a scoped service like DbContext.
        using IServiceScope scope = serviceProvider.CreateScope();
        LeaguesService leaguesService = scope.ServiceProvider.GetRequiredService<LeaguesService>();

        return leaguesService.FetchAndCacheScheduledGames();
    }
}
