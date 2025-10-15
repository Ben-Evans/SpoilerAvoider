using Microsoft.AspNetCore.Mvc;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace SpoilerFreeHighlights.Endpoints
{
    [Route("api/[controller]")]
    [ApiController]
    public class YouTubeEndpoint : ControllerBase
    {
    }
}
