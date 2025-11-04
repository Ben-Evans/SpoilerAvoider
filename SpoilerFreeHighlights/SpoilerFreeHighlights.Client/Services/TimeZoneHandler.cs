namespace SpoilerFreeHighlights.Client.Services;

public class TimeZoneHandler(TimeZoneService _timeZoneService) : DelegatingHandler
{
    private string? _cachedTimeZone;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_cachedTimeZone is null)
            _cachedTimeZone = await _timeZoneService.GetTimeZoneAsync();

        if (!request.Headers.Contains("Time-Zone"))
            request.Headers.Add("Time-Zone", _cachedTimeZone);

        return await base.SendAsync(request, cancellationToken);
    }
}
