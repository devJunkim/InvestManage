using InvestManage.Domain.Common;

namespace InvestManage.Domain.Investments;

public sealed class InvestmentItem
{
    public InvestmentItem(
        Guid id,
        string code,
        string name,
        InvestmentType type,
        string currencyCode,
        string? provider = null)
    {
        if (!Enum.IsDefined(type))
        {
            throw new ArgumentOutOfRangeException(nameof(type), type, "The investment type is not supported.");
        }

        Id = Guard.Required(id, nameof(id));
        Code = Guard.Required(code, nameof(code));
        NormalizedCode = Code.ToUpperInvariant();
        Name = Guard.Required(name, nameof(name));
        Type = type;
        CurrencyCode = NormalizeCurrencyCode(currencyCode);
        Provider = string.IsNullOrWhiteSpace(provider) ? null : provider.Trim();
    }

    public Guid Id { get; }

    public string Code { get; }

    public string NormalizedCode { get; }

    public string Name { get; }

    public InvestmentType Type { get; }

    public string CurrencyCode { get; }

    public string? Provider { get; }

    private static string NormalizeCurrencyCode(string currencyCode)
    {
        var currency = Guard.Required(currencyCode, nameof(currencyCode)).ToUpperInvariant();

        if (currency.Length != 3 || currency.Any(character => !char.IsAsciiLetterUpper(character)))
        {
            throw new ArgumentException("A currency code must contain exactly three ASCII letters.", nameof(currencyCode));
        }

        return currency;
    }
}
