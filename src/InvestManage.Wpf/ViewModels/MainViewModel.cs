using InvestManage.Client.Services;

namespace InvestManage.Wpf.ViewModels;

public sealed class MainViewModel : OperationalViewModel
{
    private readonly IInvestManageApiClient apiClient;
    private string apiStatus = "API connection not checked";

    public MainViewModel(IInvestManageApiClient apiClient)
    {
        this.apiClient = apiClient;
        UserAccess = new UserAccessViewModel(apiClient);
        Accounts = new AccountsViewModel(apiClient);
        Investments = new InvestmentsViewModel(apiClient);
        UserAccess.CurrentUserChanged += user =>
        {
            Accounts.SetCurrentUser(user);
            OnPropertyChanged(nameof(IsAuthenticated));
        };
        CheckApiCommand = new AsyncCommand(CheckApiAsync);
    }

    public AccountsViewModel Accounts { get; }

    public UserAccessViewModel UserAccess { get; }

    public InvestmentsViewModel Investments { get; }

    public bool IsAuthenticated => UserAccess.IsAuthenticated;

    public string ApiStatus
    {
        get => apiStatus;
        private set => SetProperty(ref apiStatus, value);
    }

    public AsyncCommand CheckApiCommand { get; }

    public Task CheckApiAsync() => RunAsync(async () =>
    {
        var status = await apiClient.GetSystemStatusAsync();
        ApiStatus = $"Connected to {status.Service} ({status.Environment})";
        StatusMessage = "API connection succeeded.";
    }, "Checking API connection…");
}
