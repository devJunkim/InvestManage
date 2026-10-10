using InvestManage.Domain.Accounts;
using InvestManage.Domain.Prices;
using InvestManage.Domain.Transactions;
using InvestManage.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace InvestManage.Infrastructure.Tests.Persistence;

public sealed class InvestManageDbContextModelTests
{
    private readonly IModel _model = CreateContext().Model;

    [Fact]
    public void PriceHistory_HasUniqueInvestmentDateTypeSourceIndex()
    {
        var entity = _model.FindEntityType(typeof(PriceHistory));

        var index = Assert.Single(entity!.GetIndexes(), candidate =>
            candidate.Properties.Select(property => property.Name).SequenceEqual([
                nameof(PriceHistory.InvestmentItemId),
                nameof(PriceHistory.PriceDate),
                nameof(PriceHistory.Type),
                nameof(PriceHistory.Source)
            ]));

        Assert.True(index.IsUnique);
    }

    [Fact]
    public void Transaction_HasDeterministicLedgerIndex()
    {
        var entity = _model.FindEntityType(typeof(Transaction));

        Assert.Contains(entity!.GetIndexes(), candidate =>
            candidate.Properties.Select(property => property.Name).SequenceEqual([
                nameof(Transaction.AccountInvestmentId),
                nameof(Transaction.TradeDate),
                nameof(Transaction.CreatedAtUtc),
                nameof(Transaction.Id)
            ]));
    }

    [Fact]
    public void FinancialValues_HaveRequiredPrecision()
    {
        var price = _model.FindEntityType(typeof(PriceHistory))!.FindProperty(nameof(PriceHistory.Price));
        var transaction = _model.FindEntityType(typeof(Transaction))!;

        Assert.Equal(19, price!.GetPrecision());
        Assert.Equal(8, price.GetScale());
        Assert.Equal(28, transaction.FindProperty(nameof(Transaction.Quantity))!.GetPrecision());
        Assert.Equal(8, transaction.FindProperty(nameof(Transaction.Quantity))!.GetScale());
    }

    [Fact]
    public void FinancialHistoryRelationships_UseRestrictiveDeletes()
    {
        var priceForeignKey = Assert.Single(_model.FindEntityType(typeof(PriceHistory))!.GetForeignKeys());
        var transactionForeignKeys = _model.FindEntityType(typeof(Transaction))!.GetForeignKeys();
        var accountInvestmentForeignKeys = _model.FindEntityType(typeof(AccountInvestment))!.GetForeignKeys();

        Assert.Equal(DeleteBehavior.Restrict, priceForeignKey.DeleteBehavior);
        Assert.All(transactionForeignKeys, foreignKey => Assert.Equal(DeleteBehavior.Restrict, foreignKey.DeleteBehavior));
        Assert.All(accountInvestmentForeignKeys, foreignKey => Assert.Equal(DeleteBehavior.Restrict, foreignKey.DeleteBehavior));
    }

    [Fact]
    public void MigrationSnapshot_MatchesCurrentModel()
    {
        using var context = CreateContext();

        Assert.False(context.Database.HasPendingModelChanges());
    }

    private static InvestManageDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<InvestManageDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=InvestManageModelTests;Trusted_Connection=True")
            .Options;

        return new InvestManageDbContext(options);
    }
}
