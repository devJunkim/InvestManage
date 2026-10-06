using System.Net;
using System.Net.Http.Json;
using InvestManage.Contracts.Accounts;
using Microsoft.AspNetCore.Mvc;

namespace InvestManage.Api.Tests.Integration;

public sealed class InvestmentAccountLifecycleTests(InvestManageApiFactory factory)
    : IClassFixture<InvestManageApiFactory>
{
    [Fact]
    public async Task Lifecycle_ArchivesWithoutDeletingHistory_AndCanReactivate()
    {
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync(
            "/api/v1/investment-accounts",
            new CreateInvestmentAccountRequest(
                InvestManageApiFactory.UserId,
                "Retirement",
                InvestmentAccountType.RegisteredRetirementSavingsPlan,
                "cad"));

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<InvestmentAccountResponse>();
        Assert.NotNull(created);
        Assert.Equal("CAD", created.CurrencyCode);
        Assert.False(created.IsArchived);

        var updateResponse = await client.PutAsJsonAsync(
            $"/api/v1/investment-accounts/{created.Id}",
            new UpdateInvestmentAccountRequest(
                "Long-term retirement",
                InvestmentAccountType.RegisteredRetirementIncomeFund,
                "CAD"));
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<InvestmentAccountResponse>();
        Assert.Equal("Long-term retirement", updated?.Name);

        await factory.AddFinancialHistoryAsync(created.Id);

        var archiveResponse = await client.PostAsync(
            $"/api/v1/investment-accounts/{created.Id}/archive",
            null);
        Assert.Equal(HttpStatusCode.NoContent, archiveResponse.StatusCode);

        var defaultList = await client.GetFromJsonAsync<List<InvestmentAccountResponse>>(
            $"/api/v1/investment-accounts?userId={InvestManageApiFactory.UserId}");
        Assert.Empty(defaultList ?? []);

        var archivedList = await client.GetFromJsonAsync<List<InvestmentAccountResponse>>(
            $"/api/v1/investment-accounts?userId={InvestManageApiFactory.UserId}&includeArchived=true");
        Assert.Single(archivedList ?? []);
        Assert.True(archivedList![0].IsArchived);

        var historyState = await factory.ReadHistoryStateAsync(created.Id);
        Assert.True(historyState.IsArchived);
        Assert.Equal(1, historyState.Holdings);
        Assert.Equal(1, historyState.Transactions);

        var reactivateResponse = await client.PostAsync(
            $"/api/v1/investment-accounts/{created.Id}/reactivate",
            null);
        Assert.Equal(HttpStatusCode.NoContent, reactivateResponse.StatusCode);

        var reactivated = await client.GetFromJsonAsync<InvestmentAccountResponse>(
            $"/api/v1/investment-accounts/{created.Id}");
        Assert.False(reactivated?.IsArchived);
    }

    [Fact]
    public async Task Create_WithUnknownUser_ReturnsValidationProblem()
    {
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/investment-accounts",
            new CreateInvestmentAccountRequest(
                Guid.NewGuid(),
                "TFSA",
                InvestmentAccountType.TaxFreeSavingsAccount,
                "CAD"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Contains("userId", problem?.Errors.Keys ?? []);
    }

    [Fact]
    public async Task Get_UnknownAccount_ReturnsProblemDetails()
    {
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/investment-accounts/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal("The requested resource was not found.", problem?.Title);
    }
}
