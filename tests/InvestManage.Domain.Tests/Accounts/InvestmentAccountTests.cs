using InvestManage.Domain.Accounts;

namespace InvestManage.Domain.Tests.Accounts;

public sealed class InvestmentAccountTests
{
    [Fact]
    public void UpdateDetails_NormalizesValues()
    {
        var account = CreateAccount();

        account.UpdateDetails(
            "  Long-term retirement  ",
            InvestmentAccountType.RegisteredRetirementIncomeFund,
            " usd ");

        Assert.Equal("Long-term retirement", account.Name);
        Assert.Equal(InvestmentAccountType.RegisteredRetirementIncomeFund, account.Type);
        Assert.Equal("USD", account.CurrencyCode);
    }

    [Fact]
    public void ArchiveAndReactivate_ChangeLifecycleStateWithoutChangingIdentity()
    {
        var account = CreateAccount();
        var id = account.Id;

        account.Archive();
        Assert.True(account.IsArchived);

        account.Reactivate();
        Assert.False(account.IsArchived);
        Assert.Equal(id, account.Id);
    }

    private static InvestmentAccount CreateAccount() =>
        new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Retirement",
            InvestmentAccountType.RegisteredRetirementSavingsPlan,
            "CAD");
}
