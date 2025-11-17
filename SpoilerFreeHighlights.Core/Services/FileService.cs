using System.Text;
using System.Text.Json;

namespace SpoilerFreeHighlights.Core.Services;

public static class FileService
{
    /*public static Encoding JsonEncoding { get; } = Encoding.Unicode;

    public static T? GetDataFromCache<T>(DateOnly fileDate, string cacheKey, string playlistId = "")
    {
        string localCachePath = GetCacheFilePath(fileDate, cacheKey, playlistId);

        try
        {
            if (File.Exists(localCachePath))
            {
                string cachedFileContents = File.ReadAllText(localCachePath, JsonEncoding);

                return JsonSerializer.Deserialize<T>(cachedFileContents);
            }
        }
        catch (Exception)
        {
            // File doesn't exist.
        }

        return default;
    }

    /// <param name="cacheKey">YouTube-RSS, YouTube-API, NHL-API</param>
    public static T? SaveDataToCache<T>(DateOnly fileDate, string cacheKey, string jsonData, string playlistId = "")
    {
        string localCachePath = GetCacheFilePath(fileDate, cacheKey, playlistId);

        Directory.CreateDirectory(Path.GetDirectoryName(localCachePath)!);
        File.WriteAllText(localCachePath, jsonData, JsonEncoding);

        return JsonSerializer.Deserialize<T>(jsonData);
    }
    
    /// <param name="cacheKey">YouTube-RSS, YouTube-API, NHL-API</param>
    public static YouTubeRssResult? SaveUniqueDataToCache(DateOnly fileDate, string cacheKey, YouTubeRssResult youtubeRssResult, string playlistId = "")
    {
        YouTubeRssResult? cachedData = GetDataFromCache<YouTubeRssResult>(fileDate, cacheKey, playlistId);
        if (cachedData?.Videos is null)
        {
            string newJsonData = JsonSerializer.Serialize(youtubeRssResult);
            return SaveDataToCache<YouTubeRssResult>(fileDate, cacheKey, newJsonData, playlistId);
        }

        Dictionary<string, YouTubeRssVideo> allVideos = cachedData.Videos.ToDictionary(v => v.Id);
        foreach (var nv in youtubeRssResult.Videos)
            allVideos[nv.Id] = nv;

        YouTubeRssResult combinedRssResults = new()
        {
            Videos = allVideos.Values.OrderByDescending(v => v.PublishedDate).ToList()
        };

        string jsonData = JsonSerializer.Serialize(combinedRssResults);

        return SaveDataToCache<YouTubeRssResult>(fileDate, cacheKey, jsonData, playlistId);
    }*/
}
