namespace SpoilerFreeHighlights.Shared.Models;

public class GameInfo
{
    public int Id { get; set; }
    public DateTime StartDate { get; set; }
    public TeamInfo HomeTeam { get; set; } = new TeamInfo();
    public TeamInfo AwayTeam { get; set; } = new TeamInfo();
    //public string CurrentStatus { get; set; } // PREVIEW (not started), LIVE, or FINAL
    public bool IsScoreHidden { get; set; }
}

public class TeamInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Abbreviation { get; set; } = string.Empty;
    public string LogoLink { get; set; } = string.Empty;
    public int Score { get; set; }
}

public class Schedule
{
    public List<GameInfo> Games { get; set; } = new();
}

// LeaguePreferences, PlaylistPreferences, NotificationPreferences, 
public class TeamPreferences
{
    public List<string> Teams { get; set; } = new() { "CGY", "TOR" };
}

public record VideoModel(
    string Id,
    string Title,
    string ThumbnailUrl
);
