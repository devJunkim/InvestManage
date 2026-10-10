using System.Net;
using System.Text;
using InvestManage.Client.Services;
using InvestManage.Contracts.Accounts;
using InvestManage.Contracts.Investments;
using InvestManage.Contracts.Transactions;
using InvestManage.Contracts.Users;

namespace InvestManage.Client.Tests;

public sealed class InvestManageApiClientTests
{
    [Fact]
    public async Task RegisterAndLogin_UseUserAccessRoutes()
    {
        var handler = new RecordingHandler(_ => JsonResponse(HttpStatusCode.OK, """
            {"id":"7fcb8eb1-a0ec-49ad-8d25-72042c1b269f","firstName":"Jun","lastName":"Kim","displayName":"Jun Kim","email":"jun@example.com","loginId":"junkim"}
            """));
        var client = CreateClient(handler);

        await client.RegisterUserAsync(new RegisterUserRequest("Jun", "Kim", "jun@example.com", "junkim", "password123"));
        await client.LoginAsync(new LoginRequest("jun@example.com", "password123"));

        Assert.Equal("api/v1/users", RelativeUri(handler.Requests[0]));
        Assert.Equal("api/v1/authentication/login", RelativeUri(handler.Requests[1]));
        Assert.Contains("password123", handler.Requests[0].Body);
    }

    [Fact]
    public async Task GetInvestmentItems_BuildsEncodedFilterAndDeserializesResponse()
    {
        var handler = new RecordingHandler(_ => JsonResponse(HttpStatusCode.OK, """
            [{"id":"1fe4ca47-03e6-4be9-87de-178b66e3e550","code":"TDB3046","name":"TD Canadian Index Fund","type":"MutualFund","currencyCode":"CAD","provider":"TD Asset Management","pricePrecision":4,"notes":null,"isArchived":false}]
            """));
        var client = CreateClient(handler);

        var result = await client.GetInvestmentItemsAsync(
            new InvestmentItemFilter("Canadian index", InvestmentType.MutualFund, "CAD", "TD Asset Management", true));

        Assert.Equal("TDB3046", Assert.Single(result).Code);
        Assert.Equal(
            "api/v1/investment-items?search=Canadian%20index&type=MutualFund&currencyCode=CAD&provider=TD%20Asset%20Management&includeArchived=true",
            RelativeUri(handler.Requests.Single()));
    }

    [Fact]
    public async Task CreateAccount_SendsContractAndReadsCreatedAccount()
    {
        var userId = Guid.Parse("7fcb8eb1-a0ec-49ad-8d25-72042c1b269f");
        var handler = new RecordingHandler(_ => JsonResponse(HttpStatusCode.Created, $$"""
            {"id":"23bcd69b-6806-4981-a4fc-0ee93a27c123","userId":"{{userId}}","name":"Retirement","type":"RegisteredRetirementSavingsPlan","currencyCode":"CAD","isArchived":false}
            """));
        var client = CreateClient(handler);

        var result = await client.CreateAccountAsync(
            new CreateInvestmentAccountRequest(userId, "Retirement", InvestmentAccountType.RegisteredRetirementSavingsPlan, "CAD"));

        Assert.Equal("Retirement", result.Name);
        Assert.Equal(HttpMethod.Post, handler.Requests.Single().Method);
        Assert.Contains("RegisteredRetirementSavingsPlan", handler.Requests.Single().Body);
    }

    [Fact]
    public async Task ValidationProblem_IsConvertedToDisplayableClientException()
    {
        var handler = new RecordingHandler(_ => JsonResponse(HttpStatusCode.BadRequest, """
            {"title":"Validation failed","status":400,"errors":{"pricePrecision":["Price precision must be between zero and eight decimal places."]}}
            """));
        var client = CreateClient(handler);

        var exception = await Assert.ThrowsAsync<ApiProblemException>(() =>
            client.CreateInvestmentItemAsync(
                new CreateInvestmentItemRequest("TDB3046", "Fund", InvestmentType.MutualFund, "CAD", null, 9, null)));

        Assert.Contains("pricePrecision", exception.Errors.Keys);
        Assert.Contains("between zero and eight", exception.ToDisplayMessage());
    }

