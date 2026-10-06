namespace InvestManage.Contracts.Accounts;

public sealed record UpdateInvestmentAccountRequest(
    string? Name,
    InvestmentAccountType Type,
    string? CurrencyCode);
