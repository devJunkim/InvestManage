using InvestManage.Domain.Accounts;
using InvestManage.Domain.Transactions;

namespace InvestManage.Application.Transactions;

public interface ITransactionRepository
{
    Task<AccountInvestment?> FindActiveAssignmentAsync(
        Guid accountId,
        Guid investmentItemId,
        CancellationToken cancellationToken);

    Task<bool> CurrencyExistsAsync(string currencyCode, CancellationToken cancellationToken);

    Task<IReadOnlyList<Transaction>> ListForAssignmentAsync(
        Guid accountInvestmentId,
        CancellationToken cancellationToken);

    void Add(Transaction transaction);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
