using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Collections.Specialized;
using System.Net;

namespace SpoilerFreeHighlights.FunctionApp.EndpointFunctions;

public class FetchPlaylistInfoFunction
{
    private readonly ILogger _logger = Log.ForContext<FetchPlaylistInfoFunction>();

    [Function(nameof(FetchPlaylistInfoFunction))]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = nameof(AllEndpoints.FetchPlaylistInfo))] HttpRequestData req)
    {
        NameValueCollection query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);

        int leagueId;
        string? playlistId = query[nameof(playlistId)];
        string? channelId = query[nameof(channelId)];
        if ((playlistId is null || channelId is null) || !int.TryParse(query[nameof(leagueId)], out leagueId))
        {
            HttpResponseData errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await errorResponse.WriteAsJsonAsync(new { error = "Invalid arguement provided." });
            return errorResponse;
        }

        YouTubePlaylist youtubePlaylist = AllEndpoints.FetchPlaylistInfo(playlistId, channelId, leagueId);

        HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(youtubePlaylist);

        return response;
    }
}
