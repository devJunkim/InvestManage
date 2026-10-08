using InvestManage.Contracts.Investments;

namespace InvestManage.Client.Services;

public sealed record InvestmentItemFilter(
    string? Search = null,
    InvestmentType? Type = null,
    string? CurrencyCode = null,
    string? Provider = null,
    bool IncludeArchived = false);
