# InvestManage

InvestManage is an API-first investment tracking application. Its active clients are an ASP.NET Core API and a Windows WPF desktop application.

## Open in Visual Studio 2026

1. Open `InvestManage.sln`.
2. Select **API + WPF** in the launch-profile list.
3. Press **F5**.

The API listens on `https://localhost:7094` during local development. The WPF header includes a button that calls `GET /api/v1/system/status` and displays the result. Start on **User access** to register, or sign in with an email/Login ID and password; investment-account screens then use the signed-in user automatically.

## Solution layout

- `src/InvestManage.Api` — ASP.NET Core REST API and OpenAPI document.
- `src/InvestManage.Wpf` — active Windows desktop UI using MVVM.
- `src/InvestManage.Client` — testable .NET API client shared by the WPF UI and client-service tests.
- `src/InvestManage.Maui` — inactive initial scaffold retained for reference.
- `src/InvestManage.Contracts` — request and response contracts shared by .NET clients.
- `src/InvestManage.Application` — application use cases and interfaces.
- `src/InvestManage.Domain` — investment domain model and business rules.
- `src/InvestManage.Infrastructure` — EF Core SQL Server persistence and migrations.
- `tests` — xUnit test projects.

The WPF client knows only the API address. Database connection information belongs exclusively to the API. See [Database schema and local setup](docs/database-schema.md) for secure configuration and migration instructions.

Phase 2 includes the versioned [investment-account lifecycle API](docs/investment-accounts-api.md), [investment-item management API](docs/investment-items-api.md), and [WPF account and investment workflow](docs/wpf-account-investment-management.md). Both resource types use soft archival so financial history is retained.

Phase 3 begins with [buy and sell transaction entry](docs/transaction-entry.md), including fractional units, field-level validation, and dated-ledger overselling protection in both the API and WPF client.

