# Buy and sell transaction entry

INVEST-22 adds the first transaction-ledger workflow to the API and WPF client.

## API

Create a transaction for an investment already assigned to an active account:

```http
POST /api/v1/investment-accounts/{accountId}/investments/{investmentItemId}/transactions
Content-Type: application/json
```

The request contains the transaction type (`Buy` or `Sell`), trade date, optional settlement date, quantity, unit price, fees, currency, and optional notes. A successful request returns `201 Created` and the persisted transaction.

Validation failures use standard validation problem details with field-level messages. The API rejects:

- non-positive quantities and unit prices;
- negative fees;
- unsupported currencies or transaction types;
- settlement dates before the trade date;
- values exceeding the supported decimal precision;
- transactions for an archived or unassigned account investment; and
- sells that would make the dated transaction ledger negative.

Overselling validation evaluates transactions by trade date, creation timestamp, and identifier. This also prevents a backdated sell from making a later balance negative.

## WPF workflow

After signing in, open **Transactions**:

1. Select **Load accounts**.
2. Select an active account.
3. Select one of its assigned investments.
4. Enter the buy or sell details.
5. Enter any two of quantity, unit price, and amount, then select **Calculate** or **Save transaction**.
6. Select **Save transaction**.

The form calculates the missing value as follows:

- Quantity and amount calculate unit price using the selected investment's configured price precision.
- Quantity and unit price calculate amount rounded to two decimal places.
- Unit price and amount calculate quantity rounded to eight decimal places.
- When all three values were entered manually, the form asks for confirmation and replaces unit price with amount divided by quantity, rounded to the investment's configured price precision.

Amount is a derived entry aid and is not stored as an independent database value. Persisted quantity and unit price remain the authoritative transaction values, avoiding inconsistent financial records.

The WPF application sends the request through `InvestManage.Client`; it never connects directly to SQL Server.

Transaction history and controlled corrections are intentionally scheduled for INVEST-23.