    [Fact]
    public async Task LifecycleAndAssignmentOperations_UseExpectedRoutes()
    {
        var accountId = Guid.Parse("23bcd69b-6806-4981-a4fc-0ee93a27c123");
        var itemId = Guid.Parse("1fe4ca47-03e6-4be9-87de-178b66e3e550");
        var handler = new RecordingHandler(request => request.RequestUri?.AbsolutePath.EndsWith(itemId.ToString()) == true
            ? JsonResponse(HttpStatusCode.Created, $$"""{"id":"9ca83866-463a-4e91-a5d4-8c4f6c8a3f01","investmentAccountId":"{{accountId}}","investmentItemId":"{{itemId}}"}""")
            : new HttpResponseMessage(HttpStatusCode.NoContent));
        var client = CreateClient(handler);

        await client.ArchiveAccountAsync(accountId);
        await client.ReactivateInvestmentItemAsync(itemId);
        await client.AssignInvestmentAsync(accountId, itemId);

        Assert.Equal($"api/v1/investment-accounts/{accountId:D}/archive", RelativeUri(handler.Requests[0]));
        Assert.Equal($"api/v1/investment-items/{itemId:D}/reactivate", RelativeUri(handler.Requests[1]));
        Assert.Equal($"api/v1/investment-accounts/{accountId:D}/investments/{itemId:D}", RelativeUri(handler.Requests[2]));
    }

    [Fact]
    public async Task CreateTransaction_UsesAssignedInvestmentRouteAndPreservesPrecision()
    {
        var accountId = Guid.Parse("23bcd69b-6806-4981-a4fc-0ee93a27c123");
        var itemId = Guid.Parse("1fe4ca47-03e6-4be9-87de-178b66e3e550");
        var handler = new RecordingHandler(_ => JsonResponse(HttpStatusCode.Created, $$"""
            {"id":"9ca83866-463a-4e91-a5d4-8c4f6c8a3f01","accountId":"{{accountId}}","investmentItemId":"{{itemId}}","type":"Buy","tradeDate":"2026-10-09","settlementDate":null,"quantity":12.345678,"unitPrice":20.0812,"fees":0,"currencyCode":"CAD","notes":null,"createdAtUtc":"2026-10-09T12:00:00Z"}
            """));
        var client = CreateClient(handler);

        var result = await client.CreateTransactionAsync(
            accountId,
            itemId,
            new CreateTransactionRequest(
                TransactionType.Buy,
                new DateOnly(2026, 10, 9),
                null,
                12.345678m,
                20.0812m,
                0,
                "CAD",
                null));

        Assert.Equal(12.345678m, result.Quantity);
        Assert.Equal(20.0812m, result.UnitPrice);
        Assert.Equal(
            $"api/v1/investment-accounts/{accountId:D}/investments/{itemId:D}/transactions",
            RelativeUri(handler.Requests.Single()));
        Assert.Contains("12.345678", handler.Requests.Single().Body);
    }

    private static InvestManageApiClient CreateClient(HttpMessageHandler handler) =>
        new(new HttpClient(handler) { BaseAddress = new Uri("https://localhost:7094/") });

    private static string RelativeUri(RecordedRequest request) =>
        request.RequestUri?.PathAndQuery.TrimStart('/') ?? string.Empty;

    private static HttpResponseMessage JsonResponse(HttpStatusCode statusCode, string json) =>
        new(statusCode) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    private sealed class RecordingHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        public List<RecordedRequest> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            Requests.Add(new RecordedRequest(request.Method, request.RequestUri, body));
            return responseFactory(request);
        }
    }

    private sealed record RecordedRequest(HttpMethod Method, Uri? RequestUri, string? Body);
}
