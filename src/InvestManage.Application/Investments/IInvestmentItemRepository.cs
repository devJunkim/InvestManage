using InvestManage.Domain.Accounts;
using InvestManage.Domain.Investments;

namespace InvestManage.Application.Investments;

public interface IInvestmentItemRepository
{
    Task<IReadOnlyList<InvestmentItem>> SearchAsync(
        string? search,
        InvestmentType? type,
        string? currencyCode,
        string? provider,
        bool includeArchived,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<InvestmentItem>> ListForAccountAsync(
        Guid accountId,
        bool includeArchived,
        CancellationToken cancellationToken);

    Task<InvestmentItem?> FindAsync(
        Guid id,
        bool includeArchived,
        bool trackChanges,
        CancellationToken cancellationToken);

    Task<bool> CurrencyExistsAsync(string currencyCode, CancellationToken cancellationToken);

    Task<bool> AccountExistsAsync(
        Guid accountId,
        bool includeArchived,
        CancellationToken cancellationToken);

    Task<AccountInvestment?> FindAssignmentAsync(
        Guid accountId,
        Guid investmentItemId,
        CancellationToken cancellationToken);

    void Add(InvestmentItem investmentItem);

    void Add(AccountInvestment assignment);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
