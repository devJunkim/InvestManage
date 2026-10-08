using InvestManage.Application.Accounts;
using InvestManage.Application.Investments;
using InvestManage.Domain.Accounts;
using InvestManage.Domain.Investments;
using InvestManage.Domain.Prices;
using InvestManage.Domain.Transactions;
using InvestManage.Domain.Users;
using InvestManage.Infrastructure.Persistence;
using InvestManage.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace InvestManage.Api.Tests.Integration;

public sealed class InvestManageApiFactory : WebApplicationFactory<Program>
{
    public static readonly Guid UserId = Guid.Parse("7fcb8eb1-a0ec-49ad-8d25-72042c1b269f");

    private readonly SqliteConnection connection = new("Data Source=:memory:");

    public InvestManageApiFactory()
    {
        connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<InvestManageDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<InvestManageDbContext>>();
            services.RemoveAll<InvestManageDbContext>();
            services.RemoveAll<IInvestmentAccountRepository>();
            services.RemoveAll<IInvestmentItemRepository>();

            services.AddDbContext<InvestManageDbContext>(options => options.UseSqlite(connection));
            services.AddScoped<IInvestmentAccountRepository, InvestmentAccountRepository>();
            services.AddScoped<IInvestmentItemRepository, InvestmentItemRepository>();
        });
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InvestManageDbContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        context.Users.Add(new User(
            UserId,
            "Integration",
            "User",
            "integration@example.test",
            "INTEGRATION@EXAMPLE.TEST",
            "integration",
            "INTEGRATION",
            "not-used-by-account-tests"));
        await context.SaveChangesAsync();
    }

    public async Task AddFinancialHistoryAsync(Guid accountId)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InvestManageDbContext>();

        var investment = new InvestmentItem(
            Guid.NewGuid(),
            "TDB3046",
            "TD mutual fund",
            InvestmentType.MutualFund,
            "CAD");
        var holding = new AccountInvestment(Guid.NewGuid(), accountId, investment.Id);
        var transaction = new Transaction(
            Guid.NewGuid(),
            holding.Id,
            TransactionType.Buy,
            new DateOnly(2026, 10, 6),
            10.5m,
            20.0812m,
            0m,
            "CAD");

        context.AddRange(investment, holding, transaction);
        await context.SaveChangesAsync();
    }

    public async Task<string> ReadPasswordHashAsync(Guid userId)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InvestManageDbContext>();
        return await context.Users
            .AsNoTracking()
            .Where(user => user.Id == userId)
            .Select(user => user.PasswordHash)
            .SingleAsync();
    }

    public async Task<(bool IsArchived, int Holdings, int Transactions)> ReadHistoryStateAsync(
        Guid accountId)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InvestManageDbContext>();
        var account = await context.InvestmentAccounts.AsNoTracking().SingleAsync(item => item.Id == accountId);
        var holdingIds = await context.AccountInvestments
            .Where(item => item.InvestmentAccountId == accountId)
            .Select(item => item.Id)
            .ToListAsync();

        return (
            account.IsArchived,
            holdingIds.Count,
            await context.Transactions.CountAsync(item => holdingIds.Contains(item.AccountInvestmentId)));
    }

    public async Task AddPriceHistoryAsync(Guid investmentItemId)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InvestManageDbContext>();

        context.PriceHistory.Add(
            new PriceHistory(
                Guid.NewGuid(),
                investmentItemId,
                new DateOnly(2026, 10, 6),
                20.0812m,
                PriceType.NetAssetValue,
                "Manual"));
        await context.SaveChangesAsync();
    }

    public async Task<(int Items, int Assignments, int Prices)> ReadInvestmentStateAsync(
        Guid investmentItemId)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InvestManageDbContext>();

        return (
            await context.InvestmentItems.CountAsync(item => item.Id == investmentItemId),
            await context.AccountInvestments.CountAsync(item => item.InvestmentItemId == investmentItemId),
            await context.PriceHistory.CountAsync(item => item.InvestmentItemId == investmentItemId));
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            connection.Dispose();
        }
    }
}
