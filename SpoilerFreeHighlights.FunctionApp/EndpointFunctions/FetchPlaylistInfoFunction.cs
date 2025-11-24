using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Collections.Specialized;
using System.Net;

namespace SpoilerFreeHighlights.FunctionApp.EndpointFunctions;

public class FetchPlaylistInfoFunction(HttpClient _httpClient)
{
    private readonly ILogger _logger = Log.ForContext<FetchPlaylistInfoFunction>();

    [Function(nameof(FetchPlaylistInfoFunction))]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = nameof(AllEndpoints.FetchPlaylistInfo))] HttpRequestData req)
    {
        // [FromQuery] string playlistId, [FromQuery] int leagueId
        NameValueCollection query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);

        int leagueId;
        string? playlistId = query[nameof(playlistId)];
        if (playlistId is null || !int.TryParse(query[nameof(leagueId)], out leagueId))
        {
            HttpResponseData errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await errorResponse.WriteAsJsonAsync(new { error = "Invalid arguement provided." });
            return errorResponse;
        }

        YouTubePlaylist youtubePlaylist = AllEndpoints.FetchPlaylistInfo(_httpClient, playlistId, leagueId);

        HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(youtubePlaylist);

        return response;
    }
}
