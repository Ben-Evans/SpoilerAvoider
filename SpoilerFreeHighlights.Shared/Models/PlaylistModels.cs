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

public class PlaylistConfiguration
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ChannelName { get; set; } = string.Empty;
    public int RequiredVideoPartMatches { get; set; } = 3;
    public int RequiredVideoTitlePercentageMatch { get; set; } = 96;
    public string TitlePattern { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;

    public int LeagueConfigurationId { get; set; }

    public List<VideoTitleIdentifier> TitleIdentifiers { get; set; } = [];
    public List<VideoTitleTeamFormat> TeamFormats { get; set; } = [];
    public List<VideoTitleDateFormat> DateFormats { get; set; } = [];
}

public class VideoTitleIdentifier
{
    public int Id { get; set; }
    public string Value { get; set; } = string.Empty;

    public string PlaylistConfigurationId { get; set; } = string.Empty;
}

public class VideoTitleTeamFormat
{
    public int Id { get; set; }
    public string Value { get; set; } = string.Empty;

    public string PlaylistConfigurationId { get; set; } = string.Empty;
}

public class VideoTitleDateFormat
{
    public int Id { get; set; }
    public string Value { get; set; } = string.Empty;

    public string PlaylistConfigurationId { get; set; } = string.Empty;
}

/*public class VideoTitlePattern
{
    public int Id { get; set; }
    public string Value { get; set; }

    public string PlaylistConfigurationId { get; set; }
}*/

/*public class PlaylistConfigurationValue
{
    public int Id { get; set; }
    public string Value { get; set; } = string.Empty;
    public ValueType ValueType { get; set; }

    public int PlaylistConfigurationId { get; set; }
}

public enum ValueType
{
    TitleIdentifier,
    TeamFormat,
    DateFormat
}*/

