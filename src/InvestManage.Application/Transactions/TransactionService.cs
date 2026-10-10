using InvestManage.Application.Common;
using InvestManage.Domain.Transactions;

namespace InvestManage.Application.Transactions;

public sealed class TransactionService(ITransactionRepository repository)
{
    public async Task<TransactionResult> CreateAsync(
        CreateTransactionCommand command,
        CancellationToken cancellationToken = default)
    {
        var details = Validate(command);
        var assignment = await repository.FindActiveAssignmentAsync(
            command.AccountId,
            command.InvestmentItemId,
            cancellationToken);
        if (assignment is null)
        {
            throw new ResourceNotFoundException(
                "Active account investment assignment",
                command.InvestmentItemId);
        }

        if (!await repository.CurrencyExistsAsync(details.CurrencyCode, cancellationToken))
        {
            throw Validation("currencyCode", "The specified currency does not exist.");
        }

        var transaction = new Transaction(
            Guid.NewGuid(),
            assignment.Id,
            command.Type,
            command.TradeDate,
            command.Quantity,
            command.UnitPrice,
            command.Fees,
            details.CurrencyCode,
            command.SettlementDate,
            details.Notes);

        if (command.Type == TransactionType.Sell)
        {
            var ledger = await repository.ListForAssignmentAsync(assignment.Id, cancellationToken);
            EnsureLedgerNeverOversells(ledger.Append(transaction));
        }

        repository.Add(transaction);
        await repository.SaveChangesAsync(cancellationToken);
        return new TransactionResult(transaction, command.AccountId, command.InvestmentItemId);
    }

    private static TransactionDetails Validate(CreateTransactionCommand command)
    {
        var errors = new Dictionary<string, string[]>();
        if (command.AccountId == Guid.Empty)
        {
            errors["accountId"] = ["A non-empty investment account identifier is required."];
        }

        if (command.InvestmentItemId == Guid.Empty)
        {
            errors["investmentItemId"] = ["A non-empty investment item identifier is required."];
        }

        if (!Enum.IsDefined(command.Type))
        {
            errors["type"] = ["The transaction type is not supported."];
        }

        if (command.TradeDate == default)
        {
            errors["tradeDate"] = ["A trade date is required."];
        }

        if (command.SettlementDate < command.TradeDate)
        {
            errors["settlementDate"] = ["The settlement date cannot be before the trade date."];
        }

        ValidatePositiveDecimal(command.Quantity, 8, 100_000_000_000_000_000_000m, "quantity", "Quantity", errors);
        ValidatePositiveDecimal(command.UnitPrice, 8, 100_000_000_000m, "unitPrice", "Unit price", errors);
        if (command.Fees < 0)
        {
            errors["fees"] = ["Fees cannot be negative."];
        }
        else if (DecimalPlaces(command.Fees) > 4)
        {
            errors["fees"] = ["Fees cannot exceed four decimal places."];
        }
        else if (command.Fees >= 1_000_000_000_000_000m)
        {
            errors["fees"] = ["Fees exceed the supported value range."];
        }

        var currencyCode = command.CurrencyCode?.Trim().ToUpperInvariant() ?? string.Empty;
        if (currencyCode.Length != 3 || currencyCode.Any(character => !char.IsAsciiLetterUpper(character)))
        {
            errors["currencyCode"] = ["A currency code must contain exactly three ASCII letters."];
        }

        var notes = string.IsNullOrWhiteSpace(command.Notes) ? null : command.Notes.Trim();
        if (notes?.Length > 2000)
        {
            errors["notes"] = ["Notes cannot exceed 2000 characters."];
        }

        if (errors.Count > 0)
        {
            throw new ApplicationValidationException(errors);
        }

        return new TransactionDetails(currencyCode, notes);
    }

    private static void ValidatePositiveDecimal(
        decimal value,
        int maximumDecimalPlaces,
        decimal maximumExclusive,
        string key,
        string label,
        IDictionary<string, string[]> errors)
    {
        if (value <= 0)
        {
            errors[key] = [$"{label} must be greater than zero."];
        }
        else if (DecimalPlaces(value) > maximumDecimalPlaces)
        {
            errors[key] = [$"{label} cannot exceed {maximumDecimalPlaces} decimal places."];
        }
        else if (value >= maximumExclusive)
        {
            errors[key] = [$"{label} exceeds the supported value range."];
        }
    }

    private static int DecimalPlaces(decimal value) =>
        (decimal.GetBits(value)[3] >> 16) & 0x7F;

    private static void EnsureLedgerNeverOversells(IEnumerable<Transaction> transactions)
    {
        decimal balance = 0;
        foreach (var transaction in transactions
                     .OrderBy(item => item.TradeDate)
                     .ThenBy(item => item.CreatedAtUtc)
                     .ThenBy(item => item.Id))
        {
            balance += transaction.Type == TransactionType.Buy
                ? transaction.Quantity
                : -transaction.Quantity;
            if (balance < 0)
            {
                throw Validation(
                    "quantity",
                    "The sell quantity exceeds the units held at that point in the transaction history.");
            }
        }
    }

    private static ApplicationValidationException Validation(string key, string message) =>
        new(new Dictionary<string, string[]> { [key] = [message] });

    private sealed record TransactionDetails(string CurrencyCode, string? Notes);
}
