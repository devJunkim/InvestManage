# InvestManage

InvestManage is an API-first investment tracking application. The initial clients are an ASP.NET Core API and a .NET MAUI application for Windows, with Android and iOS targets available for later development.

## Open in Visual Studio 2026

1. Open `InvestManage.sln`.
2. Select **API + MAUI (Windows)** in the launch-profile list.
3. Select **Windows Machine** for the MAUI target if Visual Studio asks for a target.
4. Press **F5**.

The API listens on `https://localhost:7094` during local development. The MAUI home page includes a button that calls `GET /api/v1/system/status` and displays the result.

## Solution layout

- `src/InvestManage.Api` — ASP.NET Core REST API and OpenAPI document.
- `src/InvestManage.Maui` — native MAUI client for Windows, Android, and iOS.
- `src/InvestManage.Contracts` — request and response contracts shared by .NET clients.
- `src/InvestManage.Application` — application use cases and interfaces.
- `src/InvestManage.Domain` — investment domain model and business rules.
- `tests` — xUnit test projects.

The MAUI client knows only the API address. Database connection information will belong exclusively to the API when persistence is added.

