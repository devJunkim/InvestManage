using InvestManage.Domain.Investments;

namespace InvestManage.Domain.Tests.Investments;

public sealed class InvestmentItemTests
{
    [Fact]
    public void Constructor_PreservesDisplayCodeAndCreatesNormalizedCode()
    {
        var investment = new InvestmentItem(
            Guid.NewGuid(),
            " ml1436 ",
            "Manulife investment",
            InvestmentType.SegregatedFund,
            " cad ",
            " Manulife ",
            4,
            " Long-term holding ");

        Assert.Equal("ml1436", investment.Code);
        Assert.Equal("ML1436", investment.NormalizedCode);
        Assert.Equal("CAD", investment.CurrencyCode);
        Assert.Equal("Manulife", investment.Provider);
        Assert.Equal(4, investment.PricePrecision);
        Assert.Equal("Long-term holding", investment.Notes);
        Assert.False(investment.IsArchived);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_RejectsMissingCode(string code)
    {
        Assert.Throws<ArgumentException>(() => new InvestmentItem(
            Guid.NewGuid(),
            code,
            "Manulife investment",
            InvestmentType.SegregatedFund,
            "CAD"));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(9)]
    public void Constructor_RejectsUnsupportedPricePrecision(int precision)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new InvestmentItem(
            Guid.NewGuid(),
            "TDB3046",
            "TD mutual fund",
            InvestmentType.MutualFund,
            "CAD",
            pricePrecision: precision));
    }

    [Fact]
    public void UpdateDetails_UpdatesNormalizedAndOptionalValues()
    {
        var investment = new InvestmentItem(
            Guid.NewGuid(),
            "TDB3046",
            "TD mutual fund",
            InvestmentType.MutualFund,
            "CAD");

        investment.UpdateDetails(
            " ml1436 ",
            " Manulife investment ",
            InvestmentType.SegregatedFund,
            " usd ",
            " Manulife ",
            6,
            " Updated note ");

        Assert.Equal("ml1436", investment.Code);
        Assert.Equal("ML1436", investment.NormalizedCode);
        Assert.Equal("Manulife investment", investment.Name);
        Assert.Equal(InvestmentType.SegregatedFund, investment.Type);
        Assert.Equal("USD", investment.CurrencyCode);
        Assert.Equal("Manulife", investment.Provider);
        Assert.Equal(6, investment.PricePrecision);
        Assert.Equal("Updated note", investment.Notes);
    }

    [Fact]
    public void ArchiveAndReactivate_ChangeLifecycleState()
    {
        var investment = new InvestmentItem(
            Guid.NewGuid(),
            "TDB3046",
            "TD mutual fund",
            InvestmentType.MutualFund,
            "CAD");

        investment.Archive();
        Assert.True(investment.IsArchived);

        investment.Reactivate();
        Assert.False(investment.IsArchived);
    }
}
