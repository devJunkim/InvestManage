using InvestManage.Domain.Common;

namespace InvestManage.Domain.Currencies;

public sealed class Currency
{
    public Currency(string code, string name)
    {
        var normalizedCode = Guard.Required(code, nameof(code)).ToUpperInvariant();

        if (normalizedCode.Length != 3 || normalizedCode.Any(character => !char.IsAsciiLetterUpper(character)))
        {
            throw new ArgumentException("A currency code must contain exactly three ASCII letters.", nameof(code));
        }

        Code = normalizedCode;
        Name = Guard.Required(name, nameof(name));
    }

    public string Code { get; }

    public string Name { get; }
}
