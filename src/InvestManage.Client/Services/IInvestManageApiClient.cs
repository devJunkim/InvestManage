using InvestManage.Contracts;
using InvestManage.Contracts.Accounts;
using InvestManage.Contracts.Investments;
using InvestManage.Contracts.Users;

namespace InvestManage.Client.Services;

public interface IInvestManageApiClient
{
    Task<SystemStatusResponse> GetSystemStatusAsync(CancellationToken cancellationToken = default);
    Task<UserResponse> RegisterUserAsync(RegisterUserRequest request, CancellationToken cancellationToken = default);
    Task<UserResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InvestmentAccountResponse>> GetAccountsAsync(Guid userId, bool includeArchived, CancellationToken cancellationToken = default);
    Task<InvestmentAccountResponse> CreateAccountAsync(CreateInvestmentAccountRequest request, CancellationToken cancellationToken = default);
    Task<InvestmentAccountResponse> UpdateAccountAsync(Guid id, UpdateInvestmentAccountRequest request, CancellationToken cancellationToken = default);
    Task ArchiveAccountAsync(Guid id, CancellationToken cancellationToken = default);
    Task ReactivateAccountAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InvestmentItemResponse>> GetInvestmentItemsAsync(InvestmentItemFilter filter, CancellationToken cancellationToken = default);
    Task<InvestmentItemResponse> CreateInvestmentItemAsync(CreateInvestmentItemRequest request, CancellationToken cancellationToken = default);
    Task<InvestmentItemResponse> UpdateInvestmentItemAsync(Guid id, UpdateInvestmentItemRequest request, CancellationToken cancellationToken = default);
    Task ArchiveInvestmentItemAsync(Guid id, CancellationToken cancellationToken = default);
    Task ReactivateInvestmentItemAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InvestmentItemResponse>> GetAccountInvestmentsAsync(Guid accountId, bool includeArchived, CancellationToken cancellationToken = default);
    Task<InvestmentAssignmentResponse> AssignInvestmentAsync(Guid accountId, Guid investmentItemId, CancellationToken cancellationToken = default);
}
