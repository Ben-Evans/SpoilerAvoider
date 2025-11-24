using System.Text.Json;

namespace SpoilerFreeHighlights.Core.Services;

/*public class YouTubeService(HttpClient _httpClient, IConfiguration _configuration)
{
    public const string CacheKey = "YouTube-API";
    private readonly string _youTubeApiKey = _configuration.GetValue("YouTubeApiKey", string.Empty);
    private readonly Dictionary<string, string> _playlists = new()
    {
        // Sportsnet: 2025-26 NHL Highlights, News and Analysis
        // https://www.youtube.com/playlist?list=PLo12SYwt93SQP81ntu8rz9QcAHTNQaFI3
        { "NHL", "PLo12SYwt93SQP81ntu8rz9QcAHTNQaFI3" },

        // MLB: 2025 Toronto Blue Jays Highlights, News and Interviews
        // https://www.youtube.com/playlist?list=PLo12SYwt93SQ58d0rRCwMk6eB09nRZOhR
        { "MLB", "PLo12SYwt93SQ58d0rRCwMk6eB09nRZOhR" }
    };

    public async Task PopulateYouTubeLinks(Schedule schedule, DateOnly gameDay, string league)
    {
        GameDay? today = schedule.GameDays.FirstOrDefault(gd => gd.Date == gameDay);
        foreach (Game game in today.Games)
        {
            game.YouTubeLink = await GetYouTubeLink(game, gameDay, league);
        }
    }

    private async Task<string> GetYouTubeLink(Game game, DateOnly gameDay, string league)
    {
        YouTubePlaylistResponse? playlist = await LoadOrFetchPlaylist(gameDay, league);
        if (playlist is null)
            return string.Empty;

        string videoId = DetermineBestVideoMatch(game, playlist, gameDay);
        string youTubeLink = !string.IsNullOrEmpty(videoId) ? $"https://www.youtube.com/watch?v={videoId}" : string.Empty;
        return youTubeLink;
    }

    /// <summary>
    /// API - https://youtube.googleapis.com/youtube/v3/playlistItems?part=snippet&playlistId=PLo12SYwt93SQP81ntu8rz9QcAHTNQaFI3&key=[YOUR_API_KEY]
    /// vs
    /// RSS - https://www.youtube.com/feeds/videos.xml?playlist_id=PLo12SYwt93SQP81ntu8rz9QcAHTNQaFI3
    /// </summary>
    private Task<YouTubePlaylistResponse?> FetchScheduleDataFromYouTubeApi(DateOnly gameDay, string playlistId)
    {
        string maxResults = "&maxResults=20"; // string.Empty;
        return _httpClient.FetchContent<YouTubePlaylistResponse?>($"https://youtube.googleapis.com/youtube/v3/playlistItems?part=snippet{maxResults}&playlistId={playlistId}&key={_youTubeApiKey}");
    }

    private async Task<YouTubePlaylistResponse?> LoadOrFetchPlaylist(DateOnly gameDay, string league)
    {
        string playlistId = _playlists[league];

        YouTubePlaylistResponse? playlist;
        //YouTubePlaylistResponse? playlist = FileService.GetDataFromCache<YouTubePlaylistResponse>(gameDay, CacheKey, playlistId);
        //if (playlist is null)
            playlist = await FetchScheduleDataFromYouTubeApi(gameDay, playlistId);

        return playlist;
    }

    private static string DetermineBestVideoMatch(Game game, YouTubePlaylistResponse playlist, DateOnly gameDay)
    {
        string[] titlePrefixes = ["NHL Game Highlights", "NHL Highlights"];
        string[] teamPatterns = [$"{game.AwayTeam.Name} vs. {game.HomeTeam.Name}", $"{game.HomeTeam.Name} vs. {game.AwayTeam.Name}"];
        string[] datePatterns = [gameDay.ToString("MMMM d, yyyy"), gameDay.ToString("MMM d, yyyy")];

        var scoredItems = playlist.Items
            .Select(item => new
            {
                Item = item,
                Score =
                    titlePrefixes.Count(p => item.Snippet.Title.Contains(p, StringComparison.OrdinalIgnoreCase))
                    + teamPatterns.Count(t => item.Snippet.Title.Contains(t, StringComparison.OrdinalIgnoreCase))
                    + datePatterns.Count(d => item.Snippet.Title.Contains(d, StringComparison.OrdinalIgnoreCase))
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ToList();

        // Picking best match above a certain threshold
        var best = scoredItems.FirstOrDefault(x => x.Score >= 3);
        string videoId = best?.Item.Snippet.ResourceId.VideoId ?? string.Empty;
        return videoId;
    }
}*/
