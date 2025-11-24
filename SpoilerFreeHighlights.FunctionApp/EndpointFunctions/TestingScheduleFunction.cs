using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace SpoilerFreeHighlights.FunctionApp.EndpointFunctions;

public class TestingScheduleFunction(LeaguesService _leaguesService)
{
    private readonly ILogger _logger = Log.ForContext<TestingScheduleFunction>();

    [Function(nameof(TestingScheduleFunction))]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = nameof(AllEndpoints.TestScheduleService))] HttpRequestData req)
    {
        _logger.Information($"C# Timer trigger function executed at: {DateTime.Now}");

        await AllEndpoints.TestScheduleService(_leaguesService);

        HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
        return response;
    }
}
