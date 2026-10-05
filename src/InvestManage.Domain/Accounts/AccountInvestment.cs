using InvestManage.Domain.Common;

namespace InvestManage.Domain.Accounts;

public sealed class AccountInvestment
{
    public AccountInvestment(Guid id, Guid investmentAccountId, Guid investmentItemId)
    {
        Id = Guard.Required(id, nameof(id));
        InvestmentAccountId = Guard.Required(investmentAccountId, nameof(investmentAccountId));
        InvestmentItemId = Guard.Required(investmentItemId, nameof(investmentItemId));
    }

    public Guid Id { get; private set; }

    public Guid InvestmentAccountId { get; private set; }

    public Guid InvestmentItemId { get; private set; }
}
