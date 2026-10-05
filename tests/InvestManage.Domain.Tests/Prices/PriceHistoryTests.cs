using InvestManage.Domain.Prices;

namespace InvestManage.Domain.Tests.Prices;

public sealed class PriceHistoryTests
{
    [Fact]
    public void Constructor_PreservesFourDecimalPriceExactly()
    {
        var price = new PriceHistory(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 10, 5),
            20.0812m,
            PriceType.NetAssetValue,
            "Manual");

        Assert.Equal(20.0812m, price.Price);
        Assert.IsType<DateOnly>(price.PriceDate);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-0.0001")]
    public void Constructor_RejectsNonPositivePrice(string value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PriceHistory(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 10, 5),
            decimal.Parse(value, System.Globalization.CultureInfo.InvariantCulture),
            PriceType.NetAssetValue,
            "Manual"));
    }
}
