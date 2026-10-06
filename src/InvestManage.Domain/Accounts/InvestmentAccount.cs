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
        IsArchived = false;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string Name { get; private set; }

    public InvestmentAccountType Type { get; private set; }

    public string CurrencyCode { get; private set; }

    public bool IsArchived { get; private set; }

    public void UpdateDetails(
        string name,
        InvestmentAccountType type,
        string currencyCode)
    {
        if (!Enum.IsDefined(type))
        {
            throw new ArgumentOutOfRangeException(nameof(type), type, "The account type is not supported.");
        }

        Name = Guard.Required(name, nameof(name));
        Type = type;
        CurrencyCode = NormalizeCurrencyCode(currencyCode);
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
}
