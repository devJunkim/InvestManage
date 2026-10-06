# Investment-account lifecycle API

The version 1 investment-account endpoints manage portfolio containers such as TFSAs, RRSPs, RESPs, RRIFs, and non-registered accounts.

## Endpoints

| Method | Route | Purpose |
| --- | --- | --- |
| `GET` | `/api/v1/investment-accounts?userId={id}&includeArchived=false` | List accounts, excluding archived accounts by default. |
| `GET` | `/api/v1/investment-accounts/{id}?includeArchived=false` | Read one account. An archived account is hidden unless explicitly included. |
| `POST` | `/api/v1/investment-accounts` | Create an account. |
| `PUT` | `/api/v1/investment-accounts/{id}` | Update an active account's name, type, and currency. |
| `POST` | `/api/v1/investment-accounts/{id}/archive` | Archive an account without deleting it or its financial history. |
| `POST` | `/api/v1/investment-accounts/{id}/reactivate` | Return an archived account to active use. |

Archive and reactivate operations are idempotent. Updating an archived account returns a validation response until the account is reactivated.

## Create request

```json
{
  "userId": "7fcb8eb1-a0ec-49ad-8d25-72042c1b269f",
  "name": "Retirement",
  "type": "RegisteredRetirementSavingsPlan",
  "currencyCode": "CAD"
}
```

The referenced user and currency must already exist. Currency codes are normalized to uppercase ISO-style three-letter codes.

## Responses

Successful create and update requests return the account representation, including `isArchived`. Archive and reactivate requests return `204 No Content`.

Validation failures use `ValidationProblemDetails` with field-keyed errors and status `400`. Missing accounts use `ProblemDetails` with status `404`. Unexpected exceptions are returned as status `500` without exposing internal exception details.

## Database migration

Apply the `AddInvestmentAccountArchival` migration using the secure configuration steps in [Database schema and local setup](database-schema.md). The migration adds `IsArchived` and optimizes the account-list index for user and lifecycle filtering.
