using System.Net;
using System.Net.Http.Json;
using InvestManage.Contracts.Users;

namespace InvestManage.Api.Tests.Integration;

public sealed class UserAccessTests
{
    [Fact]
    public async Task Register_ThenLoginWithEmailOrLoginId()
    {
        using var factory = new InvestManageApiFactory();
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync(
            "/api/v1/users",
            new RegisterUserRequest("Jun", "Kim", "jun@example.com", "junkim", "password123"));
        var registered = await registerResponse.Content.ReadFromJsonAsync<UserResponse>();

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
        Assert.NotNull(registered);
        Assert.Equal("Jun Kim", registered.DisplayName);
        Assert.NotEqual("password123", await factory.ReadPasswordHashAsync(registered.Id));

        foreach (var identifier in new[] { "JUN@example.com", "junkim" })
        {
            var loginResponse = await client.PostAsJsonAsync(
                "/api/v1/authentication/login",
                new LoginRequest(identifier, "password123"));
            var loggedIn = await loginResponse.Content.ReadFromJsonAsync<UserResponse>();

            Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
            Assert.Equal(registered.Id, loggedIn?.Id);
        }
    }
}
