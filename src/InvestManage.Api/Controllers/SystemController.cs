using InvestManage.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace InvestManage.Api.Controllers;

[ApiController]
[Route("api/v1/system")]
public sealed class SystemController(IHostEnvironment environment) : ControllerBase
{
    [HttpGet("status")]
    [ProducesResponseType<SystemStatusResponse>(StatusCodes.Status200OK)]
    public ActionResult<SystemStatusResponse> GetStatus()
    {
        var version = typeof(SystemController).Assembly.GetName().Version?.ToString() ?? "unknown";

        return Ok(new SystemStatusResponse(
            "InvestManage API",
            version,
            environment.EnvironmentName,
            DateTimeOffset.UtcNow));
    }
}

