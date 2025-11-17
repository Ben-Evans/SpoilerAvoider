namespace SpoilerFreeHighlights.Core.Models;

#region YouTube API Models
public class YouTubePlaylistResponse
{
    public string Kind { get; set; }
    public string Etag { get; set; }
    public string NextPageToken { get; set; }
    public List<PlaylistItem> Items { get; set; }
    public PageInfo PageInfo { get; set; }
}

public class PageInfo
{
    public int TotalResults { get; set; }
    public int ResultsPerPage { get; set; }
}

public class PlaylistItem
{
    public string Kind { get; set; }
    public string Etag { get; set; }
    public string Id { get; set; }
    public Snippet Snippet { get; set; }
}

public class Snippet
{
    public DateTimeOffset PublishedAt { get; set; }
    public string ChannelId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public Thumbnails Thumbnails { get; set; }
    public string ChannelTitle { get; set; }
    public string PlaylistId { get; set; }
    public int Position { get; set; }
    public ResourceId ResourceId { get; set; }
    public string VideoOwnerChannelTitle { get; set; }
    public string VideoOwnerChannelId { get; set; }
}

public class Thumbnails
{
    public Thumbnail DefaultThumbnail { get; set; }
    public Thumbnail Medium { get; set; }
    public Thumbnail High { get; set; }
    public Thumbnail Standard { get; set; }
    public Thumbnail Maxres { get; set; }
}

public class Thumbnail
{
    public string Url { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}

public class ResourceId
{
    public string Kind { get; set; }
    public string VideoId { get; set; }
}
#endregion YouTube API Models


#region YouTube RSS Models
/*public class YouTubeRssResult()
{
    // PLo12SYwt93SQP81ntu8rz9QcAHTNQaFI3
    public string Id { get; set; } = string.Empty;
    // 2025-26 NHL Highlights, News and Analysis
    public string Name { get; set; } = string.Empty;
    public List<YouTubeRssVideo> Videos { get; set; } = new();
}

/// <param name="Id">hqUoCKZvPH0 (from yt:video:hqUoCKZvPH0)</param>
/// <param name="Title">NHL Highlights | Jets vs.Flyers - October 16, 2025</param>
/// <param name="PublishedDate">2025:10:17 02:06</param>
/// <param name="Link">https://www.youtube.com/watch?v=hqUoCKZvPH0</param>
public record YouTubeRssVideo(string Id, string Title, DateTime PublishedDate, string Link);*/
#endregion YouTube RSS Models
