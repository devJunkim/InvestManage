using InvestManage.Application.Transactions;
using InvestManage.Domain.Accounts;
using InvestManage.Domain.Transactions;
using Microsoft.EntityFrameworkCore;

namespace InvestManage.Infrastructure.Persistence.Repositories;

public sealed class TransactionRepository(InvestManageDbContext context) : ITransactionRepository
{
    public Task<AccountInvestment?> FindActiveAssignmentAsync(
        Guid accountId,
        Guid investmentItemId,
        CancellationToken cancellationToken) =>
        context.AccountInvestments
            .Join(
                context.InvestmentAccounts.Where(account => !account.IsArchived),
                assignment => assignment.InvestmentAccountId,
                account => account.Id,
                (assignment, _) => assignment)
            .Join(
                context.InvestmentItems.Where(item => !item.IsArchived),
                assignment => assignment.InvestmentItemId,
                item => item.Id,
                (assignment, _) => assignment)
            .AsNoTracking()
            .SingleOrDefaultAsync(
                assignment =>
                    assignment.InvestmentAccountId == accountId &&
                    assignment.InvestmentItemId == investmentItemId,
                cancellationToken);

    public Task<bool> CurrencyExistsAsync(string currencyCode, CancellationToken cancellationToken) =>
        context.Currencies.AnyAsync(currency => currency.Code == currencyCode, cancellationToken);

    public async Task<IReadOnlyList<Transaction>> ListForAssignmentAsync(
        Guid accountInvestmentId,
        CancellationToken cancellationToken) =>
        await context.Transactions
            .AsNoTracking()
            .Where(transaction => transaction.AccountInvestmentId == accountInvestmentId)
            .ToListAsync(cancellationToken);

    public void Add(Transaction transaction) => context.Transactions.Add(transaction);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        context.SaveChangesAsync(cancellationToken);
}
