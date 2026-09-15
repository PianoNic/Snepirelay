using Microsoft.AspNetCore.Mvc;
using Snepirelay.Application.Services;

namespace Snepirelay.API.Controllers
{
    [ApiController]
    [Route(Path)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class RelayController(RelaySocketRunner runner) : ControllerBase
    {
        public const string Path = "api/relay";

        [HttpGet]
        public async Task Connect()
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            using var socket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await runner.RunAsync(socket, HttpContext.RequestAborted);
        }
    }
}
