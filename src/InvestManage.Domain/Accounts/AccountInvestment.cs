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

    public Guid Id { get; }

    public Guid InvestmentAccountId { get; }

    public Guid InvestmentItemId { get; }
}
