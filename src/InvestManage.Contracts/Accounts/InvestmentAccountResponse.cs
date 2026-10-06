namespace InvestManage.Contracts.Accounts;

public sealed record InvestmentAccountResponse(
    Guid Id,
    Guid UserId,
    string Name,
    InvestmentAccountType Type,
    string CurrencyCode,
    bool IsArchived);
