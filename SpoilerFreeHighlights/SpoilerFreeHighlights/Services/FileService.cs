using System.Text;
using System.Text.Json;

namespace SpoilerFreeHighlights.Services;

public static class FileService
{
    public static Encoding JsonEncoding { get; } = Encoding.Unicode;

    public static T? GetDataFromCache<T>(string localCachePath)
    {
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
}
