namespace InvestManage.Contracts.Transactions;

public sealed record CreateTransactionRequest(
    TransactionType Type,
    DateOnly TradeDate,
    DateOnly? SettlementDate,
    decimal Quantity,
    decimal UnitPrice,
    decimal Fees,
    string? CurrencyCode,
    string? Notes);
