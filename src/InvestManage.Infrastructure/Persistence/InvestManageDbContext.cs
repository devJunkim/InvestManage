using InvestManage.Domain.Accounts;
using InvestManage.Domain.Currencies;
using InvestManage.Domain.Investments;
using InvestManage.Domain.Prices;
using InvestManage.Domain.Transactions;
using InvestManage.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace InvestManage.Infrastructure.Persistence;

public sealed class InvestManageDbContext(DbContextOptions<InvestManageDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Currency> Currencies => Set<Currency>();

    public DbSet<InvestmentAccount> InvestmentAccounts => Set<InvestmentAccount>();

    public DbSet<InvestmentItem> InvestmentItems => Set<InvestmentItem>();

    public DbSet<AccountInvestment> AccountInvestments => Set<AccountInvestment>();

    public DbSet<Transaction> Transactions => Set<Transaction>();

    public DbSet<PriceHistory> PriceHistory => Set<PriceHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InvestManageDbContext).Assembly);
    }
}
