namespace InvestManage.Domain.Accounts;

public enum InvestmentAccountType
{
    TaxFreeSavingsAccount = 1,
    RegisteredRetirementSavingsPlan = 2,
    RegisteredEducationSavingsPlan = 3,
    RegisteredRetirementIncomeFund = 4,
    NonRegistered = 5,
    Other = 99
}
