using System.Text.Json.Serialization;

namespace InvestManage.Contracts.Investments;

[JsonConverter(typeof(JsonStringEnumConverter<InvestmentType>))]
public enum InvestmentType
{
    Stock = 1,
    ExchangeTradedFund = 2,
    MutualFund = 3,
    SegregatedFund = 4,
    Other = 99
}
