using System.Text.Json;
using InvestManage.Contracts;

namespace InvestManage.Maui.Tests;

public sealed class ApiContractTests
{
    [Fact]
    public void SystemStatusResponse_CanBeDeserializedByTheClient()
    {
        const string json = """
            {
              "service": "InvestManage API",
              "version": "1.0.0",
              "environment": "Development",
              "serverTimeUtc": "2026-10-03T16:00:00+00:00"
            }
            """;

        var response = JsonSerializer.Deserialize<SystemStatusResponse>(
            json,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.NotNull(response);
        Assert.Equal("InvestManage API", response.Service);
    }
}
