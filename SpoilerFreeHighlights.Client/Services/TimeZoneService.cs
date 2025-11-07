using Microsoft.JSInterop;

namespace SpoilerFreeHighlights.Client.Services;

public class TimeZoneService(IJSRuntime _js, ILogger<TimeZoneService> _logger)
{
    public async Task<string> GetTimeZoneAsync()
    {
        string timeZone = await _js.InvokeAsync<string>("timezoneHelper.getTimeZone");

        _logger.LogDebug("Local Time Zone: " + timeZone);

        return timeZone;
    }
}

