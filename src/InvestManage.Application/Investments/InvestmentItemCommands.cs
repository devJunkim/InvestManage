using InvestManage.Domain.Investments;

namespace InvestManage.Application.Investments;

public sealed record CreateInvestmentItemCommand(
    string? Code,
    string? Name,
    InvestmentType Type,
    string? CurrencyCode,
    string? Provider,
    int PricePrecision,
    string? Notes);

public sealed record UpdateInvestmentItemCommand(
    string? Code,
    string? Name,
    InvestmentType Type,
    string? CurrencyCode,
    string? Provider,
    int PricePrecision,
    string? Notes);

public sealed record InvestmentItemSearch(
    string? Search,
    InvestmentType? Type,
    string? CurrencyCode,
    string? Provider,
    bool IncludeArchived);
