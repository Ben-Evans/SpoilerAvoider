namespace SpoilerFreeHighlights.Server.Services;

public class MlbService(HttpClient _httpClient, AppDbContext _dbContext, IConfiguration _configuration)
    : LeagueService(_dbContext, _configuration)
{
    private static readonly ILogger _logger = Log.ForContext<MlbService>();

    public override Leagues League => Leagues.Mlb;

    public override async Task<Schedule?> FetchScheduleForThisWeek(DateOnly date)
    {
        MlbApiSchedule? mlbSchedule = await FetchScheduleDataFromMlbApi(date);
        Schedule schedule = await ConvertFromMlbApiToUsableModels(mlbSchedule);
        return schedule;
    }

    public async Task<Schedule> ConvertFromMlbApiToUsableModels(MlbApiSchedule mlbSchedule)
    {
        Schedule schedule = new()
        {
            League = Leagues.Mlb
        };
        foreach (MlbApiDate mlbDate in mlbSchedule.Dates)
        {
            GameDay gameDay = new()
            {
                DateLeague = DateOnly.Parse(mlbDate.Date)
            };

            foreach (MlbApiGame mlbGame in mlbDate.Games)
            {
                string[] homeTeams = mlbGame.Teams.Home.Team.Name.Split('/');
                string[] awayTeams = mlbGame.Teams.Away.Team.Name.Split('/');

                bool isHypotheticalGame = homeTeams.Length > 1 || awayTeams.Length > 1;
                if (isHypotheticalGame) // Call GetTeamByAbbreviation
                    continue; // ~ _configuration.AllowHypotheticGames or do add them, but use userPreferences.AllowHypotheticGames to hide them

                string homeTeamId = mlbGame.Teams.Home.Team.Id.ToString();
                string awayTeamId = mlbGame.Teams.Away.Team.Id.ToString();
                Game game = new()
                {
                    Id = mlbGame.GameGuid, // GamePk
                    StartDateLeagueTime = mlbGame.GameDate.ConvertToLeagueDateTime(Leagues.Mlb),
                    StartDateUtc = mlbGame.GameDate,
                    HomeTeamId = homeTeamId,
                    HomeTeam = await GetTeamById(homeTeamId),
                    AwayTeamId = awayTeamId,
                    AwayTeam = await GetTeamById(awayTeamId),
                    IsHypothetical = isHypotheticalGame,
                    LeagueId = Leagues.Mlb
                };

                gameDay.Games.Add(game);
            }

            gameDay.Games = gameDay.Games.OrderBy(x => x.StartDateLeagueTime).ToList();

            schedule.GameDays.Add(gameDay);
        }

        return schedule;
    }

    public static async Task SeedTeams(AppDbContext dbContext, HttpClient httpClient)
    {
        MlbApiTeamResponse? mlbApiTeams = await FetchTeamDataFromMlbApi(httpClient);

        Team[] mlbTeams = mlbApiTeams.Teams
            .Select(x => new Team
            {
                Id = x.Id.ToString(),
                LeagueId = Leagues.Mlb,
                Name = x.TeamName,
                City = x.FranchiseName, // x.LocationName,
                Abbreviation = x.Abbreviation,
                LogoLink = $"https://www.mlbstatic.com/team-logos/{x.Id}.svg"
            })
            .OrderBy(x => x.ToString())
            .ToArray();

        _logger.Information("Seeding MLB teams into the database...");
        dbContext.Teams.AddRange(mlbTeams);
        dbContext.SaveChanges();
        _logger.Information("MLB teams seeded successfully.");
    }

    private Task<MlbApiSchedule?> FetchScheduleDataFromMlbApi(DateOnly startDate, DateOnly? endDate = default) => _httpClient.FetchContent<MlbApiSchedule?>($"https://statsapi.mlb.com/api/v1/schedule?sportId=1&startDate={startDate:yyyy-MM-dd}&endDate={endDate ?? startDate.AddDays(7):yyyy-MM-dd}/");

    private static Task<MlbApiTeamResponse?> FetchTeamDataFromMlbApi(HttpClient httpClient) => httpClient.FetchContent<MlbApiTeamResponse?>("https://statsapi.mlb.com/api/v1/teams?sportId=1");

    private Task<Team> GetTeamById(string id) => _dbContext.Teams.FirstOrDefaultAsync(x => x.LeagueId == Leagues.Mlb && x.Id == id);

    //private Task<Team> GetTeamByAbbreviation(string teamAbbreviation) => _dbContext.Teams.FirstOrDefaultAsync(x => x.LeagueId == Leagues.Mlb && x.Abbreviation == teamAbbreviation);
}
