namespace SpoilerFreeHighlights.Models;

public class YouTubePlaylistResponse
{
    public string kind { get; set; }
    public string etag { get; set; }
    public string nextPageToken { get; set; }
    public List<PlaylistItem> items { get; set; }
    public PageInfo pageInfo { get; set; }
}

public class PageInfo
{
    public int totalResults { get; set; }
    public int resultsPerPage { get; set; }
}

public class PlaylistItem
{
    public string kind { get; set; }
    public string etag { get; set; }
    public string id { get; set; }
    public Snippet snippet { get; set; }
}

public class Snippet
{
    public DateTimeOffset publishedAt { get; set; }
    public string channelId { get; set; }
    public string title { get; set; }
    public string description { get; set; }
    public Thumbnails thumbnails { get; set; }
    public string channelTitle { get; set; }
    public string playlistId { get; set; }
    public int position { get; set; }
    public ResourceId resourceId { get; set; }
    public string videoOwnerChannelTitle { get; set; }
    public string videoOwnerChannelId { get; set; }
}

public class Thumbnails
{
    public Thumbnail defaultThumbnail { get; set; }
    public Thumbnail medium { get; set; }
    public Thumbnail high { get; set; }
    public Thumbnail standard { get; set; }
    public Thumbnail maxres { get; set; }
}

public class Thumbnail
{
    public string url { get; set; }
    public int width { get; set; }
    public int height { get; set; }
}

public class ResourceId
{
    public string kind { get; set; }
    public string videoId { get; set; }
}
