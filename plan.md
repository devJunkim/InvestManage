# InvestManage Development Plan

This document is the living roadmap and source of truth for InvestManage. Update it as phases are completed or requirements change.

## Product goal

Build an API-first investment tracker that can replace the relevant investing features currently handled in Quicken. The first client is a Windows .NET MAUI application. Angular, React, and Vue clients may be added later against the same API.

The application must support:

- Multiple investment accounts per user.
- Multiple investments within each account.
- Stocks, mutual funds, ETFs, and other investment types.
- Buy and sell transactions, followed later by distributions, fees, and transfers.
- Manual price and NAV entry with at least four decimal places.
- Historical portfolio valuation and gain/loss calculations.
- Smooth, responsive charts for multiple investments and date ranges.
- SQL Server persistence and good query performance.

## Architecture decisions

- **Runtime:** .NET 10.
- **Desktop/mobile client:** .NET MAUI, starting with Windows.
- **Backend:** ASP.NET Core REST API with OpenAPI.
- **Database:** Microsoft SQL Server through Entity Framework Core.
- **Structure:** Domain, Application, Contracts, API, and client projects remain separated.
- **Database access:** Only the API accesses SQL Server. Client applications store only the API address and client-specific preferences.
- **Financial precision:** Use `decimal`, never `float` or `double`, for money, prices, quantities, fees, and calculated financial values.
- **Source of holdings:** Holdings are calculated from transactions rather than maintained as an independently editable quantity.
- **History:** Financial records should normally be archived or corrected with an audit trail instead of being destructively deleted.
- **Price sources:** Begin with manual entry and CSV import. Automatic providers will be optional adapters because many Canadian mutual-fund prices are unavailable through free APIs.
- **Business rules:** Keep valuation and accounting logic in the Domain and Application layers so every UI receives consistent results.
- **Delivery:** Use focused branches and pull requests. CI must build and test affected components before merging.

## Account terminology

Two different concepts must remain distinct:

- **User:** A person who signs in and owns data.
- **Investment account:** A portfolio container such as TFSA, RRSP, RESP, or a non-registered account.

One user can own multiple investment accounts. One investment account can contain multiple investment items.

## Phase 0 — Foundation

**Status:** Complete

Deliverables:

- .NET 10 solution that opens in Visual Studio 2026.
- ASP.NET Core API and Windows .NET MAUI application.
- Domain, Application, and Contracts projects.
- xUnit test projects.
- Shared Visual Studio launch profile for API and MAUI.
- Pull-request CI workflow.

Completion criteria:

- API and MAUI build successfully.
- Automated tests pass.
- MAUI can call the API system-status endpoint.

## Phase 1 — Database and core domain

**Status:** In progress

Suggested branch: `database-domain`

Deliverables:

- Add EF Core SQL Server dependencies and an application `DbContext`.
- Configure the SQL Server connection through user secrets, environment variables, or local uncommitted configuration.
- Model `User`, `InvestmentAccount`, `InvestmentItem`, `AccountInvestment`, `Transaction`, `PriceHistory`, and `Currency` concepts.
- Add entity configurations, constraints, indexes, and an initial migration.
- Add development database creation and migration instructions.
- Add domain and persistence tests.

Important data rules:

- Investment codes are normalized for searching while preserving their display value.
- Price history is unique per investment, date, and applicable price type/source.
- Prices support at least four decimal places; quantities support fractional mutual-fund units.
- Dates representing market or transaction days use date-only semantics where possible.
- Investment/date and account/date access paths receive appropriate indexes.
- Referenced financial history cannot be accidentally removed.

Completion criteria:

- A migration creates a clean SQL Server database.
- Relationships and uniqueness constraints are verified by tests.
- No database credentials are committed.

## Phase 2 — Account and investment management

**Status:** Planned

Suggested branch: `account-investments`

Deliverables:

- API endpoints for creating, reading, updating, archiving, and reactivating investment accounts.
- API endpoints for investment items.
- Assign investments to one or more accounts.
- MAUI account and investment list/detail screens.
- Search and filtering.

Initial investment fields:

- Symbol or fund code, such as `TDB3046`.
- Name, investment type, currency, provider, price precision, notes, and active status.

Completion criteria:

- An account can contain multiple investment items.
- Archived records disappear from default lists without losing history.
- Validation failures are consistent between the API and MAUI client.

## Phase 3 — Transactions and holdings

**Status:** Planned

Suggested branch: `transactions`

Deliverables:

- Buy and sell transaction entry.
- Transaction history, editing, and controlled correction.
- Fields for trade date, settlement date, quantity, unit price, fees, currency, and notes.
- Calculation of units held on any requested date.
- Tests for fractional units, partial sales, invalid overselling, and transaction ordering.
- Extend transaction types later with reinvested distribution, cash distribution/dividend, fee, and transfer.

Completion criteria:

- Holdings are reproducible entirely from transaction history.
- Buy and sell examples match independently calculated expected values.
- Invalid transactions do not corrupt account history.

## Phase 4 — Manual prices and CSV import

**Status:** Planned

Suggested branch: `price-history`

Deliverables:

- Enter, edit, and review one historical price or NAV.
- Enter prices for multiple investments for the same date.
- Copy the previous price as an entry starting point.
- CSV import with preview, validation, and duplicate handling.
- Provider abstraction for manual, CSV, and future market API sources.
- Price display and storage with the precision required by each investment.

