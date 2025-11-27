namespace SpoilerFreeHighlights.Shared.Models;

/// <summary>
/// Originally a YouTube Playlist, but now it can now exist with just a Channel.
/// </summary>
public class YouTubePlaylist(string playlistId, string playlistName, string channelId, string channelName)
{
    public int Id { get; set; }
    public string PlaylistId { get; set; } = playlistId;
    public string PlaylistName { get; set; } = playlistName;
    public string ChannelId { get; set; } = channelId;
    public string ChannelName { get; set; } = channelName;

    public int LeagueId { get; set; }
    public League League { get; set; }

    public List<YouTubeVideo> Videos { get; set; } = new();

    public override string ToString()
    {
        return !string.IsNullOrEmpty(PlaylistName) ? $"{ChannelName}: {PlaylistName}" : ChannelName;
    }
}

public class YouTubeVideo(string id, string title, DateTime publishedDateUtc, DateTime publishedDateTimeLeague, string link)
{
    public string Id { get; set; } = id;
    public string Title { get; set; } = title;
    public DateTime PublishedDateTimeLeague { get; set; } = publishedDateTimeLeague;
    public DateTime PublishedDateUtc { get; set; } = publishedDateUtc;
    public string Link { get; set; } = link;
    public string ExtractedTitleTeamA { get; set; } = string.Empty;
    public string ExtractedTitleTeamB { get; set; } = string.Empty;
    public DateOnly? ExtractedTitleDate { get; set; }
    public string ExtractedTitleIdentifierA { get; set; } = string.Empty;
    public string ExtractedTitleIdentifierB { get; set; } = string.Empty;

    public int PlaylistId { get; set; }
    public YouTubePlaylist Playlist { get; set; }

    public override string ToString()
    {
        return $"{PublishedDateTimeLeague:yyyy-MM-dd HH:mm} {Title}";
    }
}
