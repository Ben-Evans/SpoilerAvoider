using SpoilerFreeHighlights.Shared.Models;
using System.Text.Json;

namespace SpoilerFreeHighlights.Services;

public class YouTubeService(HttpClient _httpClient, IConfiguration _configuration)
{
    private readonly string _youTubeApiKey = _configuration.GetValue("YouTubeApiKey", string.Empty);

    public async Task PopulateYouTubeLinks(Schedule schedule, string league, DateOnly gameDay)
    {
        foreach (var game in schedule.Games)
        {
            game.YouTubeLink = await GetYouTubeLink(league, gameDay, game);
        }
    }

    private async Task<string> GetYouTubeLink(string league, DateOnly gameDay, GameInfo game)
    {
        YouTubePlaylistResponse? playlist = await LoadOrFetchPlaylist(league, gameDay);
        if (playlist is null)
            return string.Empty;

        string videoId = DetermineBestVideoMatch(gameDay, game, playlist);
        string youTubeLink = !string.IsNullOrEmpty(videoId) ? $"https://www.youtube.com/watch?v={videoId}" : string.Empty;
        return youTubeLink;
    }

    /// <summary>
    /// GET https://youtube.googleapis.com/youtube/v3/playlistItems?part=snippet&playlistId=PLo12SYwt93SQP81ntu8rz9QcAHTNQaFI3&key=[YOUR_API_KEY] HTTP/1.1
    /// Authorization: Bearer[YOUR_ACCESS_TOKEN]
    /// Accept: application / json*/
    /// </summary>
    /// <param name="httpClient"></param>
    /// <param name="localCachePath"></param>
    /// <param name="youTubeEncoding"></param>
    /// <param name="playlistId"></param>
    /// <returns></returns>
    private async Task<YouTubePlaylistResponse?> FetchScheduleDataFromYouTubeApi(string localCachePath, string playlistId)
    {
        string maxResults = "&maxResults=20"; // string.Empty;
        string url = $"https://youtube.googleapis.com/youtube/v3/playlistItems?part=snippet{maxResults}&playlistId={playlistId}&key={_youTubeApiKey}";
        HttpResponseMessage response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
            return null;

        string resultJson = await response.Content.ReadAsStringAsync();

        Directory.CreateDirectory(Path.GetDirectoryName(localCachePath)!);
        File.WriteAllText(localCachePath, resultJson, FileService.JsonEncoding);

        return JsonSerializer.Deserialize<YouTubePlaylistResponse>(resultJson);
    }

    private async Task<YouTubePlaylistResponse?> LoadOrFetchPlaylist(string league, DateOnly gameDay)
    {
        Dictionary<string, string> playlists = new()
        {
            // Sportsnet: 2025-26 NHL Highlights, News and Analysis
            // https://www.youtube.com/playlist?list=PLo12SYwt93SQP81ntu8rz9QcAHTNQaFI3
            { "NHL", "PLo12SYwt93SQP81ntu8rz9QcAHTNQaFI3" },

            // MLB: 2025 Toronto Blue Jays Highlights, News and Interviews
            // https://www.youtube.com/playlist?list=PLo12SYwt93SQ58d0rRCwMk6eB09nRZOhR*/
            { "MLB", "PLo12SYwt93SQ58d0rRCwMk6eB09nRZOhR*/" }
        };

        string playlistId = playlists[league];

        string localCachePath = Path.Combine(AppContext.BaseDirectory, "Resources", "Downloads", $"{gameDay:yyyy-MM-dd} YouTube - {playlistId}.json");

        YouTubePlaylistResponse? playlist = FileService.GetDataFromCache<YouTubePlaylistResponse>(localCachePath);
        if (playlist is null)
            playlist = await FetchScheduleDataFromYouTubeApi(localCachePath, playlistId);

        return playlist;
    }

    private static string DetermineBestVideoMatch(DateOnly gameDay, GameInfo game, YouTubePlaylistResponse playlist)
    {
        string[] titlePrefixes = ["NHL Game Highlights", "NHL Highlights"];
        string[] teamPatterns = [$"{game.AwayTeam.Name} vs. {game.HomeTeam.Name}", $"{game.HomeTeam.Name} vs. {game.AwayTeam.Name}"];
        string[] datePatterns = [gameDay.ToString("MMMM d, yyyy"), gameDay.ToString("MMM d, yyyy")];

        var scoredItems = playlist.items
            .Select(item => new
            {
                Item = item,
                Score =
                    titlePrefixes.Count(p => item.snippet.title.Contains(p, StringComparison.OrdinalIgnoreCase))
                    + teamPatterns.Count(t => item.snippet.title.Contains(t, StringComparison.OrdinalIgnoreCase))
                    + datePatterns.Count(d => item.snippet.title.Contains(d, StringComparison.OrdinalIgnoreCase))
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ToList();

        // Picking best match above a certain threshold
        var best = scoredItems.FirstOrDefault(x => x.Score >= 3);
        string videoId = best?.Item.snippet.resourceId.videoId ?? string.Empty;
        return videoId;
    }
}
