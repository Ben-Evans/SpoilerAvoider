using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace SpoilerFreeHighlights.FunctionApp.EndpointFunctions;

public class UpdateAppSettingsFunction(AppDbContext _dbContext)
{
    private readonly ILogger _logger = Log.ForContext<UpdateAppSettingsFunction>();

    [Function(nameof(UpdateAppSettingsFunction))]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = nameof(AllEndpoints.UpdateAppSettings))] HttpRequestData req)
    {
        LeagueConfigurationDto? updatedLeagueConfig = await req.ReadFromJsonAsync<LeagueConfigurationDto>();
        if (updatedLeagueConfig is null)
        {
            HttpResponseData errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await errorResponse.WriteAsJsonAsync(new { error = "Invalid arguement provided." });
            return errorResponse;
        }

        LeagueConfigurationDto? savedLeagueConfig = await AllEndpoints.UpdateAppSettings(_dbContext, updatedLeagueConfig);
        if (savedLeagueConfig is not null)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(savedLeagueConfig);
            return response;
        }
        else
        {
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(new { error = "Failed to persist changes." });
            return errorResponse;
        }
    }
}
