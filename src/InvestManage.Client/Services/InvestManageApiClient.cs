using System.Net.Http.Json;
using System.Text.Json;
using InvestManage.Contracts;
using InvestManage.Contracts.Accounts;
using InvestManage.Contracts.Investments;
using InvestManage.Contracts.Users;

namespace InvestManage.Client.Services;

public sealed class InvestManageApiClient(HttpClient httpClient) : IInvestManageApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public Task<SystemStatusResponse> GetSystemStatusAsync(CancellationToken cancellationToken = default) =>
        GetAsync<SystemStatusResponse>("api/v1/system/status", cancellationToken);

    public Task<UserResponse> RegisterUserAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken = default) =>
        SendForResponseAsync<UserResponse>(HttpMethod.Post, "api/v1/users", request, cancellationToken);

    public Task<UserResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default) =>
        SendForResponseAsync<UserResponse>(HttpMethod.Post, "api/v1/authentication/login", request, cancellationToken);

    public Task<IReadOnlyList<InvestmentAccountResponse>> GetAccountsAsync(
        Guid userId,
        bool includeArchived,
        CancellationToken cancellationToken = default) =>
        GetListAsync<InvestmentAccountResponse>(
            $"api/v1/investment-accounts?userId={userId:D}&includeArchived={Bool(includeArchived)}",
            cancellationToken);

    public Task<InvestmentAccountResponse> CreateAccountAsync(
        CreateInvestmentAccountRequest request,
        CancellationToken cancellationToken = default) =>
        SendForResponseAsync<InvestmentAccountResponse>(HttpMethod.Post, "api/v1/investment-accounts", request, cancellationToken);

    public Task<InvestmentAccountResponse> UpdateAccountAsync(
        Guid id,
        UpdateInvestmentAccountRequest request,
        CancellationToken cancellationToken = default) =>
        SendForResponseAsync<InvestmentAccountResponse>(HttpMethod.Put, $"api/v1/investment-accounts/{id:D}", request, cancellationToken);

    public Task ArchiveAccountAsync(Guid id, CancellationToken cancellationToken = default) =>
        SendWithoutResponseAsync(HttpMethod.Post, $"api/v1/investment-accounts/{id:D}/archive", cancellationToken);

    public Task ReactivateAccountAsync(Guid id, CancellationToken cancellationToken = default) =>
        SendWithoutResponseAsync(HttpMethod.Post, $"api/v1/investment-accounts/{id:D}/reactivate", cancellationToken);

    public Task<IReadOnlyList<InvestmentItemResponse>> GetInvestmentItemsAsync(
        InvestmentItemFilter filter,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        AddQuery(query, "search", filter.Search);
        AddQuery(query, "type", filter.Type?.ToString());
        AddQuery(query, "currencyCode", filter.CurrencyCode);
        AddQuery(query, "provider", filter.Provider);
        query.Add($"includeArchived={Bool(filter.IncludeArchived)}");
        return GetListAsync<InvestmentItemResponse>($"api/v1/investment-items?{string.Join('&', query)}", cancellationToken);
    }

    public Task<InvestmentItemResponse> CreateInvestmentItemAsync(
        CreateInvestmentItemRequest request,
        CancellationToken cancellationToken = default) =>
        SendForResponseAsync<InvestmentItemResponse>(HttpMethod.Post, "api/v1/investment-items", request, cancellationToken);

    public Task<InvestmentItemResponse> UpdateInvestmentItemAsync(
        Guid id,
        UpdateInvestmentItemRequest request,
        CancellationToken cancellationToken = default) =>
        SendForResponseAsync<InvestmentItemResponse>(HttpMethod.Put, $"api/v1/investment-items/{id:D}", request, cancellationToken);

    public Task ArchiveInvestmentItemAsync(Guid id, CancellationToken cancellationToken = default) =>
        SendWithoutResponseAsync(HttpMethod.Post, $"api/v1/investment-items/{id:D}/archive", cancellationToken);

    public Task ReactivateInvestmentItemAsync(Guid id, CancellationToken cancellationToken = default) =>
        SendWithoutResponseAsync(HttpMethod.Post, $"api/v1/investment-items/{id:D}/reactivate", cancellationToken);

    public Task<IReadOnlyList<InvestmentItemResponse>> GetAccountInvestmentsAsync(
        Guid accountId,
        bool includeArchived,
        CancellationToken cancellationToken = default) =>
        GetListAsync<InvestmentItemResponse>(
            $"api/v1/investment-accounts/{accountId:D}/investments?includeArchived={Bool(includeArchived)}",
            cancellationToken);

    public Task<InvestmentAssignmentResponse> AssignInvestmentAsync(
        Guid accountId,
        Guid investmentItemId,
        CancellationToken cancellationToken = default) =>
        SendForResponseAsync<InvestmentAssignmentResponse>(
            HttpMethod.Post,
            $"api/v1/investment-accounts/{accountId:D}/investments/{investmentItemId:D}",
            null,
            cancellationToken);

    private async Task<T> GetAsync<T>(string path, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(path, cancellationToken);
        return await ReadResponseAsync<T>(response, cancellationToken);
    }

    private async Task<IReadOnlyList<T>> GetListAsync<T>(string path, CancellationToken cancellationToken) =>
        await GetAsync<List<T>>(path, cancellationToken);

    private async Task<T> SendForResponseAsync<T>(
        HttpMethod method,
        string path,
        object? request,
        CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(method, path);
        if (request is not null)
        {
            message.Content = JsonContent.Create(request, options: JsonOptions);
        }

        using var response = await httpClient.SendAsync(message, cancellationToken);
        return await ReadResponseAsync<T>(response, cancellationToken);
    }

    private async Task SendWithoutResponseAsync(HttpMethod method, string path, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(method, path);
        using var response = await httpClient.SendAsync(message, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    private static async Task<T> ReadResponseAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken)
            ?? throw new ApiProblemException("The API returned an empty response.");
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        ApiProblem? problem = null;
        try
        {
            problem = await response.Content.ReadFromJsonAsync<ApiProblem>(JsonOptions, cancellationToken);
        }
        catch (JsonException)
        {
            // Use the status fallback when a server does not return problem details.
        }

        var message = problem?.Detail ?? problem?.Title ?? $"The API request failed with status {(int)response.StatusCode}.";
        throw new ApiProblemException(message, problem?.Errors);
    }

    private static void AddQuery(ICollection<string> query, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            query.Add($"{name}={Uri.EscapeDataString(value.Trim())}");
        }
    }

    private static string Bool(bool value) => value.ToString().ToLowerInvariant();

    private sealed record ApiProblem(string? Title, string? Detail, Dictionary<string, string[]>? Errors);
}
