namespace SpoilerFreeHighlights.Core.Endpoints;

public static class AllEndpoints
{
    public static Task<Team[]> GetTeams(AppDbContext dbContext)
    {
        return dbContext.Teams.ToArrayAsync();
    }

    public static async Task<Schedule?> GetGameDays(
        NhlService nhlService,
        MlbService mlbService,
        CflService cflService,
        ScheduleQuery scheduleQuery)
    {
        Dictionary<Leagues, LeagueService> services = new()
        {
            { Leagues.Nhl, nhlService },
            { Leagues.Mlb, mlbService },
            { Leagues.Cfl, cflService }
        };

        if (scheduleQuery.Leagues is null || scheduleQuery.Leagues.Length == 0 || scheduleQuery.Leagues.Contains(Leagues.All))
            scheduleQuery.Leagues = services.Keys.ToArray();

        List<Schedule> schedules = [];
        foreach (Leagues league in scheduleQuery.Leagues)
        {
            LeagueService leagueService = services[league];

            Schedule? leagueSchedule = await leagueService.GetScheduleForThisWeek(scheduleQuery.UserPreferences);
            if (leagueSchedule is null)
                return default;

            schedules.Add(leagueSchedule);
        }

        Schedule allSchedule = new()
        {
            League = Leagues.All,
            GameDays = schedules
                .SelectMany(x => x.GameDays)
                .GroupBy(x => x.DateLeague)
                .Select(x => new GameDay
                {
                    DateLeague = x.Key,
                    Games = x.SelectMany(y => y.Games).OrderBy(y => y.StartDateUtc).ToList()
                })
                .ToList()
        };

        return allSchedule;
    }

    public static async Task TestScheduleService(LeaguesService leaguesService)
    {
        await leaguesService.FetchAndCacheScheduledGames();
    }

    public static async Task TestYouTubeService(YouTubeRssService youTubeRssService)
    {
        await youTubeRssService.FetchAndCacheNewVideos();

        await youTubeRssService.AddYouTubeLinksToAllMatches();
    }

    public static async Task ResetData(AppDbContext dbContext)
    {
        await dbContext.YouTubeVideos.ExecuteDeleteAsync();
        await dbContext.YouTubePlaylists.ExecuteDeleteAsync();
        await dbContext.Games.ExecuteDeleteAsync();
        //await dbContext.Teams.ExecuteDeleteAsync();
        //await dbContext.Leagues.ExecuteDeleteAsync();
        await dbContext.SaveChangesAsync();
    }
}
