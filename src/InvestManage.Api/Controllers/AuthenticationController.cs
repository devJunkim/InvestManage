using InvestManage.Application.Users;
using InvestManage.Contracts.Users;
using Microsoft.AspNetCore.Mvc;

namespace InvestManage.Api.Controllers;

[ApiController]
[Route("api/v1/authentication")]
public sealed class AuthenticationController(UserService service) : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var user = await service.LoginAsync(request.Identifier, request.Password, cancellationToken);
        return Ok(UsersController.ToResponse(user));
    }
}
