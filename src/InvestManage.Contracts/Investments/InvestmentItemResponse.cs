namespace InvestManage.Contracts.Investments;

public sealed record InvestmentItemResponse(
    Guid Id,
    string Code,
    string Name,
    InvestmentType Type,
    string CurrencyCode,
    string? Provider,
    int PricePrecision,
    string? Notes,
    bool IsArchived);
