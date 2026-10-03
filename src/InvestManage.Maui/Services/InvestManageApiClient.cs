using System.Net.Http.Json;
using InvestManage.Contracts;

namespace InvestManage.Maui.Services;

public sealed class InvestManageApiClient(HttpClient httpClient) : IInvestManageApiClient
{
    public async Task<SystemStatusResponse> GetSystemStatusAsync(
        CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<SystemStatusResponse>(
                   "api/v1/system/status",
                   cancellationToken)
               ?? throw new InvalidOperationException("The API returned an empty status response.");
    }
}

