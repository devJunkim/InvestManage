using InvestManage.Domain.Transactions;

namespace InvestManage.Application.Transactions;

public sealed record TransactionResult(
    Transaction Transaction,
    Guid AccountId,
    Guid InvestmentItemId);
