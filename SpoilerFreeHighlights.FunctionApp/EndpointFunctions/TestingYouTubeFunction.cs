using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace SpoilerFreeHighlights.FunctionApp.EndpointFunctions;

public class TestingYouTubeFunction(YouTubeService _youtubeService)
{
    private readonly ILogger _logger = Log.ForContext<TestingYouTubeFunction>();

    [Function(nameof(TestingYouTubeFunction))]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = nameof(AllEndpoints.TestYouTubeService))] HttpRequestData req)
    {
        await AllEndpoints.TestYouTubeService(_youtubeService);

        HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
        return response;
    }
}
