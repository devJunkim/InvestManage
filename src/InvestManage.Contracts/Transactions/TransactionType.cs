using System.Text.Json.Serialization;

namespace InvestManage.Contracts.Transactions;

[JsonConverter(typeof(JsonStringEnumConverter<TransactionType>))]
public enum TransactionType
{
    Buy = 1,
    Sell = 2
}
