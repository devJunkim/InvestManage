using InvestManage.Domain.Transactions;

namespace InvestManage.Domain.Tests.Transactions;

public sealed class TransactionTests
{
    [Fact]
    public void Constructor_PreservesFractionalQuantityAndPricePrecision()
    {
        var transaction = new Transaction(
            Guid.NewGuid(),
            Guid.NewGuid(),
            TransactionType.Buy,
            new DateOnly(2026, 10, 3),
            12.345678m,
            20.0812m,
            9.99m,
            "cad",
            new DateOnly(2026, 10, 5));

        Assert.Equal(12.345678m, transaction.Quantity);
        Assert.Equal(20.0812m, transaction.UnitPrice);
        Assert.Equal("CAD", transaction.CurrencyCode);
        Assert.IsType<DateOnly>(transaction.TradeDate);
    }

    [Fact]
    public void Constructor_RejectsSettlementBeforeTradeDate()
    {
        Assert.Throws<ArgumentException>(() => new Transaction(
            Guid.NewGuid(),
            Guid.NewGuid(),
            TransactionType.Buy,
            new DateOnly(2026, 10, 5),
            1.25m,
            20.0812m,
            0m,
            "CAD",
            new DateOnly(2026, 10, 4)));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    public void Constructor_RejectsNonPositiveQuantity(string value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Transaction(
            Guid.NewGuid(),
            Guid.NewGuid(),
            TransactionType.Sell,
            new DateOnly(2026, 10, 5),
            decimal.Parse(value, System.Globalization.CultureInfo.InvariantCulture),
            20.0812m,
            0m,
            "CAD"));
    }
}
