namespace SpoilerFreeHighlights.Core.Services;

/// <summary>
/// This evening is 2025-10-26 @ 22:43 MTN, 2026-10-27 00:43 EST
/// </summary>
public static class FakeDataService
{
    /// <summary>
    /// This evening is 2025-10-26 @ 22:43 MTN, 2026-10-27 00:43 EST
    /// </summary>
    public static FakeData GetFakeData()
    {
        // -------------------------- This evening is 2025-10-26 @ 22:43 MTN, 2026-10-27 00:43 EST --------------------------

        // https://www.youtube.com/feeds/videos.xml?playlist_id=PLo12SYwt93SQP81ntu8rz9QcAHTNQaFI3
        YouTubePlaylist playlist = new()
        {
            Id = "PLo12SYwt93SQP81ntu8rz9QcAHTNQaFI3",
            Name = "2025-26 NHL Highlights, News and Analysis",
            ChannelName = "@sportsnet",
            LeagueId = Leagues.Nhl.Value,
            Videos = new()
            {
                GetFakeVideo(3),
                GetFakeVideo(3),
                GetFakeVideo(3),
                GetFakeVideo(3),
                GetFakeVideo(2),
                GetFakeVideo(2),
                GetFakeVideo(2),
                GetFakeVideo(1),
                GetFakeVideo(1),
                GetFakeVideo(1),

                // Id: 'zDa1szi7IAA', Publish Date: '2025-10-27T03:12:16+00:00' && Title: "NHL Highlights | Rangers vs. Flames - October 26, 2025"
                new YouTubeVideo("A", $"NHL Highlights | Lightning vs. Golden Knights - October 26, 2025", FakeData.VideoUtc, FakeData.VideoLeagueTime, "https://www.youtube.com/watch?v=LkiHNFV24kE"),
                new YouTubeVideo("B", $"Edmonton Oilers at Vancouver Canucks | FULL Overtime Highlights - October 26, 2025", FakeData.VideoUtc, FakeData.VideoLeagueTime, "https://www.youtube.com/watch?v=J_5zIkngE_o"),
                new YouTubeVideo("C", $"NHL Highlights | Oilers vs. Canucks - October 26, 2025", FakeData.VideoUtc, FakeData.VideoLeagueTime, "https://www.youtube.com/watch?v=LIPCNE01tDw"),
                new YouTubeVideo("D", $"Flames' Yegor Sharangovich Squeaks One Through Igor Shesterkin", FakeData.VideoUtc, FakeData.VideoLeagueTime, "https://www.youtube.com/watch?v=nItvs3Ag-t8"),
                new YouTubeVideo("E", $"NHL Highlights | Rangers vs. Flames - October 26, 2025", FakeData.VideoUtc, FakeData.VideoLeagueTime, "https://www.youtube.com/watch?v=zDa1szi7IAA"),
                new YouTubeVideo("F", $"NHL Highlights | Stars vs. Predators - October 26, 2025", FakeData.VideoUtc, FakeData.VideoLeagueTime, "https://www.youtube.com/watch?v=bpR3yM70PHo"),
                new YouTubeVideo("G", $"NHL Highlights | Sharks vs. Wild - October 26, 2025", FakeData.VideoUtc, FakeData.VideoLeagueTime, "https://www.youtube.com/watch?v=EiY38vqX35o"),
                new YouTubeVideo("H", $"NHL Highlights | Mammoth vs. Jets - October 26, 2025", FakeData.VideoUtc, FakeData.VideoLeagueTime, "https://www.youtube.com/watch?v=45SFCCjVZNY"),
                new YouTubeVideo("I", $"NHL Highlights | Oilers vs. Kraken - October 25, 2025", FakeData.VideoUtc.AddDays(-1), FakeData.VideoLeagueTime.AddDays(-1), "https://www.youtube.com/watch?v=Ope-kL1DS-M"),
                new YouTubeVideo("I", $"NHL Highlights | Sabres vs. Maple Leafs - October 25, 2025", FakeData.VideoUtc.AddDays(-1), FakeData.VideoLeagueTime.AddDays(-1), "https://www.youtube.com/watch?v=Wpllj0cPkjw"),

                GetFakeVideo(-2),
                GetFakeVideo(-2),
                GetFakeVideo(-2),
                GetFakeVideo(-3),
                GetFakeVideo(-3),
                GetFakeVideo(-4),
                GetFakeVideo(-4),
                GetFakeVideo(-4),
                GetFakeVideo(-4),
                GetFakeVideo(-4),
                GetFakeVideo(-4),
                GetFakeVideo(-4),
                //GetFakeVideo(-5),
                GetFakeVideo(-6),
                GetFakeVideo(-6),
                GetFakeVideo(-6),
                GetFakeVideo(-6),
                GetFakeVideo(-6),
                GetFakeVideo(-7),
                GetFakeVideo(-7),
                GetFakeVideo(-7),
                GetFakeVideo(-8),
                GetFakeVideo(-8),
                GetFakeVideo(-8),
                GetFakeVideo(-8),
                GetFakeVideo(-9),
                GetFakeVideo(-10),
                GetFakeVideo(-11),
                GetFakeVideo(-12),
                GetFakeVideo(-13),
                GetFakeVideo(-14),
                GetFakeVideo(-15),
            }
        };

        Schedule allSchedule = new() { League = Leagues.Nhl }; // LocalToday = userDateTime
        for (int i = -7; i < 21; i++)
        {
            DateTime utcDateTime = FakeData.FakeUtcNow.AddDays(i);
            DateTime leagueDateTime = utcDateTime.ConvertToLeagueDateTime(allSchedule.League);

            allSchedule.GameDays.Add(
                new GameDay()
                {
                    DateLeague = DateOnly.FromDateTime(leagueDateTime),
                    Games = new()
                    {
                        new Game()
                        {
                            Id = i.ToString(),
                            StartDateLeagueTime = leagueDateTime,
                            StartDateUtc = utcDateTime,
                            HomeTeamId = "20",
                            HomeTeam = new Team()
                            {
                                Id = "20",
                                Name = "Flames",
                                City = "Calgary",
                                Abbreviation = "CGY",
                                LeagueId = Leagues.Nhl,
                                LogoLink = "https://assets.nhle.com/logos/nhl/svg/CGY_dark.svg"
                            },
                            AwayTeamId = "3",
                            AwayTeam = new Team()
                            {
                                Id = "3",
                                Name = "Rangers",
                                City = "New York",
                                Abbreviation = "NYR",
                                LeagueId = Leagues.Nhl,
                                LogoLink = "https://assets.nhle.com/logos/nhl/svg/NYR_dark.svg"
                            },
                            LeagueId = Leagues.Nhl,
                            //YouTubeLink = "https://www.youtube.com/watch?v=cdypaXsPOq0"
                        },
                        new Game()
                        {
                            Id = i.ToString() + "2",
                            StartDateLeagueTime = leagueDateTime,
                            StartDateUtc = utcDateTime,
                            HomeTeamId = "22",
                            HomeTeam = new Team()
                            {
                                Id = "22",
                                Name = "Oilers",
                                City = "Edmonton",
                                Abbreviation = "EDM",
                                LeagueId = Leagues.Nhl,
                                LogoLink = "https://assets.nhle.com/logos/nhl/svg/EDM_dark.svg"
                            },
                            AwayTeamId = "23",
                            AwayTeam = new Team()
                            {
                                Id = "23",
                                Name = "Canucks",
                                City = "Vancouver",
                                Abbreviation = "VAN",
                                LeagueId = Leagues.Nhl,
                                LogoLink = "https://assets.nhle.com/logos/nhl/svg/VAN_dark.svg"
                            },
                            LeagueId = Leagues.Nhl,
                            //YouTubeLink = "https://www.youtube.com/watch?v=cdypaXsPOq0"
                        },
                        new Game()
                        {
                            Id = i.ToString() + "3",
                            StartDateLeagueTime = leagueDateTime,
                            StartDateUtc = utcDateTime,
                            HomeTeamId = "7",
                            HomeTeam = new Team()
                            {
                                Id = "7",
                                Name = "Sabres",
                                City = "Buffalo",
                                Abbreviation = "BUF",
                                LeagueId = Leagues.Nhl,
                                LogoLink = "https://assets.nhle.com/logos/nhl/svg/BUF_dark.svg"
                            },
                            AwayTeamId = "10",
                            AwayTeam = new Team()
                            {
                                Id = "10",
                                Name = "Maple Leafs",
                                City = "Toronto",
                                Abbreviation = "TOR",
                                LeagueId = Leagues.Nhl,
                                LogoLink = "https://assets.nhle.com/logos/nhl/svg/TOR_dark.svg"
                            },
                            LeagueId = Leagues.Nhl,
                            //YouTubeLink = "https://www.youtube.com/watch?v=cdypaXsPOq0"
                        }
                    }
                });
        }

        return new FakeData(allSchedule, playlist);
    }

