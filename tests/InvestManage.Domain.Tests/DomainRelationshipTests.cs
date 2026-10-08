using InvestManage.Domain.Accounts;
using InvestManage.Domain.Currencies;
using InvestManage.Domain.Investments;
using InvestManage.Domain.Users;

namespace InvestManage.Domain.Tests;

public sealed class DomainRelationshipTests
{
    [Fact]
    public void Entities_PreserveIdentifiersForCoreRelationships()
    {
        var user = new User(
            Guid.NewGuid(),
            "Jun",
            "Kim",
            "jun@example.test",
            "JUN@EXAMPLE.TEST",
            "junkim",
            "JUNKIM",
            "password-hash");
        var account = new InvestmentAccount(
            Guid.NewGuid(),
            user.Id,
            "Retirement",
            InvestmentAccountType.RegisteredRetirementSavingsPlan,
            "CAD");
        var item = new InvestmentItem(
            Guid.NewGuid(),
            "TDB3046",
            "TD mutual fund",
            InvestmentType.MutualFund,
            "CAD");
        var holding = new AccountInvestment(Guid.NewGuid(), account.Id, item.Id);

        Assert.Equal(user.Id, account.UserId);
        Assert.Equal(account.Id, holding.InvestmentAccountId);
        Assert.Equal(item.Id, holding.InvestmentItemId);
    }

    [Fact]
    public void Currency_NormalizesIsoCode()
    {
        var currency = new Currency(" cad ", "Canadian dollar");

        Assert.Equal("CAD", currency.Code);
    }

    [Theory]
    [InlineData("CA")]
    [InlineData("CAD1")]
    [InlineData("C$D")]
    public void Currency_RejectsInvalidCodes(string code)
    {
        Assert.Throws<ArgumentException>(() => new Currency(code, "Invalid"));
    }
}