Completion criteria:

- Daily prices can be entered efficiently for funds such as TDB3046, PCS112, and ML1436.
- Duplicate or invalid records produce clear validation messages.
- Imported data is transactional: a failed import does not leave partial data unless explicitly accepted.

## Phase 5 — Portfolio calculation engine

**Status:** Planned

Suggested branch: `portfolio-calculations`

Deliverables:

- Calculate units, market value, contributions, book cost, realized gain/loss, unrealized gain/loss, total gain/loss, and return percentage.
- Produce daily account and portfolio valuation series.
- Define and test Canadian adjusted cost base behavior before relying on book-cost results.
- Handle missing price dates explicitly, using a documented carry-forward policy where appropriate.
- Add caching only after measuring real query and calculation performance.

Completion criteria:

- Results match a set of manually verified financial scenarios.
- The same calculation services support MAUI and future web clients.
- Missing or stale prices are visible to the user rather than silently hidden.

## Phase 6 — MAUI MVP

**Status:** Planned

Suggested branch: `maui-mvp`

Deliverables:

- Dashboard.
- Account and investment navigation.
- Transaction entry and history.
- Manual price entry and history.
- Current holdings and gain/loss summary.
- API connection and application settings.
- Loading, empty, offline, validation, and error states.

Completion criteria:

- The complete daily investment-tracking workflow can be performed from the Windows MAUI application.
- Common operations are covered by view-model or service-level unit tests.

## Phase 7 — Charts and performance

**Status:** Planned

Suggested branch: `portfolio-charts`

Deliverables:

- Select one or multiple investments, an account, or the entire portfolio.
- Custom date range and common presets such as one month, year-to-date, one year, and all.
- Absolute-value and percentage-growth views.
- Optional normalized comparison that begins each series at 100.
- Smooth line rendering with exact unsmoothed values in tooltips.
- API aggregation, range filtering, measured query tuning, and downsampling for very large series.

Completion criteria:

- Several years of daily prices remain responsive.
- Visual smoothing never alters stored or reported financial values.
- Chart API queries use verified indexes and bounded result sizes.

## Phase 8 — Authentication and authorization

**Status:** Planned

Suggested branch: `authentication`

Deliverables:

- ASP.NET Core Identity or an equivalent well-supported identity implementation.
- Secure login and password storage.
- Token-based API authentication suitable for native and web clients.
- Ownership and authorization checks for every account-scoped resource.
- Secrets kept outside source control.

Completion criteria:

- A user cannot read or modify another user's information.
- Authentication and authorization behavior has integration-test coverage.
- Development convenience does not bypass production authorization accidentally.

## Phase 9 — Reporting and Quicken-like capabilities

**Status:** Planned

Suggested branch: `reporting`

Potential deliverables, prioritized by actual use:

- Net-worth dashboard.
- Performance by account and investment.
- Contribution history.
- Income and distribution reporting.
- Asset allocation.
- Transaction and valuation reports.
- CSV and Excel export.
- Backup and restore guidance.

Completion criteria:

- Reports reconcile with the underlying transactions, prices, and portfolio calculations.
- Exported data can be independently reviewed without proprietary tooling.

## Phase 10 — Web clients

**Status:** Planned for later

Build one web client to completion before starting the others. Recommended order:

1. Angular for the first structured web client.
2. React.
3. Vue.

Deliverables:

- Publish a stable OpenAPI contract.
- Generate or share typed API clients instead of duplicating request models manually.
- Keep all financial business rules in the API/domain layers.
- Bring each client to essential feature parity with the MAUI workflow before starting the next.

Completion criteria:

- A web client can perform the same essential account, investment, transaction, price, and portfolio operations as MAUI.
- UI implementations do not calculate authoritative financial results independently.

## Phase 11 — Deployment and operations

**Status:** Planned for later

Deliverables:

- Containerize the API and each web client as needed.
- Run SQL Server separately with persistent storage and backups.
- Install MAUI directly on Windows; do not place the MAUI GUI in a container.
- Configure connection strings and API addresses through environment-specific configuration.
- Add health checks, structured logging, database migration procedures, and recovery documentation.

Completion criteria:

- A clean environment can be deployed predictably from documented steps.
- Backup restoration is tested, not merely documented.

## Cross-cutting quality requirements

Apply these requirements throughout all phases:

- Add unit tests for domain and calculation rules.
- Add API integration tests for persistence, validation, and authorization.
- Keep API contracts backward-compatible when practical.
- Validate all external and imported data.
- Use cancellation tokens for database and network operations.
- Avoid premature caching; measure first and add indexes before introducing complexity.
- Provide accessible UI labels, keyboard navigation, and meaningful error messages.
- Log operational failures without exposing credentials or personal financial data.
- Keep source files, migrations, tests, and documentation in the same pull request as their feature.

## Delivery workflow

For each phase:

1. Confirm scope and unresolved business decisions.
2. Create a focused branch.
3. Implement the smallest complete vertical slice.
4. Add or update automated tests.
5. Run local builds and tests.
6. Update this plan and relevant documentation.
7. Push the branch and open a pull request.
8. Merge only after CI passes and the change is reviewed.

## Next action

Continue Phase 1 with INVEST-17: add EF Core SQL Server persistence and the initial migration. Before creating the migration, confirm the local SQL Server instance name, authentication method, preferred database name, and whether the first release is single-user or should include user identity immediately.
