using InvestManage.Domain.Accounts;

namespace InvestManage.Application.Accounts;

public interface IInvestmentAccountRepository
{
    Task<IReadOnlyList<InvestmentAccount>> ListAsync(
        Guid? userId,
        bool includeArchived,
        CancellationToken cancellationToken);

    Task<InvestmentAccount?> FindAsync(
        Guid id,
        bool includeArchived,
        bool trackChanges,
        CancellationToken cancellationToken);

    Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken);

    Task<bool> CurrencyExistsAsync(string currencyCode, CancellationToken cancellationToken);

    void Add(InvestmentAccount account);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
