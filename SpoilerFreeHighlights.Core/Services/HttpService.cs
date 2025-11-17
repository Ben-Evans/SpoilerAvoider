using System.Runtime.CompilerServices;
using System.Text.Json;

namespace SpoilerFreeHighlights.Core.Services;

public static class HttpService
{
    private static readonly ILogger _logger = Log.ForContext(typeof(HttpService));
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public static async Task<T?> FetchContent<T>(this HttpClient httpClient, string httpUrl, [CallerMemberName] string caller = "")
    {
        _logger.Information("Fetching {Url}...", httpUrl);
        HttpResponseMessage response = await httpClient.GetAsync(httpUrl);
        if (!response.IsSuccessStatusCode)
        {
            _logger.Error("Failed to fetch from {Url} for {Caller}.", httpUrl, caller);
            return default;
        }
        _logger.Information("Fetched successfully.");

        string jsonData = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<T>(jsonData, JsonOptions);
    }
}
