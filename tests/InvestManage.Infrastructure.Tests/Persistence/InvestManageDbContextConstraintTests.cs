using InvestManage.Domain.Investments;
using InvestManage.Domain.Prices;
using InvestManage.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace InvestManage.Infrastructure.Tests.Persistence;

public sealed class InvestManageDbContextConstraintTests
{
    [Fact]
    public void PriceHistory_DatabaseConstraintRejectsDuplicateInvestmentDateTypeAndSource()
    {
        using var connection = OpenDatabase();
        using var context = CreateContext(connection);
        var investment = AddInvestment(context);

        context.PriceHistory.Add(new PriceHistory(
            Guid.NewGuid(),
            investment.Id,
            new DateOnly(2026, 10, 5),
            20.0812m,
            PriceType.NetAssetValue,
            "Manual"));
        context.SaveChanges();

        context.PriceHistory.Add(new PriceHistory(
            Guid.NewGuid(),
            investment.Id,
            new DateOnly(2026, 10, 5),
            20.1000m,
            PriceType.NetAssetValue,
            "Manual"));

        Assert.Throws<DbUpdateException>(() => context.SaveChanges());
    }

    [Fact]
    public void PriceHistory_ForeignKeyRejectsUnknownInvestment()
    {
        using var connection = OpenDatabase();
        using var context = CreateContext(connection);

        context.PriceHistory.Add(new PriceHistory(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 10, 5),
            20.0812m,
            PriceType.NetAssetValue,
            "Manual"));

        Assert.Throws<DbUpdateException>(() => context.SaveChanges());
    }

    [Fact]
    public void InvestmentWithPriceHistory_CannotBeDeleted()
    {
        using var connection = OpenDatabase();
        Guid investmentId;

        using (var setup = CreateContext(connection))
        {
            var investment = AddInvestment(setup);
            investmentId = investment.Id;
            setup.PriceHistory.Add(new PriceHistory(
                Guid.NewGuid(),
                investment.Id,
                new DateOnly(2026, 10, 5),
                20.0812m,
                PriceType.NetAssetValue,
                "Manual"));
            setup.SaveChanges();
        }

        using var context = CreateContext(connection);
        var persistedInvestment = context.InvestmentItems.Single(item => item.Id == investmentId);
        context.InvestmentItems.Remove(persistedInvestment);

        Assert.Throws<DbUpdateException>(() => context.SaveChanges());
        Assert.Single(context.PriceHistory.AsNoTracking());
    }

    private static InvestmentItem AddInvestment(InvestManageDbContext context)
    {
        var investment = new InvestmentItem(
            Guid.NewGuid(),
            "TDB3046",
            "TD mutual fund",
            InvestmentType.MutualFund,
            "CAD");

        context.InvestmentItems.Add(investment);
        context.SaveChanges();
        return investment;
    }

    private static SqliteConnection OpenDatabase()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        using var context = CreateContext(connection);
        context.Database.EnsureCreated();
        return connection;
    }

    private static InvestManageDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<InvestManageDbContext>()
            .UseSqlite(connection)
            .Options;

        return new InvestManageDbContext(options);
    }
}
