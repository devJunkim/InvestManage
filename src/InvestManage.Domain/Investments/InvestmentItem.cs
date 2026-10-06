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
        string? provider = null,
        int pricePrecision = 4,
        string? notes = null)
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
        Provider = NormalizeOptional(provider);
        PricePrecision = ValidatePricePrecision(pricePrecision);
        Notes = NormalizeOptional(notes);
        IsArchived = false;
    }

    public Guid Id { get; private set; }

    public string Code { get; private set; }

    public string NormalizedCode { get; private set; }

    public string Name { get; private set; }

    public InvestmentType Type { get; private set; }

    public string CurrencyCode { get; private set; }

    public string? Provider { get; private set; }

    public int PricePrecision { get; private set; }

    public string? Notes { get; private set; }

    public bool IsArchived { get; private set; }

    public void UpdateDetails(
        string code,
        string name,
        InvestmentType type,
        string currencyCode,
        string? provider,
        int pricePrecision,
        string? notes)
    {
        if (!Enum.IsDefined(type))
        {
            throw new ArgumentOutOfRangeException(nameof(type), type, "The investment type is not supported.");
        }

        Code = Guard.Required(code, nameof(code));
        NormalizedCode = Code.ToUpperInvariant();
        Name = Guard.Required(name, nameof(name));
        Type = type;
        CurrencyCode = NormalizeCurrencyCode(currencyCode);
        Provider = NormalizeOptional(provider);
        PricePrecision = ValidatePricePrecision(pricePrecision);
        Notes = NormalizeOptional(notes);
    }

    public void Archive() => IsArchived = true;

    public void Reactivate() => IsArchived = false;

    private static string NormalizeCurrencyCode(string currencyCode)
    {
        var currency = Guard.Required(currencyCode, nameof(currencyCode)).ToUpperInvariant();

        if (currency.Length != 3 || currency.Any(character => !char.IsAsciiLetterUpper(character)))
        {
            throw new ArgumentException("A currency code must contain exactly three ASCII letters.", nameof(currencyCode));
        }

        return currency;
    }

    private static int ValidatePricePrecision(int pricePrecision)
    {
        if (pricePrecision is < 0 or > 8)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pricePrecision),
                pricePrecision,
                "Price precision must be between zero and eight decimal places.");
        }

        return pricePrecision;
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
