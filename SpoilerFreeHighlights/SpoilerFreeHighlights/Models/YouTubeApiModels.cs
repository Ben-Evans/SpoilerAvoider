using System.Text.Json.Serialization;

namespace SpoilerFreeHighlights.Models;

// --- YouTube API Response Structure ---

// Resource ID contains the video ID we need
public record YouTubeResourceId(
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("videoId")] string VideoId
);

// Individual thumbnail size
public record YouTubeThumbnail(
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("width")] int Width,
    [property: JsonPropertyName("height")] int Height
);

// All available thumbnails (default, medium, high)
public record YouTubeThumbnails(
    [property: JsonPropertyName("default")] YouTubeThumbnail Default,
    [property: JsonPropertyName("medium")] YouTubeThumbnail Medium,
    [property: JsonPropertyName("high")] YouTubeThumbnail High
);

// Snippet contains the main video information
public record YouTubeSnippet(
    [property: JsonPropertyName("publishedAt")] DateTime PublishedAt,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("channelTitle")] string ChannelTitle,
    [property: JsonPropertyName("playlistId")] string PlaylistId,
    [property: JsonPropertyName("position")] int Position,
    [property: JsonPropertyName("resourceId")] YouTubeResourceId ResourceId,
    [property: JsonPropertyName("thumbnails")] YouTubeThumbnails Thumbnails
);

// A single item in the playlist
public record YouTubePlaylistItem(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("snippet")] YouTubeSnippet Snippet
);

// Top-level response object
public record YouTubePlaylistItemsListResponse(
    [property: JsonPropertyName("nextPageToken")] string NextPageToken,
    [property: JsonPropertyName("items")] YouTubePlaylistItem[] Items
);
