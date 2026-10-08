using System.Collections.ObjectModel;
using InvestManage.Client.Services;
using InvestManage.Contracts.Accounts;
using InvestManage.Contracts.Investments;
using InvestManage.Contracts.Users;

namespace InvestManage.Wpf.ViewModels;

public sealed class AccountsViewModel : OperationalViewModel
{
    private readonly IInvestManageApiClient apiClient;
    private InvestmentAccountResponse? selectedAccount;
    private InvestmentItemResponse? selectedAvailableInvestment;
    private UserResponse? currentUser;
    private bool includeArchived;
    private string name = string.Empty;
    private InvestmentAccountType selectedType = InvestmentAccountType.Other;
    private string currencyCode = "CAD";
    private string? assignmentSearch;

    public AccountsViewModel(IInvestManageApiClient apiClient)
    {
        this.apiClient = apiClient;
        AccountTypes = Enum.GetValues<InvestmentAccountType>();
        RefreshCommand = new AsyncCommand(RefreshAsync);
        SaveCommand = new AsyncCommand(SaveAsync);
        ArchiveOrReactivateCommand = new AsyncCommand(ChangeLifecycleAsync);
        NewCommand = new RelayCommand(StartNew);
        RefreshAssignmentsCommand = new AsyncCommand(LoadAssignmentsAsync);
        AssignCommand = new AsyncCommand(AssignAsync);
    }

    public ObservableCollection<InvestmentAccountResponse> Accounts { get; } = [];

    public ObservableCollection<InvestmentItemResponse> AssignedInvestments { get; } = [];

    public ObservableCollection<InvestmentItemResponse> AvailableInvestments { get; } = [];

    public IReadOnlyList<InvestmentAccountType> AccountTypes { get; }

    public AsyncCommand RefreshCommand { get; }

    public AsyncCommand SaveCommand { get; }

    public AsyncCommand ArchiveOrReactivateCommand { get; }

    public RelayCommand NewCommand { get; }

    public AsyncCommand RefreshAssignmentsCommand { get; }

    public AsyncCommand AssignCommand { get; }

    public string OwnerDescription => CurrentUser is null
        ? "Sign in before managing investment accounts."
        : $"Owner: {CurrentUser.DisplayName} ({CurrentUser.Email})";

    public UserResponse? CurrentUser
    {
        get => currentUser;
        private set
        {
            if (SetProperty(ref currentUser, value))
            {
                OnPropertyChanged(nameof(OwnerDescription));
            }
        }
    }

    public bool IncludeArchived { get => includeArchived; set => SetProperty(ref includeArchived, value); }

    public InvestmentAccountResponse? SelectedAccount
    {
        get => selectedAccount;
        set
        {
            if (SetProperty(ref selectedAccount, value))
            {
                LoadEditor(value);
                OnPropertyChanged(nameof(EditorTitle));
                OnPropertyChanged(nameof(LifecycleAction));
                OnPropertyChanged(nameof(CanChangeLifecycle));
                _ = LoadAssignmentsAsync();
            }
        }
    }

    public InvestmentItemResponse? SelectedAvailableInvestment
    {
        get => selectedAvailableInvestment;
        set => SetProperty(ref selectedAvailableInvestment, value);
    }

    public string EditorTitle => SelectedAccount is null ? "New account" : "Account details";

    public string LifecycleAction => SelectedAccount?.IsArchived == true ? "Reactivate" : "Archive";

    public bool CanChangeLifecycle => SelectedAccount is not null;

    public string Name { get => name; set => SetProperty(ref name, value); }

    public InvestmentAccountType SelectedType { get => selectedType; set => SetProperty(ref selectedType, value); }

    public string CurrencyCode { get => currencyCode; set => SetProperty(ref currencyCode, value); }

    public string? AssignmentSearch { get => assignmentSearch; set => SetProperty(ref assignmentSearch, value); }

    public Task RefreshAsync() => RunAsync(async () =>
    {
        var result = await apiClient.GetAccountsAsync(RequireCurrentUser().Id, IncludeArchived);
        Accounts.Clear();
        foreach (var account in result)
        {
            Accounts.Add(account);
        }

        StatusMessage = Accounts.Count == 0
            ? "No accounts were found for this user."
            : $"Loaded {Accounts.Count} account(s).";
    }, "Loading accounts…");

