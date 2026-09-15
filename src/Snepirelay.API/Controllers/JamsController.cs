using Mediator;
using Microsoft.AspNetCore.Mvc;
using Snepirelay.API.Extensions;
using Snepirelay.Application.Dtos.Jams;
using Snepirelay.Application.Queries.Jams;

namespace Snepirelay.API.Controllers
{
    [ApiController]
    [Route("api/jams")]
    public class JamsController(IMediator mediator) : ControllerBase
    {
        [HttpGet("{joinToken}")]
        [ProducesResponseType(typeof(JamPreviewDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Preview(string joinToken, CancellationToken cancellationToken = default)
        {
            var result = await mediator.Send(new GetJamPreviewQuery(joinToken), cancellationToken);
            return result.ToActionResult();
        }
    }
}
