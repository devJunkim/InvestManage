namespace InvestManage.Contracts.Transactions;

public sealed record TransactionResponse(
    Guid Id,
    Guid AccountId,
    Guid InvestmentItemId,
    TransactionType Type,
    DateOnly TradeDate,
    DateOnly? SettlementDate,
    decimal Quantity,
    decimal UnitPrice,
    decimal Fees,
    string CurrencyCode,
    string? Notes,
    DateTimeOffset CreatedAtUtc);
