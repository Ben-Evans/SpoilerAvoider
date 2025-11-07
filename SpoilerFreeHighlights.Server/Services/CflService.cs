using Ical.Net;
using Ical.Net.CalendarComponents;

namespace SpoilerFreeHighlights.Server.Services;

public class CflService(HttpClient _httpClient, AppDbContext _dbContext, IConfiguration _configuration)
    : LeagueService(_dbContext, _configuration)
{
    private static readonly ILogger _logger = Log.ForContext<CflService>();
    // TODO: Does this need to be refreshed every year?
    // webcal://ics.ecal.com/ecal-sub/11a1a1a111111a1111111111/CFL.ics => https://ics.ecal.com/ecal-sub/11a1a1a111111a1111111111/CFL.ics
    private readonly string _icsUrl = _configuration.GetValue("CflIcalLink", string.Empty).Replace("webcal://", "https://");

    public override Leagues League => Leagues.Cfl;

    public override async Task<Schedule?> FetchScheduleForThisWeek(DateOnly date)
    {
        /*MlbApiSchedule? mlbSchedule = await FetchScheduleDataFromMlbApi(date);
        Schedule schedule = await ConvertFromMlbApiToUsableModels(mlbSchedule);
        return schedule;*/

        _logger.Information("Fetching schedule from: {Url}.", _icsUrl);

        string icsContent;
        try
        {
            icsContent = await _httpClient.GetStringAsync(_icsUrl);
        }
        catch (HttpRequestException ex)
        {
            _logger.Error("Error fetching CFL ICS URL: {Exception}", ex);
            return default;
        }

        Calendar? calendar = Calendar.Load(icsContent);

        Schedule schedule = new();
        foreach (IGrouping<DateOnly, CalendarEvent> dateEvents in calendar.Events.GroupBy(x => x.Start.Date).OrderBy(x => x.Key))
        {
            GameDay gameDay = new()
            {
                DateLeague = dateEvents.Key
            };
            foreach (CalendarEvent calendarEvent in dateEvents)
            {
                // 🏈 Calgary Stampeders @ Edmonton Elks -- Away @ Home
                string matchup = calendarEvent.Summary;
                if (!matchup.Contains("@"))
                {
                    _logger.Information("Skipping non-matchup CFL event: '{EventSummary}'.", matchup);
                    continue;
                }

                string[] teams = matchup.Split("🏈 ")[1].Split(" @ ");
                Team homeTeam = await GetTeamByFullName(teams[1]);
                Team awayTeam = await GetTeamByFullName(teams[0]);

                Game game = new()
                {
                    Id = calendarEvent.Uid.ToString(),
                    StartDateLeagueTime = calendarEvent.Start.Value.ConvertToLeagueDateTime(Leagues.Cfl),
                    StartDateUtc = calendarEvent.Start.Value,
                    HomeTeamId = homeTeam.Id,
                    HomeTeam = homeTeam,
                    AwayTeamId = awayTeam.Id,
                    AwayTeam = awayTeam,
                    LeagueId = Leagues.Cfl
                };
                gameDay.Games.Add(game);
            }
            
            schedule.GameDays.Add(gameDay);
        }

        schedule.GameDays.ForEach(gd => gd.Games = gd.Games.OrderBy(g => g.StartDateLeagueTime).ToList());

        schedule.GameDays = schedule.GameDays
            .Where(x => x.Games.Any())
            .OrderBy(x => x.DateLeague)
            .ToList();

        return schedule;
    }

    public static async Task SeedTeams(AppDbContext dbContext)
    {
        string logoLink = $"/Resources/Team-Logos{Leagues.Cfl.Name}/Originals";
        Team[] cflTeams =
        [
            // WEST DIVISION
            new Team
            {
                Id = Guid.NewGuid().ToString(),
                LeagueId = Leagues.Cfl,
                Name = "Stampeders",
                City = "Calgary",
                Abbreviation = "CGY",
                LogoLink = $"{logoLink}/CGY.svg"
            },
            new Team
            {
                Id = Guid.NewGuid().ToString(),
                LeagueId = Leagues.Cfl,
                Name = "Elks",
                City = "Edmonton",
                Abbreviation = "EDM",
                LogoLink = $"{logoLink}/EDM.svg"
            },
            new Team
            {
                Id = Guid.NewGuid().ToString(),
                LeagueId = Leagues.Cfl,
                Name = "Lions",
                City = "BC",
                Abbreviation = "BC",
                LogoLink = $"{logoLink}/BC.svg"
            },
            new Team
            {
                Id = Guid.NewGuid().ToString(),
                LeagueId = Leagues.Cfl,
                Name = "Blue Bombers",
                City = "Winnipeg",
                Abbreviation = "WPG",
                LogoLink = $"{logoLink}/WPG.svg"
            },
            new Team
            {
                Id = Guid.NewGuid().ToString(),
                LeagueId = Leagues.Cfl,
                Name = "Roughriders",
                City = "Saskatchewan",
                Abbreviation = "SSK",
                LogoLink = $"{logoLink}/SSK.svg"
            },

            // EAST DIVISION
            new Team
            {
                Id = Guid.NewGuid().ToString(),
                LeagueId = Leagues.Cfl,
                Name = "Tiger-Cats",
                City = "Hamilton",
                Abbreviation = "HAM",
                LogoLink = $"{logoLink}/HAM.svg"
            },
            new Team
            {
                Id = Guid.NewGuid().ToString(),
                LeagueId = Leagues.Cfl,
                Name = "Alouettes",
                City = "Montreal",
                Abbreviation = "MTL",
                LogoLink = $"{logoLink}/MTL.svg"
            },
            new Team
            {
                Id = Guid.NewGuid().ToString(),
                LeagueId = Leagues.Cfl,
                Name = "Redblacks",
                City = "Ottawa",
                Abbreviation = "OTT",
                LogoLink = $"{logoLink}/OTT.svg"
            },
            new Team
            {
                Id = Guid.NewGuid().ToString(),
                LeagueId = Leagues.Cfl,
                Name = "Argonauts",
                City = "Toronto",
                Abbreviation = "TOR",
                LogoLink = $"{logoLink}/TOR.svg"
            }
        ];

        _logger.Information("Seeding CFL teams into the database...");

        dbContext.Teams.AddRange(cflTeams.OrderBy(x => x.ToString()));
        await dbContext.SaveChangesAsync();

        _logger.Information("CFL teams seeded successfully.");
    }

    //private Task<Team> GetTeamByFullName(string fullTeamName) => _dbContext.Teams.FirstOrDefaultAsync(t => t.LeagueId == Leagues.Cfl && t.City + " " + t.Name == fullTeamName);
    private Task<Team> GetTeamByFullName(string fullTeamName)
    {
        // Failed due to "Hamilton Tiger Cats" from schedule not matching officially named "Hamilton Tiger-Cats"
        if (fullTeamName == "Hamilton Tiger Cats")
            fullTeamName = "Hamilton Tiger-Cats";

        return _dbContext.Teams.FirstOrDefaultAsync(t => t.LeagueId == Leagues.Cfl && t.City + " " + t.Name == fullTeamName);
    }
}
