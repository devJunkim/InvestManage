using InvestManage.Domain.Common;

namespace InvestManage.Domain.Prices;

public sealed class PriceHistory
{
    public PriceHistory(
        Guid id,
        Guid investmentItemId,
        DateOnly priceDate,
        decimal price,
        PriceType type,
        string source)
    {
        if (!Enum.IsDefined(type))
        {
            throw new ArgumentOutOfRangeException(nameof(type), type, "The price type is not supported.");
        }

        Id = Guard.Required(id, nameof(id));
        InvestmentItemId = Guard.Required(investmentItemId, nameof(investmentItemId));
        PriceDate = priceDate;
        Price = Guard.Positive(price, nameof(price));
        Type = type;
        Source = Guard.Required(source, nameof(source));
    }

    public Guid Id { get; }

    public Guid InvestmentItemId { get; }

    public DateOnly PriceDate { get; }

    public decimal Price { get; }

    public PriceType Type { get; }

    public string Source { get; }
}
