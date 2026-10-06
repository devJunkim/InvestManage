using InvestManage.Domain.Accounts;

namespace InvestManage.Application.Accounts;

public sealed record CreateInvestmentAccountCommand(
    Guid UserId,
    string? Name,
    InvestmentAccountType Type,
    string? CurrencyCode);

public sealed record UpdateInvestmentAccountCommand(
    string? Name,
    InvestmentAccountType Type,
    string? CurrencyCode);
