using InvestManage.Domain.Common;

namespace InvestManage.Domain.Transactions;

public sealed class Transaction
{
    public Transaction(
        Guid id,
        Guid accountInvestmentId,
        TransactionType type,
        DateOnly tradeDate,
        decimal quantity,
        decimal unitPrice,
        decimal fees,
        string currencyCode,
        DateOnly? settlementDate = null)
    {
        if (!Enum.IsDefined(type))
        {
            throw new ArgumentOutOfRangeException(nameof(type), type, "The transaction type is not supported.");
        }

        if (settlementDate < tradeDate)
        {
            throw new ArgumentException("The settlement date cannot be before the trade date.", nameof(settlementDate));
        }

        Id = Guard.Required(id, nameof(id));
        AccountInvestmentId = Guard.Required(accountInvestmentId, nameof(accountInvestmentId));
        Type = type;
        TradeDate = tradeDate;
        SettlementDate = settlementDate;
        Quantity = Guard.Positive(quantity, nameof(quantity));
        UnitPrice = Guard.Positive(unitPrice, nameof(unitPrice));
        Fees = Guard.NotNegative(fees, nameof(fees));
        CurrencyCode = NormalizeCurrencyCode(currencyCode);
    }

    public Guid Id { get; }

    public Guid AccountInvestmentId { get; }

    public TransactionType Type { get; }

    public DateOnly TradeDate { get; }

    public DateOnly? SettlementDate { get; }

    public decimal Quantity { get; }

    public decimal UnitPrice { get; }

    public decimal Fees { get; }

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
