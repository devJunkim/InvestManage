namespace InvestManage.Domain.Investments;

public sealed class Investment
{
    public Investment(
        Guid id,
        string symbol,
        string name,
        InvestmentType type,
        string currency)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("An investment id is required.", nameof(id));
        }

        Id = id;
        Symbol = Required(symbol, nameof(symbol)).ToUpperInvariant();
        Name = Required(name, nameof(name));
        Type = type;
        Currency = Required(currency, nameof(currency)).ToUpperInvariant();
    }

    public Guid Id { get; }

    public string Symbol { get; }

    public string Name { get; }

    public InvestmentType Type { get; }

    public string Currency { get; }

    private static string Required(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A value is required.", parameterName);
        }

        return value.Trim();
    }
}

