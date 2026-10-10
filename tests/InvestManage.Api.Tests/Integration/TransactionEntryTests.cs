using System.Net;
using System.Net.Http.Json;
using InvestManage.Contracts.Accounts;
using InvestManage.Contracts.Investments;
using InvestManage.Contracts.Transactions;
using Microsoft.AspNetCore.Mvc;

namespace InvestManage.Api.Tests.Integration;

public sealed class TransactionEntryTests(InvestManageApiFactory factory)
    : IClassFixture<InvestManageApiFactory>
{
    [Fact]
    public async Task BuyAndSell_ArePersisted_WhileOversellingIsRejected()
    {
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();
        var (accountId, itemId) = await CreateAssignedInvestmentAsync(client);

        var buyResponse = await PostAsync(client, accountId, itemId, new CreateTransactionRequest(
            TransactionType.Buy,
            new DateOnly(2026, 10, 8),
            new DateOnly(2026, 10, 10),
            10.5m,
            20.0812m,
            9.99m,
            "cad",
            "Opening purchase"));
        Assert.Equal(HttpStatusCode.Created, buyResponse.StatusCode);
        var buy = await buyResponse.Content.ReadFromJsonAsync<TransactionResponse>();
        Assert.Equal(10.5m, buy?.Quantity);
        Assert.Equal(20.0812m, buy?.UnitPrice);
        Assert.Equal("CAD", buy?.CurrencyCode);
        Assert.Equal("Opening purchase", buy?.Notes);

        var sellResponse = await PostAsync(client, accountId, itemId, new CreateTransactionRequest(
            TransactionType.Sell,
            new DateOnly(2026, 10, 9),
            null,
            4.25m,
            21.1234m,
            0m,
            "CAD",
            null));
        Assert.Equal(HttpStatusCode.Created, sellResponse.StatusCode);

        var oversellResponse = await PostAsync(client, accountId, itemId, new CreateTransactionRequest(
            TransactionType.Sell,
            new DateOnly(2026, 10, 9),
            null,
            6.25000001m,
            21.1234m,
            0m,
            "CAD",
            null));
        Assert.Equal(HttpStatusCode.BadRequest, oversellResponse.StatusCode);
        var problem = await oversellResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Contains("quantity", problem?.Errors.Keys ?? []);
    }

    [Fact]
    public async Task InvalidTransaction_ReturnsFieldLevelValidation()
    {
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();
        var (accountId, itemId) = await CreateAssignedInvestmentAsync(client);

        var response = await PostAsync(client, accountId, itemId, new CreateTransactionRequest(
            TransactionType.Buy,
            new DateOnly(2026, 10, 9),
            new DateOnly(2026, 10, 8),
            0,
            -1,
            -1,
            "CA",
            null));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Equal(
            ["currencyCode", "fees", "quantity", "settlementDate", "unitPrice"],
            (problem?.Errors.Keys ?? []).OrderBy(key => key).ToArray());
    }

    private static Task<HttpResponseMessage> PostAsync(
        HttpClient client,
        Guid accountId,
        Guid itemId,
        CreateTransactionRequest request) =>
        client.PostAsJsonAsync(
            $"/api/v1/investment-accounts/{accountId:D}/investments/{itemId:D}/transactions",
            request);

    private static async Task<(Guid AccountId, Guid ItemId)> CreateAssignedInvestmentAsync(HttpClient client)
    {
        var accountResponse = await client.PostAsJsonAsync(
            "/api/v1/investment-accounts",
            new CreateInvestmentAccountRequest(
                InvestManageApiFactory.UserId,
                "RRSP",
                InvestmentAccountType.RegisteredRetirementSavingsPlan,
                "CAD"));
        accountResponse.EnsureSuccessStatusCode();
        var account = await accountResponse.Content.ReadFromJsonAsync<InvestmentAccountResponse>();

        var itemResponse = await client.PostAsJsonAsync(
            "/api/v1/investment-items",
            new CreateInvestmentItemRequest(
                "TDB3046",
                "TD mutual fund",
                InvestmentType.MutualFund,
                "CAD",
                "TD",
                4,
                null));
        itemResponse.EnsureSuccessStatusCode();
        var item = await itemResponse.Content.ReadFromJsonAsync<InvestmentItemResponse>();

        var accountId = Assert.IsType<InvestmentAccountResponse>(account).Id;
        var itemId = Assert.IsType<InvestmentItemResponse>(item).Id;
        var assignmentResponse = await client.PostAsync(
            $"/api/v1/investment-accounts/{accountId:D}/investments/{itemId:D}",
            null);
        assignmentResponse.EnsureSuccessStatusCode();
        return (accountId, itemId);
    }
}
