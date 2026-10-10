using InvestManage.Domain.Common;

namespace InvestManage.Domain.Transactions;

public sealed class Transaction
{
    private Transaction()
    {
        CurrencyCode = null!;
    }

    public Transaction(
        Guid id,
        Guid accountInvestmentId,
        TransactionType type,
        DateOnly tradeDate,
        decimal quantity,
        decimal unitPrice,
        decimal fees,
        string currencyCode,
        DateOnly? settlementDate = null,
        string? notes = null,
        DateTimeOffset? createdAtUtc = null)
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
        Notes = NormalizeNotes(notes);
        CreatedAtUtc = createdAtUtc ?? DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid AccountInvestmentId { get; private set; }

    public TransactionType Type { get; private set; }

    public DateOnly TradeDate { get; private set; }

    public DateOnly? SettlementDate { get; private set; }

    public decimal Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }

    public decimal Fees { get; private set; }

    public string CurrencyCode { get; private set; }

    public string? Notes { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    private static string NormalizeCurrencyCode(string currencyCode)
    {
        var currency = Guard.Required(currencyCode, nameof(currencyCode)).ToUpperInvariant();

        if (currency.Length != 3 || currency.Any(character => !char.IsAsciiLetterUpper(character)))
        {
            throw new ArgumentException("A currency code must contain exactly three ASCII letters.", nameof(currencyCode));
        }

        return currency;
    }

    private static string? NormalizeNotes(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
        {
            return null;
        }

        var normalized = notes.Trim();
        if (normalized.Length > 2000)
        {
            throw new ArgumentException("Notes cannot exceed 2000 characters.", nameof(notes));
        }

        return normalized;
    }
}
