using InvestManage.Domain.Investments;

namespace InvestManage.Domain.Tests.Investments;

public sealed class InvestmentTests
{
    [Fact]
    public void Constructor_NormalizesSymbolAndCurrency()
    {
        var investment = new Investment(
            Guid.NewGuid(),
            " ml1436 ",
            "Manulife investment",
            InvestmentType.SegregatedFund,
            " cad ");

        Assert.Equal("ML1436", investment.Symbol);
        Assert.Equal("CAD", investment.Currency);
    }

    [Fact]
    public void Constructor_RejectsMissingSymbol()
    {
        Assert.Throws<ArgumentException>(() => new Investment(
            Guid.NewGuid(),
            " ",
            "Manulife investment",
            InvestmentType.SegregatedFund,
            "CAD"));
    }
}

