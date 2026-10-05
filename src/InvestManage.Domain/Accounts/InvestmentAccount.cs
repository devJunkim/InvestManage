using InvestManage.Domain.Common;

namespace InvestManage.Domain.Accounts;

public sealed class InvestmentAccount
{
    public InvestmentAccount(
        Guid id,
        Guid userId,
        string name,
        InvestmentAccountType type,
        string currencyCode)
    {
        if (!Enum.IsDefined(type))
        {
            throw new ArgumentOutOfRangeException(nameof(type), type, "The account type is not supported.");
        }

        Id = Guard.Required(id, nameof(id));
        UserId = Guard.Required(userId, nameof(userId));
        Name = Guard.Required(name, nameof(name));
        Type = type;
        CurrencyCode = NormalizeCurrencyCode(currencyCode);
    }

    public Guid Id { get; }

    public Guid UserId { get; }

    public string Name { get; }

    public InvestmentAccountType Type { get; }

    public string CurrencyCode { get; }

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
