namespace SpoilerFreeHighlights.Core.Services.LeagueServices;

public class NhlService(HttpClient _httpClient, AppDbContext _dbContext, IConfiguration _configuration)
    : LeagueService(_dbContext, _configuration)
{
    private static readonly ILogger _logger = Log.ForContext<NhlService>();

    public override Leagues League => Leagues.Nhl;

    public override async Task<Schedule?> FetchScheduleForThisWeek(DateOnly date)
    {
        NhlApiSchedule? nhlSchedule = await FetchScheduleDataFromNhlApi(date, _httpClient);
        if (nhlSchedule is null)
        {
            _logger.Error("Failed to fetch NHL schedule data from the NHL API.");
            return default;
        }

        Schedule schedule = ConvertFromNhlApiToUsableModels(nhlSchedule);
        return schedule;
    }

    public static async Task SeedTeams(AppDbContext dbContext, HttpClient httpClient)
    {
        // Trying to pick a likely date to ensure all teams play that week. Might be able to pick any week though.
        DateOnly date = DateOnly.FromDateTime(new DateTime(DateTime.Now.Year, 10, 21));

        NhlApiSchedule? nhlApiSchedule = await FetchScheduleDataFromNhlApi(date, httpClient);
        if (nhlApiSchedule is null)
        {
            _logger.Error("Failed to fetch NHL schedule data from the NHL API. Cannot seed teams.");
            return;
        }

        Schedule schedule = ConvertFromNhlApiToUsableModels(nhlApiSchedule);

        List<Team> teams = new();
        foreach (Game game in schedule.GameDays.SelectMany(gd => gd.Games))
        {
            if (!teams.Any(x => x.Id == game.HomeTeam.Id))
                teams.Add(game.HomeTeam);

            if (!teams.Any(x => x.Id == game.AwayTeam.Id))
                teams.Add(game.AwayTeam);
        }

        Team[] nhlTeams = teams.DistinctBy(x => x.Id).OrderBy(x => x.ToString()).ToArray();

        _logger.Information("Seeding NHL teams into the database...");
        dbContext.Teams.AddRange(nhlTeams);
        await dbContext.SaveChangesAsync();
        _logger.Information("NHL teams seeded successfully.");
    }

    private static Schedule ConvertFromNhlApiToUsableModels(NhlApiSchedule nhlSchedule)
    {
        Schedule schedule = new()
        {
            League = Leagues.Nhl,
            GameDays = nhlSchedule.GameWeek.Select(gw => new GameDay()
            {
                DateLeague = gw.Date,
                Games = gw.Games.Select(game => new Game()
                {
                    Id = game.Id.ToString(),
                    StartDateUtc = game.StartTimeUTC,
                    StartDateLeagueTime = game.StartTimeUTC.ConvertToLeagueDateTime(Leagues.Nhl),
                    HomeTeamId = game.HomeTeam.Id.ToString(),
                    HomeTeam = new Team()
                    {
                        Id = game.HomeTeam.Id.ToString(),
                        Name = game.HomeTeam.CommonName.Default,
                        City = game.HomeTeam.PlaceName.Default,
                        Abbreviation = game.HomeTeam.Abbrev,
                        // API Returns: https://assets.nhle.com/logos/nhl/svg/WSH_secondary_dark.svg
                        // NHL Site Uses: https://assets.nhle.com/logos/nhl/svg/WSH_dark.svg
                        LogoLink = game.HomeTeam.DarkLogo,
                        LeagueId = Leagues.Nhl
                    },
                    AwayTeamId = game.AwayTeam.Id.ToString(),
                    AwayTeam = new Team()
                    {
                        Id = game.AwayTeam.Id.ToString(),
                        Name = game.AwayTeam.CommonName.Default,
                        City = game.AwayTeam.PlaceName.Default,
                        Abbreviation = game.AwayTeam.Abbrev,
                        LogoLink = game.AwayTeam.DarkLogo,
                        LeagueId = Leagues.Nhl
                    },
                    LeagueId = Leagues.Nhl
                })
                .OrderBy(x => x.StartDateLeagueTime)
                .ToList()
            }).ToList()
        };

        return schedule;
    }

    private static Task<NhlApiSchedule?> FetchScheduleDataFromNhlApi(DateOnly date, HttpClient httpClient) => httpClient.FetchContent<NhlApiSchedule?>($"https://api-web.nhle.com/v1/schedule/{date:yyyy-MM-dd}");
}
