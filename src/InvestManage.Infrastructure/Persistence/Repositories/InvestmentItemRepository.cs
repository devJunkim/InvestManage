using InvestManage.Application.Investments;
using InvestManage.Domain.Accounts;
using InvestManage.Domain.Investments;
using Microsoft.EntityFrameworkCore;

namespace InvestManage.Infrastructure.Persistence.Repositories;

public sealed class InvestmentItemRepository(InvestManageDbContext context)
    : IInvestmentItemRepository
{
    public async Task<IReadOnlyList<InvestmentItem>> SearchAsync(
        string? search,
        InvestmentType? type,
        string? currencyCode,
        string? provider,
        bool includeArchived,
        CancellationToken cancellationToken)
    {
        var query = context.InvestmentItems.AsNoTracking();
        if (!includeArchived)
        {
            query = query.Where(item => !item.IsArchived);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.ToUpperInvariant();
            query = query.Where(item =>
                item.NormalizedCode.Contains(normalizedSearch) ||
                item.Name.Contains(search) ||
                (item.Provider != null && item.Provider.Contains(search)));
        }

        if (type.HasValue)
        {
            query = query.Where(item => item.Type == type.Value);
        }

        if (!string.IsNullOrWhiteSpace(currencyCode))
        {
            query = query.Where(item => item.CurrencyCode == currencyCode);
        }

        if (!string.IsNullOrWhiteSpace(provider))
        {
            query = query.Where(item => item.Provider == provider);
        }

        return await query
            .OrderBy(item => item.NormalizedCode)
            .ThenBy(item => item.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<InvestmentItem>> ListForAccountAsync(
        Guid accountId,
        bool includeArchived,
        CancellationToken cancellationToken)
    {
        var query = context.AccountInvestments
            .Where(assignment => assignment.InvestmentAccountId == accountId)
            .Join(
                context.InvestmentItems,
                assignment => assignment.InvestmentItemId,
                item => item.Id,
                (_, item) => item)
            .AsNoTracking();

        if (!includeArchived)
        {
            query = query.Where(item => !item.IsArchived);
        }

        return await query
            .OrderBy(item => item.NormalizedCode)
            .ThenBy(item => item.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<InvestmentItem?> FindAsync(
        Guid id,
        bool includeArchived,
        bool trackChanges,
        CancellationToken cancellationToken)
    {
        IQueryable<InvestmentItem> query = context.InvestmentItems;
        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        if (!includeArchived)
        {
            query = query.Where(item => !item.IsArchived);
        }

        return await query.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
    }

    public Task<bool> CurrencyExistsAsync(string currencyCode, CancellationToken cancellationToken) =>
        context.Currencies.AnyAsync(currency => currency.Code == currencyCode, cancellationToken);

    public Task<bool> AccountExistsAsync(
        Guid accountId,
        bool includeArchived,
        CancellationToken cancellationToken) =>
        context.InvestmentAccounts.AnyAsync(
            account => account.Id == accountId && (includeArchived || !account.IsArchived),
            cancellationToken);

    public Task<AccountInvestment?> FindAssignmentAsync(
        Guid accountId,
        Guid investmentItemId,
        CancellationToken cancellationToken) =>
        context.AccountInvestments
            .AsNoTracking()
            .SingleOrDefaultAsync(
                assignment =>
                    assignment.InvestmentAccountId == accountId &&
                    assignment.InvestmentItemId == investmentItemId,
                cancellationToken);

    public void Add(InvestmentItem investmentItem) => context.InvestmentItems.Add(investmentItem);

    public void Add(AccountInvestment assignment) => context.AccountInvestments.Add(assignment);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        context.SaveChangesAsync(cancellationToken);
}
