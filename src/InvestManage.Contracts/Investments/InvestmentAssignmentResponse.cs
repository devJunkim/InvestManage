namespace InvestManage.Contracts.Investments;

public sealed record InvestmentAssignmentResponse(
    Guid Id,
    Guid InvestmentAccountId,
    Guid InvestmentItemId);
