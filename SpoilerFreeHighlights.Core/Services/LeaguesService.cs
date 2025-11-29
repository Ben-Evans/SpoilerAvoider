namespace SpoilerFreeHighlights.Core.Services;

public class LeaguesService(
    NhlService _nhlService,
    MlbService _mlbService,
    CflService _cflService,
    AppDbContext _dbContext,
    IConfiguration _configuration)
{
    private static readonly ILogger _logger = Log.ForContext<LeaguesService>();

    /// <summary>
    /// For each league:
    /// - Adding today plus everyday until the end of the week if available.
    /// - Cleanup any invalid games that no longer exist. ie. Hyphothetical games that have been corrected. -- perform here or keep cleanup service?
    /// </summary>
    public async Task FetchAndCacheScheduledGames()
    {
        Dictionary<Leagues, LeagueService> services = new()
        {
            { Leagues.Nhl, _nhlService },
            { Leagues.Mlb, _mlbService },
            { Leagues.Cfl, _cflService }
        };

        List<Schedule> schedules = [];
        foreach (Leagues league in services.Keys)
        {
            LeagueService leagueService = services[league];
            DateOnly leagueToday = league.LeagueDateToday;
            Schedule? leagueSchedule = await leagueService.FetchScheduleForThisWeek(leagueToday);
            if (leagueSchedule is null)
            {
                _logger.Error("Failed to retrieve data from external API '{LeagueName}'.", league.DisplayName);
                continue;
            }

            DateOnly fetchDaysBack = league.LeagueDateToday.AddDays(-_configuration.GetValue<int>("FetchDaysBack"));
            leagueSchedule.GameDays = leagueSchedule.GameDays
                .Where(x => x.DateLeague >= fetchDaysBack)
                .ToList();

            _logger.Debug("Fetched schedule data for {ScheduleSummary}.", leagueSchedule.ToString());
            schedules.Add(leagueSchedule);
        }

        Game[] allFetchedGames = schedules.SelectMany(x => x.GameDays.SelectMany(y => y.Games)).ToArray();
        string[] fetchedGameIds = allFetchedGames.Select(x => x.Id).ToArray();

        // Set teams to null to comply with EF model references during saving.
        foreach (Game game in allFetchedGames)
        {
            game.HomeTeam = null;
            game.AwayTeam = null;
        }

        string[] existingIds = await _dbContext.Games
            .Where(x => fetchedGameIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToArrayAsync();

        // TODO: Also delete old games that may have change, like hyphothetical games that have been corrected
        Game[] newGames = allFetchedGames
            .Where(x => !existingIds.Contains(x.Id))
            .ToArray();
        if (newGames.Any())
        {
            _logger.Information("Adding '{GameCount}' new games to DB.", newGames.Length);

            _dbContext.Games.AddRange(newGames);
            await _dbContext.SaveChangesAsync();
        }
    }
}

public abstract class LeagueService(AppDbContext _dbContext, IConfiguration _configuration)
{
    public abstract Leagues League { get; }

    public abstract Task<Schedule?> FetchScheduleForThisWeek(DateOnly date);

    public async Task<Schedule?> GetScheduleForThisWeek(UserPreference userPreference)
    {
        // Limit which days get displayed.
        DateTime daysForward = League.LeagueDateTimeToday.AddDays(_configuration.GetValue<int>("DisplayDaysForward"));
        DateTime daysBack = League.LeagueDateTimeToday.AddDays(-_configuration.GetValue<int>("DisplayDaysBack"));

        string[] preferredTeamIds = userPreference.LeaguePreferences[League].Select(team => team.Id).ToArray();

        // In order to avoid using .Date on the comparison is using daysForwardPlusOne a better approach?
        DateTime daysForwardPlusOne = daysForward.AddDays(1);

        // TODO: For reduced DB load prevent using .Date on x.StartDateLeagueTime.Date?
        // This is also hit once for every league on a Home page load, is there a better way to consolidate this into less DB calls? (LeagueDateTimeToday limiting possibilites)
        IQueryable<Game> gamesQuery = _dbContext.Games
            .Include(x => x.HomeTeam)
            .Include(x => x.AwayTeam)
            .Where(x => x.LeagueId == League
                // && x.StartDateLeagueTime.Date <= daysForward && x.StartDateLeagueTime.Date >= daysBack)
                && x.StartDateLeagueTime <= daysForwardPlusOne && x.StartDateLeagueTime > daysBack);

        if (preferredTeamIds.Any())
            gamesQuery = gamesQuery.Where(x => preferredTeamIds.Contains(x.HomeTeamId) || preferredTeamIds.Contains(x.AwayTeamId));

        Game[] games = await gamesQuery.ToArrayAsync();

        GameDay[] gameDays = games
            .GroupBy(x => x.StartDateLeagueTime.Date)
            .Select(x => new GameDay()
            {
                DateLeague = DateOnly.FromDateTime(x.Key),
                Games = x.OrderBy(y => y.StartDateLeagueTime).ToList()
            })
            .OrderByDescending(x => x.DateLeague)
            .ToArray();

        Schedule schedule = new()
        {
            League = League,
            GameDays = gameDays.Where(x => x.Games.Any()).ToList()
        };
        return schedule;
    }
}
