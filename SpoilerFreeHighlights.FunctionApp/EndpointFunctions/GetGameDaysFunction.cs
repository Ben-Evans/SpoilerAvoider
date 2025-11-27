using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using SpoilerFreeHighlights.Core.Services.LeagueServices;
using System.Net;

namespace SpoilerFreeHighlights.FunctionApp.EndpointFunctions;

public class GetGameDaysFunction(NhlService _nhlService, MlbService _mlbService, CflService _cflService)
{
    private readonly ILogger _logger = Log.ForContext<GetGameDaysFunction>();

    [Function(nameof(GetGameDaysFunction))]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = nameof(AllEndpoints.GetGameDays))] HttpRequestData req)
    {
        ScheduleQuery scheduleQuery = req.Headers.TryGetValues("Content-Length", out var contentLength) && contentLength.First() != "0"
            ? await req.ReadFromJsonAsync<ScheduleQuery>() ?? new ScheduleQuery()
            : new ScheduleQuery();

        Schedule? allSchedule = await AllEndpoints.GetGameDays(_nhlService, _mlbService, _cflService, scheduleQuery);
        if (allSchedule is not null)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(allSchedule);
            return response;
        }
        else
        {
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(new { error = "Failed to deserialize external API response." });
            return errorResponse;
        }
    }
}