    private static YouTubeVideo GetFakeVideo(int addDays)
    {
        var videoLeagueTime = FakeData.VideoLeagueTime.AddDays(addDays);
        return new YouTubeVideo(Guid.NewGuid().ToString(), $"NHL Highlights | {GetRandomTeam()} vs. {GetRandomTeam()} - {videoLeagueTime:MMMM d, yyyy}", FakeData.VideoUtc.AddDays(addDays), videoLeagueTime, "https://www.youtube.com/watch?v=AAAAAAAAA");
    }

    private static readonly string[] Teams = new string[]
    {
        "Ducks",
        "Coyotes",
        "Bruins",
        "Sabres",
        "Flames",
        "Hurricanes",
        "Blackhawks",
        "Avalanche",
        "Blue Jackets",
        "Stars",
        "Red Wings",
        "Oilers",
        "Panthers",
        "Kings",
        "Wild",
        "Canadiens",
        "Predators",
        "Devils",
        "Islanders",
        "Rangers",
        "Senators",
        "Flyers",
        "Penguins",
        "Sharks",
        "Kraken",
        "Blues",
        "Lightning",
        "Maple Leafs",
        "Canucks",
        "Golden Knights",
        "Capitals",
        "Jets"
    };

    private static readonly Random Random = new Random();

    public static string GetRandomTeam()
    {
        int index = Random.Next(Teams.Length);
        return Teams[index];
    }
}

/// <summary>
/// This evening is 2025-10-26 @ 22:43 MTN, 2026-10-27 00:43 EST
/// </summary>
public class FakeData(Schedule schedule, YouTubePlaylist playlist)
{
    public Schedule Schedule { get; } = schedule;
    public YouTubePlaylist Playlist { get; } = playlist;

    public static DateTime FakeUserDateTimeNow { get; } = new(2025, 10, 26, 22, 43, 17);
    public static DateTime FakeLeagueTimeNow { get; } = new(2025, 10, 27, 0, 43, 17);
    public static DateTime FakeUtcNow { get; } = new(2025, 10, 27, 4, 43, 17, DateTimeKind.Utc);

    public static DateTime VideoUtc { get; } = new(2025, 10, 27, 3, 22, 16, DateTimeKind.Utc);
    public static DateTime VideoLeagueTime { get; } = new(2025, 10, 26, 11, 22, 16);
}
