# Investment-item management API

INVEST-20 adds the Phase 2 investment catalog and account-assignment endpoints under `/api/v1`.

An investment item is a global identity for a stock, ETF, mutual fund, segregated fund, or other investment. Assigning the same item to several accounts creates account memberships; it does not duplicate the investment identity or its price history.

## Investment fields

| Field | Rules |
| --- | --- |
| `code` | Required; maximum 50 characters. The entered casing is preserved and a normalized uppercase value is stored for searching. |
| `name` | Required; maximum 200 characters. |
| `type` | `Stock`, `ExchangeTradedFund`, `MutualFund`, `SegregatedFund`, or `Other`. |
| `currencyCode` | Required three-letter currency already present in the currency table; normalized to uppercase. |
| `provider` | Optional; maximum 200 characters. |
| `pricePrecision` | Zero through eight decimal places. |
| `notes` | Optional; maximum 2,000 characters. |
| `isArchived` | Read-only lifecycle state. Archived items are excluded from default queries. |

## Endpoints

| Method | Route | Purpose |
| --- | --- | --- |
| `GET` | `/api/v1/investment-items` | Search and filter investment items. |
| `GET` | `/api/v1/investment-items/{id}` | Retrieve one investment item. |
| `POST` | `/api/v1/investment-items` | Create an investment item. |
| `PUT` | `/api/v1/investment-items/{id}` | Update an active investment item. |
| `POST` | `/api/v1/investment-items/{id}/archive` | Archive an investment item. |
| `POST` | `/api/v1/investment-items/{id}/reactivate` | Reactivate an archived investment item. |
| `GET` | `/api/v1/investment-accounts/{accountId}/investments` | List investments assigned to an account. |
| `POST` | `/api/v1/investment-accounts/{accountId}/investments/{investmentItemId}` | Assign an existing investment item to an account. |

The item-list endpoint accepts optional `search`, `type`, `currencyCode`, `provider`, and `includeArchived` query parameters. The account list accepts `includeArchived`. Both exclude archived investment items by default.

Assignment is idempotent. The first request returns `201 Created`; repeating the same account/item assignment returns `200 OK` with the existing assignment. The database also enforces uniqueness for the account and investment pair.

## Archival behavior

Archiving changes lifecycle state only. The investment item, account assignments, transactions, and price history remain stored. Archived items cannot be edited or newly assigned until reactivated, and they are hidden from default global and account investment lists.

## Errors

Validation failures return `400 Bad Request` as validation problem details. Missing accounts or investment items return `404 Not Found` as problem details.
