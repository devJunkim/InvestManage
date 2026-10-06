namespace InvestManage.Contracts.Accounts;

public sealed record CreateInvestmentAccountRequest(
    Guid UserId,
    string? Name,
    InvestmentAccountType Type,
    string? CurrencyCode);
