namespace SpoilerFreeHighlights.Models;

// Based on appsettings.json structure
public static class YouTubeRssConstants
{
    public const string YouTubeSectionName = "YouTube";
}

public class YouTubeSettings
{
    public Dictionary<string, LeaguePlaylistSettings> LeaguePlaylists { get; set; } = new();
}

public class LeaguePlaylistSettings
{
    public string SelectPlaylist { get; set; } = string.Empty;

    public List<PlaylistConfiguration> Playlists { get; set; } = new();
}

public class PlaylistConfiguration
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ChannelName { get; set; } = string.Empty;
    public int RequiredVideoPartMatches { get; set; } = 3;
    public int RequiredVideoTitlePercentageMatch { get; set; } = 96;

    public List<string> TitleIdentifiers { get; set; } = new();
    public List<string> TeamFormats { get; set; } = new();
    public List<string> DateFormats { get; set; } = new();
}
