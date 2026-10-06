using System.Net;
using System.Net.Http.Json;
using InvestManage.Contracts.Accounts;
using InvestManage.Contracts.Investments;
using Microsoft.AspNetCore.Mvc;

namespace InvestManage.Api.Tests.Integration;

public sealed class InvestmentItemManagementTests(InvestManageApiFactory factory)
    : IClassFixture<InvestManageApiFactory>
{
    [Fact]
    public async Task Lifecycle_AssignsOneIdentityToMultipleAccounts_AndPreservesHistoryWhenArchived()
    {
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();
        var firstAccountId = await CreateAccountAsync(client, "RRSP");
        var secondAccountId = await CreateAccountAsync(client, "TFSA");

        var createResponse = await client.PostAsJsonAsync(
            "/api/v1/investment-items",
            new CreateInvestmentItemRequest(
                " tdb3046 ",
                "TD mutual fund",
                InvestmentType.MutualFund,
                "cad",
                "TD",
                4,
                "Core holding"));

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<InvestmentItemResponse>();
        Assert.NotNull(created);
        Assert.Equal("tdb3046", created.Code);
        Assert.Equal("CAD", created.CurrencyCode);
        Assert.False(created.IsArchived);

        var firstAssignment = await client.PostAsync(
            $"/api/v1/investment-accounts/{firstAccountId}/investments/{created.Id}",
            null);
        var repeatedAssignment = await client.PostAsync(
            $"/api/v1/investment-accounts/{firstAccountId}/investments/{created.Id}",
            null);
        var secondAssignment = await client.PostAsync(
            $"/api/v1/investment-accounts/{secondAccountId}/investments/{created.Id}",
            null);

        Assert.Equal(HttpStatusCode.Created, firstAssignment.StatusCode);
        Assert.Equal(HttpStatusCode.OK, repeatedAssignment.StatusCode);
        Assert.Equal(HttpStatusCode.Created, secondAssignment.StatusCode);

        var updateResponse = await client.PutAsJsonAsync(
            $"/api/v1/investment-items/{created.Id}",
            new UpdateInvestmentItemRequest(
                "TDB3046",
                "TD Canadian Index Fund",
                InvestmentType.MutualFund,
                "CAD",
                "TD Asset Management",
                6,
                "Updated details"));
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<InvestmentItemResponse>();
        Assert.Equal(6, updated?.PricePrecision);

        var searchResults = await client.GetFromJsonAsync<List<InvestmentItemResponse>>(
            "/api/v1/investment-items?search=Canadian&type=MutualFund&currencyCode=CAD&provider=TD%20Asset%20Management");
        Assert.Equal(created.Id, Assert.Single(searchResults ?? []).Id);

        var firstAccountItems = await client.GetFromJsonAsync<List<InvestmentItemResponse>>(
            $"/api/v1/investment-accounts/{firstAccountId}/investments");
        var secondAccountItems = await client.GetFromJsonAsync<List<InvestmentItemResponse>>(
            $"/api/v1/investment-accounts/{secondAccountId}/investments");
        Assert.Equal(created.Id, Assert.Single(firstAccountItems ?? []).Id);
        Assert.Equal(created.Id, Assert.Single(secondAccountItems ?? []).Id);

        await factory.AddPriceHistoryAsync(created.Id);
        var archiveResponse = await client.PostAsync(
            $"/api/v1/investment-items/{created.Id}/archive",
            null);
        Assert.Equal(HttpStatusCode.NoContent, archiveResponse.StatusCode);

        var activeItems = await client.GetFromJsonAsync<List<InvestmentItemResponse>>(
            "/api/v1/investment-items");
        var activeAccountItems = await client.GetFromJsonAsync<List<InvestmentItemResponse>>(
            $"/api/v1/investment-accounts/{firstAccountId}/investments");
        Assert.Empty(activeItems ?? []);
        Assert.Empty(activeAccountItems ?? []);

        var archivedItems = await client.GetFromJsonAsync<List<InvestmentItemResponse>>(
            "/api/v1/investment-items?includeArchived=true");
        Assert.True(Assert.Single(archivedItems ?? []).IsArchived);

        var state = await factory.ReadInvestmentStateAsync(created.Id);
        Assert.Equal(1, state.Items);
        Assert.Equal(2, state.Assignments);
        Assert.Equal(1, state.Prices);

        var reactivateResponse = await client.PostAsync(
            $"/api/v1/investment-items/{created.Id}/reactivate",
            null);
        Assert.Equal(HttpStatusCode.NoContent, reactivateResponse.StatusCode);

        var reactivated = await client.GetFromJsonAsync<InvestmentItemResponse>(
            $"/api/v1/investment-items/{created.Id}");
        Assert.False(reactivated?.IsArchived);
    }

    [Fact]
    public async Task Create_WithInvalidPrecision_ReturnsValidationProblem()
    {
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/investment-items",
            new CreateInvestmentItemRequest(
                "TDB3046",
                "TD mutual fund",
                InvestmentType.MutualFund,
                "CAD",
                "TD",
                9,
                null));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Contains("pricePrecision", problem?.Errors.Keys ?? []);
    }

    [Fact]
    public async Task Assign_UnknownInvestmentItem_ReturnsProblemDetails()
    {
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();
        var accountId = await CreateAccountAsync(client, "RRSP");

        var response = await client.PostAsync(
            $"/api/v1/investment-accounts/{accountId}/investments/{Guid.NewGuid()}",
            null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal("The requested resource was not found.", problem?.Title);
    }

    private static async Task<Guid> CreateAccountAsync(HttpClient client, string name)
    {
        var response = await client.PostAsJsonAsync(
            "/api/v1/investment-accounts",
            new CreateInvestmentAccountRequest(
                InvestManageApiFactory.UserId,
                name,
                InvestmentAccountType.Other,
                "CAD"));
        response.EnsureSuccessStatusCode();
        var account = await response.Content.ReadFromJsonAsync<InvestmentAccountResponse>();
        return Assert.IsType<InvestmentAccountResponse>(account).Id;
    }
}
