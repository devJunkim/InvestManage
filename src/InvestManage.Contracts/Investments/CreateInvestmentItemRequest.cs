namespace InvestManage.Contracts.Investments;

public sealed record CreateInvestmentItemRequest(
    string? Code,
    string? Name,
    InvestmentType Type,
    string? CurrencyCode,
    string? Provider,
    int PricePrecision,
    string? Notes);
