using InvestManage.Contracts;

namespace InvestManage.Maui.Services;

public interface IInvestManageApiClient
{
    Task<SystemStatusResponse> GetSystemStatusAsync(CancellationToken cancellationToken = default);
}

