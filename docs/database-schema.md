# Database schema

The SQL Server schema is defined by `InvestManageDbContext` in the Infrastructure project. The API is the only executable project that references Infrastructure and opens database connections.

## Entity relationship diagram

The [InvestManage ERD](../output/pdf/InvestManage-ERD.pdf) shows the tables, keys, relationships, SQL data types, indexes, and principal integrity rules on one A3 landscape page.

## Financial storage

- Prices and unit prices use `decimal(19,8)`.
- Quantities use `decimal(28,8)` to preserve fractional fund units.
- Fees use `decimal(19,4)`.
- Trade, settlement, and price-history dates use SQL Server `date` columns.
- Check constraints reject non-positive prices, quantities, and unit prices, negative fees, settlement dates before trade dates, and investment price precision outside zero to eight decimal places.

## Indexes

| Table | Columns | Purpose |
| --- | --- | --- |
| `Users` | `NormalizedEmail` (unique) | Support case-insensitive email login and prevent duplicate registration. |
| `Users` | `NormalizedLoginId` (unique) | Support case-insensitive Login ID login and prevent duplicate registration. |
| `InvestmentItems` | `IsArchived`, `NormalizedCode` | List active or archived investments and search by normalized symbol or fund code. |
| `InvestmentItems` | `IsArchived`, `Type`, `CurrencyCode` | Filter active or archived investments by type and currency. |
| `InvestmentAccounts` | `UserId`, `IsArchived`, `Name` | List a user's active or archived accounts efficiently. |
| `AccountInvestments` | `InvestmentAccountId`, `InvestmentItemId` (unique) | Prevent duplicate assignment of an investment to an account and list account holdings. |
| `PriceHistory` | `InvestmentItemId`, `PriceDate`, `Type`, `Source` (unique) | Retrieve investment price ranges and reject duplicate prices from the same source and type for a date. |
| `Transactions` | `AccountInvestmentId`, `TradeDate` | Rebuild an account investment's holdings in trade-date order. |

SQL Server also creates supporting indexes for foreign keys that are not already covered by these indexes. All relationships use restrictive deletes so referenced financial history cannot be removed through cascading deletion.

User passwords are never stored directly. `Users.PasswordHash` contains the versioned hash produced by ASP.NET Core's password hasher. Existing users that predate credential support receive legacy placeholder identities and must register a new credentialed user.

## Migrations

Restore the repository-local EF tool before running migration commands:

```powershell
dotnet tool restore
```

Generate a migration after changing the model:

```powershell
dotnet tool run dotnet-ef migrations add <MigrationName> --project src/InvestManage.Infrastructure/InvestManage.Infrastructure.csproj --context InvestManageDbContext --output-dir Persistence/Migrations
```

## Local SQL Server configuration

The API supports ignored local settings, .NET user secrets, and environment variables. Configuration precedence is command line, environment variables, user secrets, ignored local settings, and then the normal ASP.NET Core settings files.

### Ignored local settings

The API loads `src/InvestManage.Api/appsettings.Local.json` only in Development. This file is ignored by Git and is the simplest place for local SQL Server credentials.

```powershell
Copy-Item src/InvestManage.Api/appsettings.Local.example.json src/InvestManage.Api/appsettings.Local.json
```

Replace the placeholder login and password in the copied file. Never remove `appsettings.Local.json` from `.gitignore`.

The current local template targets:

- Server: `JK-DESKTOP`
- Database: `InvestManage`
- Authentication: SQL Server Authentication

SQL Server must have mixed-mode authentication enabled, and the configured SQL login must have permission to connect to or create the database before applying migrations.

### User secrets

Visual Studio and `dotnet run` also load .NET user secrets in Development:

```powershell
dotnet user-secrets set "ConnectionStrings:InvestManage" "Server=JK-DESKTOP;Database=InvestManage;User ID=<login>;Password=<password>;Integrated Security=False;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True" --project src/InvestManage.Api/InvestManage.Api.csproj
```

### Environment variables

Use the standard ASP.NET Core double-underscore format for deployments and temporary shell configuration:

```powershell
$env:ConnectionStrings__InvestManage = "Server=<server>;Database=InvestManage;User ID=<login>;Password=<password>;Encrypt=True;TrustServerCertificate=False"
```

Production startup fails when the connection string is absent. Production credentials must come from the deployment environment or its secret manager, never from committed JSON.

## Create or update the database

From the repository root, restore the local EF tool and load the ignored local connection into the current process:

```powershell
dotnet tool restore
$settings = Get-Content src/InvestManage.Api/appsettings.Local.json -Raw | ConvertFrom-Json
$env:ConnectionStrings__InvestManage = $settings.ConnectionStrings.InvestManage
```

Apply all pending migrations:

```powershell
dotnet tool run dotnet-ef database update --project src/InvestManage.Infrastructure/InvestManage.Infrastructure.csproj --context InvestManageDbContext
```

Remove the temporary environment variable afterward:

```powershell
Remove-Item Env:ConnectionStrings__InvestManage
```

The command creates the configured database when the SQL login has permission and records applied migrations in `__EFMigrationsHistory`. It is safe to run again; only pending migrations are applied.
