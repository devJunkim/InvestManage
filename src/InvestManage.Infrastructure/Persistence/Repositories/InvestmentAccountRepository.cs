using InvestManage.Application.Accounts;
using InvestManage.Domain.Accounts;
using Microsoft.EntityFrameworkCore;

namespace InvestManage.Infrastructure.Persistence.Repositories;

public sealed class InvestmentAccountRepository(InvestManageDbContext context)
    : IInvestmentAccountRepository
{
    public async Task<IReadOnlyList<InvestmentAccount>> ListAsync(
        Guid? userId,
        bool includeArchived,
        CancellationToken cancellationToken)
    {
        var query = context.InvestmentAccounts.AsNoTracking();

        if (userId.HasValue)
        {
            query = query.Where(account => account.UserId == userId.Value);
        }

        if (!includeArchived)
        {
            query = query.Where(account => !account.IsArchived);
        }

        return await query
            .OrderBy(account => account.Name)
            .ThenBy(account => account.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<InvestmentAccount?> FindAsync(
        Guid id,
        bool includeArchived,
        bool trackChanges,
        CancellationToken cancellationToken)
    {
        IQueryable<InvestmentAccount> query = context.InvestmentAccounts;
        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        if (!includeArchived)
        {
            query = query.Where(account => !account.IsArchived);
        }

        return await query.SingleOrDefaultAsync(account => account.Id == id, cancellationToken);
    }

    public Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken) =>
        context.Users.AnyAsync(user => user.Id == userId, cancellationToken);

    public Task<bool> CurrencyExistsAsync(string currencyCode, CancellationToken cancellationToken) =>
        context.Currencies.AnyAsync(currency => currency.Code == currencyCode, cancellationToken);

    public void Add(InvestmentAccount account) => context.InvestmentAccounts.Add(account);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        context.SaveChangesAsync(cancellationToken);
}
