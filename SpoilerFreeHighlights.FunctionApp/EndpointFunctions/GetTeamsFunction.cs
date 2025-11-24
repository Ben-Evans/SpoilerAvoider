using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace SpoilerFreeHighlights.FunctionApp.EndpointFunctions;

public class GetTeamsFunction(AppDbContext _dbContext)
{
    private readonly ILogger _logger = Log.ForContext<GetTeamsFunction>();

    [Function(nameof(GetTeamsFunction))]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = nameof(AllEndpoints.GetTeams))] HttpRequestData req)
    {        
        Team[] teams = await AllEndpoints.GetTeams(_dbContext);

        HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(teams);

        return response;
    }
}
