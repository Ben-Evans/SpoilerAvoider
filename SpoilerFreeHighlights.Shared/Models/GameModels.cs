namespace SpoilerFreeHighlights.Shared.Models;

public class Game
{
    public string Id { get; set; }
    public DateTime StartDateLeagueTime { get; set; }
    public DateTime StartDateUtc { get; set; }
    //public DateTime EstimatedEndDate { get; set; } // Utc / Local
    public string YouTubeLink { get; set; } = string.Empty;
    // Secondary playlistLink
    //public string YouTubeLinkAlternate { get; set; } = string.Empty;

    /// <summary>
    /// Likely for playoff games that may or may not happen if Team A beats Team B and moves on to face Team C.
    /// </summary>
    public bool IsHypothetical { get; set; }

    public string HomeTeamId { get; set; }
    public Team HomeTeam { get; set; }

    public string AwayTeamId { get; set; }
    public Team AwayTeam { get; set; }

    public int LeagueId { get; set; }
    public League League { get; set; }

    public override string ToString() => $"{HomeTeam?.Abbreviation ?? HomeTeamId ?? "???"} vs {AwayTeam?.Abbreviation ?? AwayTeamId ?? "???"} - {StartDateLeagueTime:yyyy-MM-dd HH:mm}";
}

public class Team
{
    public string Id { get; set; }
    public string City { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Abbreviation { get; set; } = string.Empty;
    public string LogoLink { get; set; } = string.Empty;

    public int LeagueId { get; set; }
    public League League { get; set; }

    public override string ToString() => $"{City} {Name}";
}

public class GameDay
{
    public DateOnly DateLeague { get; set; }
    public string DayAbbreviation => DateLeague.ToString("ddd").ToUpper();
    public List<Game> Games { get; set; } = new();

    public override string ToString() => $"{DateLeague:yyyy-MM-dd} with {Games.Count} games";
}

public class Schedule
{
    public Leagues League { get; set; } = Leagues.All;
    public List<GameDay> GameDays { get; set; } = new();

    public override string ToString()
    {
        if (GameDays.Any())
        {
            DateOnly[] dates = GameDays.Select(x => x.DateLeague).OrderBy(x => x).ToArray();
            DateOnly first = dates.First();
            DateOnly last = dates.Last();
            return first != last ? $"{League.DisplayName}: {dates.First():yyyy-MM-dd} - {dates.Last():yyyy-MM-dd}" : $"{League.DisplayName}: {dates.First():yyyy-MM-dd}";
        }
        return League.DisplayName;
    }
}

/// <summary>
/// Populated according to <see cref="Leagues" />.
/// </summary>
public class League
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
