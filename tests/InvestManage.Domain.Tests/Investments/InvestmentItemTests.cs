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
            " cad ");

        Assert.Equal("ml1436", investment.Code);
        Assert.Equal("ML1436", investment.NormalizedCode);
        Assert.Equal("CAD", investment.CurrencyCode);
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
}
