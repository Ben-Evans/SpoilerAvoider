using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace SpoilerFreeHighlights.FunctionApp.EndpointFunctions;

public class GetAppSettingsFunction(AppDbContext _dbContext)
{
    private readonly ILogger _logger = Log.ForContext<GetAppSettingsFunction>();

    [Function(nameof(GetAppSettingsFunction))]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = nameof(AllEndpoints.GetAppSettings))] HttpRequestData req)
    {
        LeagueConfigurationDto appsettings = await AllEndpoints.GetAppSettings(_dbContext);

        HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(appsettings);

        return response;
    }
}
