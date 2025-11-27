namespace SpoilerFreeHighlights.Shared.Models;

public class LeagueConfigurationDto
{
    public List<LeagueConfiguration> LeagueConfigurations { get; set; } = [];
}

public class LeagueConfiguration
{
    public int LeagueId { get; set; }
    public League League { get; set; }

    /// <summary>
    /// All, Single, First
    /// </summary>
    public string SelectPlaylistType { get; set; } = string.Empty;

    public List<PlaylistConfiguration> Playlists { get; set; } = [];
}

public class PlaylistConfiguration(string playlistId, string playlistName, string channelId, string channelName)
{
    public int Id { get; set; }
    public bool IsDisabled { get; set; }
    public string PlaylistId { get; set; } = playlistId;
    public string PlaylistName { get; set; } = playlistName;
    public string ChannelId { get; set; } = channelId;
    public string ChannelName { get; set; } = channelName;
    public int RequiredVideoPartMatches { get; set; } = 3;
    public int RequiredVideoTitlePercentageMatch { get; set; } = 96;
    public string TitlePattern { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;

    public int LeagueConfigurationId { get; set; }

    public List<VideoTitleIdentifier> TitleIdentifiers { get; set; } = [];
    public List<VideoTitleTeamFormat> TeamFormats { get; set; } = [];
    public List<VideoTitleDateFormat> DateFormats { get; set; } = [];

    public override string ToString()
    {
        return !string.IsNullOrEmpty(PlaylistName) ? $"{ChannelName}: {PlaylistName}" : ChannelName;
    }
}

public class VideoTitleIdentifier
{
    public int Id { get; set; }
    public string Value { get; set; } = string.Empty;

    public int PlaylistConfigurationId { get; set; }
}

public class VideoTitleTeamFormat
{
    public int Id { get; set; }
    public string Value { get; set; } = string.Empty;

    public int PlaylistConfigurationId { get; set; }
}

public class VideoTitleDateFormat
{
    public int Id { get; set; }
    public string Value { get; set; } = string.Empty;

    public int PlaylistConfigurationId { get; set; }
}
