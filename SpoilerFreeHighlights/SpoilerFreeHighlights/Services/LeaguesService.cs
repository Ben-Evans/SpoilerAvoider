namespace SpoilerFreeHighlights.Services;

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
        Game[] onlyNew = allFetchedGames
            .Where(x => !existingIds.Contains(x.Id))
            .ToArray();
        if (onlyNew.Any())
        {
            _logger.Information("Adding '{GameCount}' new games to DB.", onlyNew.Length);
            foreach (Game newGame in onlyNew)
            {
                _logger.Information("Adding game: '{GameInfo}'.", new { newGame.Id, Date = newGame.StartDateLeagueTime.ToString("yyyy-MM-dd HH:mm"), newGame.HomeTeamId, newGame.AwayTeamId });

                try
                {
                    _dbContext.Games.Add(newGame);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Couldn't add");
                }

                try
                {
                    await _dbContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Couldn't save");
                }
            }

            //_dbContext.Games.AddRange(onlyNew);
            //await _dbContext.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Ensures that all related Team entities are marked as Unchanged, 
    /// preventing EF Core from trying to insert them as duplicates.
    /// </summary>
    private void MarkRelatedTeamsAsUnchanged(AppDbContext dbContext, IEnumerable<Game> games)
    {
        // Use a HashSet to track which unique Team entities we've processed, 
        // to avoid trying to set the state of the same entity key multiple times.
        var uniqueTeams = new HashSet<Team>();

        foreach (var game in games)
        {
            if (game.HomeTeam != null)
            {
                uniqueTeams.Add(game.HomeTeam);
            }
            if (game.AwayTeam != null)
            {
                uniqueTeams.Add(game.AwayTeam);
            }
        }

        foreach (var team in uniqueTeams)
        {
            // Check if the entity is already being tracked. This avoids the InvalidOperationException.
            var existingEntry = dbContext.Entry(team);

            // If the entity is not already tracked (or is in the 'Added' state from a prior operation, 
            // though that's less likely here), we can safely attach it.
            if (existingEntry.State == EntityState.Detached)
            {
                // Attach the entity to the DbContext, telling EF Core that this 
                // entity is expected to exist in the database (Unchanged state).
                dbContext.Teams.Attach(team);
            }
            // Note: If it's already tracked (e.g., EntityState.Unchanged), we do nothing.
            // If it's EntityState.Added, we might have a logic issue, but Attach will handle 
            // Detached and Unchanged gracefully.
        }
    }
}

public abstract class LeagueService(AppDbContext _dbContext, IConfiguration _configuration)
{
    public abstract Leagues League { get; }

    public abstract Task<Schedule?> FetchScheduleForThisWeek(DateOnly date);

    public async Task<Schedule?> GetScheduleForThisWeek(UserPreference userPreference)
    {
        // Limit whih days get displayed.
        DateTime daysForward = League.LeagueDateTimeToday.AddDays(_configuration.GetValue<int>("DisplayDaysForward"));
        DateTime daysBack = League.LeagueDateTimeToday.AddDays(-_configuration.GetValue<int>("DisplayDaysBack"));

        Game[] games = await _dbContext.Games
            .Include(x => x.HomeTeam)
            .Include(x => x.AwayTeam)
            .Where(x => x.LeagueId == League && x.StartDateLeagueTime.Date <= daysForward && x.StartDateLeagueTime.Date >= daysBack)
            .ToArrayAsync();

        GameDay[] gameDays = games
            .GroupBy(x => x.StartDateLeagueTime.Date)
            .Select(x => new GameDay()
            {
                Date = DateOnly.FromDateTime(x.Key),
                Games = x.OrderBy(y => y.StartDateLeagueTime).ToList()
            })
            .OrderByDescending(x => x.Date)
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
