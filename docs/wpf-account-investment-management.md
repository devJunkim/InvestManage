# WPF account and investment management

INVEST-21 provides the first Windows-native management workflow. The WPF application uses MVVM and communicates exclusively with the ASP.NET Core API through `InvestManage.Client`.

`MainWindow.xaml` is only the application shell and tab host. Each workflow is isolated in its own WPF `UserControl` under `Views` (`UserAccessView`, `AccountsView`, `InvestmentsView`, and `TransactionsView`) and receives its dedicated view model through data binding.

## Start in Visual Studio

Open `InvestManage.sln`, select the **API + WPF** shared launch profile, and press **F5**. The API starts on `https://localhost:7094` and the WPF application opens alongside it.

## Account workflow

The **User access** tab separates user registration from investment-account management:

- Create a user with first name, last name, email, Login ID, and password.
- Sign in with either the email address or Login ID and the password.
- Passwords are processed by the API and stored only as ASP.NET Core Identity-compatible hashes.
- Successful registration signs the new user in automatically.

The **Accounts** tab becomes available after login and supports listing, creating, editing, archiving, and reactivating the signed-in user's investment accounts. The GUID is generated and managed internally; it is never entered in the WPF form. Select an account to review its assigned investments, search the global active-investment catalog, and assign an existing investment identity.

The WPF application never reads or stores the SQL Server connection string. Phase 8 will add bearer tokens and server-enforced authorization to the credential verification introduced here.

## Investment workflow

The Investments tab supports:

- search by code, name, or provider;
- filters for type, currency, provider, and archived state;
- editing code, name, type, currency, provider, price precision, and notes;
- price precision selection from zero through eight decimal places; and
- archive and reactivation without deleting history.

All selectors and editable fields use native WPF controls. API validation problem details are displayed as field-specific messages while preserving the entered form values.

## Testable layers

`InvestManage.Client.Tests` verifies HTTP routes, JSON contracts, user access, filtering, lifecycle requests, assignment, and validation-problem translation. `InvestManage.Wpf.Tests` verifies registration, view-model filtering, editable investment fields, account loading, assignment, and local validation states without opening a desktop window. Application and API integration tests cover registration rules, duplicate identities, password hashing, and login by email or Login ID.
