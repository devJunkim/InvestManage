using InvestManage.Client.Services;
using InvestManage.Contracts.Users;

namespace InvestManage.Wpf.ViewModels;

public sealed class UserAccessViewModel : OperationalViewModel
{
    private readonly IInvestManageApiClient apiClient;
    private string loginIdentifier = string.Empty;
    private string loginPassword = string.Empty;
    private string firstName = string.Empty;
    private string lastName = string.Empty;
    private string email = string.Empty;
    private string loginId = string.Empty;
    private string registrationPassword = string.Empty;
    private UserResponse? currentUser;
    private bool isRegistrationVisible;

    public UserAccessViewModel(IInvestManageApiClient apiClient)
    {
        this.apiClient = apiClient;
        LoginCommand = new AsyncCommand(LoginAsync);
        RegisterCommand = new AsyncCommand(RegisterAsync);
        ShowRegistrationCommand = new RelayCommand(ShowRegistration);
        CancelRegistrationCommand = new RelayCommand(CancelRegistration);
        LogoutCommand = new RelayCommand(Logout);
    }

    public event Action<UserResponse?>? CurrentUserChanged;

    public AsyncCommand LoginCommand { get; }

    public AsyncCommand RegisterCommand { get; }

    public RelayCommand LogoutCommand { get; }

    public RelayCommand ShowRegistrationCommand { get; }

    public RelayCommand CancelRegistrationCommand { get; }

    public string LoginIdentifier { get => loginIdentifier; set => SetProperty(ref loginIdentifier, value); }

    public string LoginPassword { get => loginPassword; set => SetProperty(ref loginPassword, value); }

    public string FirstName { get => firstName; set => SetProperty(ref firstName, value); }

    public string LastName { get => lastName; set => SetProperty(ref lastName, value); }

    public string Email { get => email; set => SetProperty(ref email, value); }

    public string LoginId { get => loginId; set => SetProperty(ref loginId, value); }

    public string RegistrationPassword { get => registrationPassword; set => SetProperty(ref registrationPassword, value); }

    public bool IsRegistrationVisible
    {
        get => isRegistrationVisible;
        private set => SetProperty(ref isRegistrationVisible, value);
    }

    public UserResponse? CurrentUser
    {
        get => currentUser;
        private set
        {
            if (SetProperty(ref currentUser, value))
            {
                OnPropertyChanged(nameof(IsAuthenticated));
                OnPropertyChanged(nameof(SessionDescription));
                CurrentUserChanged?.Invoke(value);
            }
        }
    }

    public bool IsAuthenticated => CurrentUser is not null;

    public string SessionDescription => CurrentUser?.DisplayName ?? string.Empty;

    public Task LoginAsync() => RunAsync(async () =>
    {
        CurrentUser = await apiClient.LoginAsync(new LoginRequest(LoginIdentifier, LoginPassword));
        IsRegistrationVisible = false;
        LoginPassword = string.Empty;
        StatusMessage = "Login successful.";
    }, "Signing in…");

    public Task RegisterAsync() => RunAsync(async () =>
    {
        CurrentUser = await apiClient.RegisterUserAsync(
            new RegisterUserRequest(FirstName, LastName, Email, LoginId, RegistrationPassword));
        IsRegistrationVisible = false;
        RegistrationPassword = string.Empty;
        StatusMessage = "User created and signed in.";
    }, "Creating user…");

    private void ShowRegistration()
    {
        IsRegistrationVisible = true;
        ErrorMessage = null;
        StatusMessage = "Enter the new user details.";
    }

    private void CancelRegistration()
    {
        FirstName = string.Empty;
        LastName = string.Empty;
        Email = string.Empty;
        LoginId = string.Empty;
        RegistrationPassword = string.Empty;
        IsRegistrationVisible = false;
        ErrorMessage = null;
        StatusMessage = "Ready";
    }

    private void Logout()
    {
        CurrentUser = null;
        LoginPassword = string.Empty;
        RegistrationPassword = string.Empty;
        IsRegistrationVisible = false;
        StatusMessage = "Signed out.";
        ErrorMessage = null;
    }
}
