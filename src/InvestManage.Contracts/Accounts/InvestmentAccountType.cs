using System.Text.Json.Serialization;

namespace InvestManage.Contracts.Accounts;

[JsonConverter(typeof(JsonStringEnumConverter<InvestmentAccountType>))]
public enum InvestmentAccountType
{
    TaxFreeSavingsAccount = 1,
    RegisteredRetirementSavingsPlan = 2,
    RegisteredEducationSavingsPlan = 3,
    RegisteredRetirementIncomeFund = 4,
    NonRegistered = 5,
    Other = 99
}