    public Task SaveAsync() => RunAsync(async () =>
    {
        InvestmentAccountResponse saved;
        if (SelectedAccount is null)
        {
            saved = await apiClient.CreateAccountAsync(
                new CreateInvestmentAccountRequest(RequireCurrentUser().Id, Name, SelectedType, CurrencyCode));
        }
        else
        {
            saved = await apiClient.UpdateAccountAsync(
                SelectedAccount.Id,
                new UpdateInvestmentAccountRequest(Name, SelectedType, CurrencyCode));
        }

        await RefreshCoreAsync();
        SelectedAccount = Accounts.SingleOrDefault(item => item.Id == saved.Id) ?? saved;
        StatusMessage = "Account saved.";
    }, "Saving account…");

    public Task ChangeLifecycleAsync() => RunAsync(async () =>
    {
        if (SelectedAccount is null)
        {
            throw new InvalidOperationException("Select an account first.");
        }

        if (SelectedAccount.IsArchived)
        {
            await apiClient.ReactivateAccountAsync(SelectedAccount.Id);
        }
        else
        {
            await apiClient.ArchiveAccountAsync(SelectedAccount.Id);
        }

        SelectedAccount = null;
        await RefreshCoreAsync();
        StatusMessage = "Account lifecycle updated.";
    }, "Updating account…");

    public Task LoadAssignmentsAsync() => RunAsync(async () =>
    {
        AssignedInvestments.Clear();
        AvailableInvestments.Clear();
        if (SelectedAccount is null)
        {
            StatusMessage = "Select an account to manage its investments.";
            return;
        }

        var assigned = await apiClient.GetAccountInvestmentsAsync(SelectedAccount.Id, IncludeArchived);
        foreach (var investment in assigned)
        {
            AssignedInvestments.Add(investment);
        }

        var catalog = await apiClient.GetInvestmentItemsAsync(new InvestmentItemFilter(AssignmentSearch));
        var assignedIds = assigned.Select(item => item.Id).ToHashSet();
        foreach (var investment in catalog.Where(item => !assignedIds.Contains(item.Id)))
        {
            AvailableInvestments.Add(investment);
        }

        StatusMessage = $"{AssignedInvestments.Count} assigned; {AvailableInvestments.Count} available.";
    }, "Loading account investments…");

    public Task AssignAsync() => RunAsync(async () =>
    {
        if (SelectedAccount is null || SelectedAvailableInvestment is null)
        {
            throw new InvalidOperationException("Select an account and an available investment first.");
        }

        await apiClient.AssignInvestmentAsync(SelectedAccount.Id, SelectedAvailableInvestment.Id);
        await LoadAssignmentsCoreAsync();
        StatusMessage = "Investment assigned to the account.";
    }, "Assigning investment…");

    public void StartNew()
    {
        SelectedAccount = null;
        LoadEditor(null);
        AssignedInvestments.Clear();
        AvailableInvestments.Clear();
        StatusMessage = "Enter the new account details.";
    }

    private async Task RefreshCoreAsync()
    {
        var result = await apiClient.GetAccountsAsync(RequireCurrentUser().Id, IncludeArchived);
        Accounts.Clear();
        foreach (var account in result)
        {
            Accounts.Add(account);
        }
    }

    private async Task LoadAssignmentsCoreAsync()
    {
        if (SelectedAccount is null)
        {
            return;
        }

        var assigned = await apiClient.GetAccountInvestmentsAsync(SelectedAccount.Id, IncludeArchived);
        AssignedInvestments.Clear();
        foreach (var investment in assigned)
        {
            AssignedInvestments.Add(investment);
        }

        var catalog = await apiClient.GetInvestmentItemsAsync(new InvestmentItemFilter(AssignmentSearch));
        var assignedIds = assigned.Select(item => item.Id).ToHashSet();
        AvailableInvestments.Clear();
        foreach (var investment in catalog.Where(item => !assignedIds.Contains(item.Id)))
        {
            AvailableInvestments.Add(investment);
        }

        SelectedAvailableInvestment = null;
    }

    public void SetCurrentUser(UserResponse? user)
    {
        CurrentUser = user;
        SelectedAccount = null;
        Accounts.Clear();
        AssignedInvestments.Clear();
        AvailableInvestments.Clear();
        StatusMessage = user is null ? "Sign in to manage accounts." : "Select Load accounts to continue.";
        ErrorMessage = null;
    }

    private UserResponse RequireCurrentUser() =>
        CurrentUser ?? throw new InvalidOperationException("Sign in before managing investment accounts.");

    private void LoadEditor(InvestmentAccountResponse? account)
    {
        Name = account?.Name ?? string.Empty;
        SelectedType = account?.Type ?? InvestmentAccountType.Other;
        CurrencyCode = account?.CurrencyCode ?? "CAD";
    }
}
