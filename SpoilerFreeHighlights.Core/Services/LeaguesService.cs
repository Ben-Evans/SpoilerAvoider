namespace SpoilerFreeHighlights.Core.Services;

public class LeaguesService(
    NhlService _nhlService,
    MlbService _mlbService,
    CflService _cflService,
    AppDbContext _dbContext)
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

        List<Schedule> schedules = new();
        foreach (Leagues league in services.Keys)
        {
            LeagueService leagueService = services[league];
            DateOnly leagueToday = league.LeagueDateToday;
            Schedule? leagueSchedule = await leagueService.FetchScheduleForThisWeek(leagueToday);
            if (leagueSchedule is null)
            {
                _logger.Error("Failed to retrieve data from external API '{LeagueName}'.", league.Name);
                continue;
            }

            _logger.Debug("Fetched schedule data for {ScheduleSummary}.", leagueSchedule);
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

        Game[] games = await _dbContext.Games
            .Include(x => x.HomeTeam)
            .Include(x => x.AwayTeam)
            .Where(x => x.LeagueId == League
                && x.StartDateLeagueTime.Date <= daysForward && x.StartDateLeagueTime.Date >= daysBack)
            .ToArrayAsync();

        GameDay[] gameDays = games
            .GroupBy(x => x.StartDateLeagueTime.Date)
            .Select(x => new GameDay()
            {
                DateLeague = DateOnly.FromDateTime(x.Key),
                Games = x.OrderBy(y => y.StartDateLeagueTime).ToList()
            })
            .OrderByDescending(x => x.DateLeague)
            .ToArray();

        string[] preferredTeamIds = userPreference.LeaguePreferences[League].Select(team => team.Id).ToArray();
        if (preferredTeamIds.Any())
            foreach (GameDay gameDay in gameDays)
                gameDay.Games = gameDay.Games.Where(x => preferredTeamIds.Any(y => y == x.HomeTeam.Id || y == x.AwayTeam.Id)).ToList();

        Schedule schedule = new()
        {
            League = League,
            GameDays = gameDays.Where(x => x.Games.Any()).ToList()
        };
        return schedule;
    }
}
