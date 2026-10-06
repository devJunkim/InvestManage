using InvestManage.Domain.Accounts;

namespace InvestManage.Application.Investments;

public sealed record InvestmentAssignmentResult(AccountInvestment Assignment, bool Created);
