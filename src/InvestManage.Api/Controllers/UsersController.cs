using InvestManage.Application.Users;
using InvestManage.Contracts.Users;
using InvestManage.Domain.Users;
using Microsoft.AspNetCore.Mvc;

namespace InvestManage.Api.Controllers;

[ApiController]
[Route("api/v1/users")]
public sealed class UsersController(UserService service) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<UserResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserResponse>> Register(
        RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        var user = await service.RegisterAsync(
            request.FirstName,
            request.LastName,
            request.Email,
            request.LoginId,
            request.Password,
            cancellationToken);
        var response = ToResponse(user);
        return Created($"api/v1/users/{response.Id:D}", response);
    }

    internal static UserResponse ToResponse(User user) =>
        new(user.Id, user.FirstName, user.LastName, user.DisplayName, user.Email, user.LoginId);
}
