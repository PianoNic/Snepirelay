using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Snepirelay.Application.Dtos.App;
using Snepirelay.Application.Models;

namespace Snepirelay.API.Controllers
{
    [ApiController]
    [Route("api/app")]
    public class AppController : ControllerBase
    {
        private static readonly string AppVersion =
            typeof(AppController).Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                ?.Split('+')[0]
            ?? "0.0.0";

        [HttpGet]
        [ProducesResponseType(typeof(AppDto), StatusCodes.Status200OK)]
        public IActionResult Get() => Ok(new AppDto(AppVersion, RelayProtocol.Version));
    }
}
