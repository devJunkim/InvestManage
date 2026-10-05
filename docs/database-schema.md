# Database schema

The SQL Server schema is defined by `InvestManageDbContext` in the Infrastructure project. The API is the only executable project that references Infrastructure and opens database connections.

## Financial storage

- Prices and unit prices use `decimal(19,8)`.
- Quantities use `decimal(28,8)` to preserve fractional fund units.
- Fees use `decimal(19,4)`.
- Trade, settlement, and price-history dates use SQL Server `date` columns.
- Check constraints reject non-positive prices, quantities, and unit prices, negative fees, and settlement dates before trade dates.

## Indexes

| Table | Columns | Purpose |
| --- | --- | --- |
| `InvestmentItems` | `NormalizedCode` | Search investments by normalized symbol or fund code. |
| `InvestmentAccounts` | `UserId`, `Name` | List and locate a user's accounts. |
| `AccountInvestments` | `InvestmentAccountId`, `InvestmentItemId` (unique) | Prevent duplicate assignment of an investment to an account and list account holdings. |
| `PriceHistory` | `InvestmentItemId`, `PriceDate`, `Type`, `Source` (unique) | Retrieve investment price ranges and reject duplicate prices from the same source and type for a date. |
| `Transactions` | `AccountInvestmentId`, `TradeDate` | Rebuild an account investment's holdings in trade-date order. |

SQL Server also creates supporting indexes for foreign keys that are not already covered by these indexes. All relationships use restrictive deletes so referenced financial history cannot be removed through cascading deletion.

## Migrations

Restore the repository-local EF tool before running migration commands:

```powershell
dotnet tool restore
```

Generate a migration after changing the model:

```powershell
dotnet tool run dotnet-ef migrations add <MigrationName> --project src/InvestManage.Infrastructure/InvestManage.Infrastructure.csproj --context InvestManageDbContext --output-dir Persistence/Migrations
```

Applying migrations with environment-specific, uncommitted connection configuration is covered by INVEST-18.

## Local SQL Server configuration

The API optionally loads `src/InvestManage.Api/appsettings.Local.json`. This file is ignored by Git and is the appropriate place for local SQL Server credentials. Copy `appsettings.Local.example.json` when setting up a new development checkout, then replace the placeholder login and password locally.

The current local template targets:

- Server: `JK-DESKTOP`
- Database: `InvestManage`
- Authentication: SQL Server Authentication

SQL Server must have mixed-mode authentication enabled, and the configured SQL login must have permission to connect to or create the database before applying migrations.
