using InvestManage.Domain.Transactions;

namespace InvestManage.Application.Transactions;

public sealed record CreateTransactionCommand(
    Guid AccountId,
    Guid InvestmentItemId,
    TransactionType Type,
    DateOnly TradeDate,
    DateOnly? SettlementDate,
    decimal Quantity,
    decimal UnitPrice,
    decimal Fees,
    string? CurrencyCode,
    string? Notes);
