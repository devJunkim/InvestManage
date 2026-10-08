using InvestManage.Client.Services;
using InvestManage.Contracts;
using InvestManage.Contracts.Accounts;
using InvestManage.Contracts.Investments;
using InvestManage.Contracts.Users;
using InvestManage.Wpf.ViewModels;

namespace InvestManage.Wpf.Tests;

public sealed class ManagementViewModelTests
{
    private static readonly Guid UserId = Guid.Parse("7fcb8eb1-a0ec-49ad-8d25-72042c1b269f");
    private static readonly Guid AccountId = Guid.Parse("23bcd69b-6806-4981-a4fc-0ee93a27c123");
    private static readonly Guid ItemId = Guid.Parse("1fe4ca47-03e6-4be9-87de-178b66e3e550");

    [Fact]
    public async Task UserAccess_RegistersAndLogsInWithoutExposingGuidInput()
    {
        var api = new FakeApiClient();
        var viewModel = new UserAccessViewModel(api)
        {
            FirstName = "Jun",
            LastName = "Kim",
            Email = "jun@example.test",
            LoginId = "junkim",
            RegistrationPassword = "password123"
        };

        Assert.False(viewModel.IsRegistrationVisible);
        viewModel.ShowRegistrationCommand.Execute(null);
        Assert.True(viewModel.IsRegistrationVisible);
        viewModel.CancelRegistrationCommand.Execute(null);
        Assert.False(viewModel.IsRegistrationVisible);
        Assert.Empty(viewModel.FirstName);
        Assert.Empty(viewModel.RegistrationPassword);
        viewModel.FirstName = "Jun";
        viewModel.LastName = "Kim";
        viewModel.Email = "jun@example.test";
        viewModel.LoginId = "junkim";
        viewModel.RegistrationPassword = "password123";
        viewModel.ShowRegistrationCommand.Execute(null);

        await viewModel.RegisterAsync();

        Assert.True(viewModel.IsAuthenticated);
        Assert.Equal("Jun Kim", viewModel.CurrentUser?.DisplayName);
        Assert.Equal("password123", api.LastRegistration?.Password);
        Assert.Empty(viewModel.RegistrationPassword);
        Assert.False(viewModel.IsRegistrationVisible);
        Assert.Equal("Jun Kim", viewModel.SessionDescription);
    }

    [Fact]
    public async Task Investments_RefreshPassesAllFiltersAndPopulatesGrid()
    {
        var api = new FakeApiClient();
        api.Items.Add(Item());
        var viewModel = new InvestmentsViewModel(api)
        {
            SearchText = "Canadian index",
            SelectedFilterType = nameof(InvestmentType.MutualFund),
            FilterCurrency = "CAD",
            FilterProvider = "TD",
            IncludeArchived = true
        };

        await viewModel.RefreshAsync();

        Assert.Single(viewModel.Investments);
        Assert.Equal(
            new InvestmentItemFilter("Canadian index", InvestmentType.MutualFund, "CAD", "TD", true),
            api.LastFilter);
        Assert.Null(viewModel.ErrorMessage);
    }

    [Fact]
    public async Task Investments_NewFormSavesEveryEditableField()
    {
        var api = new FakeApiClient();
        var viewModel = new InvestmentsViewModel(api);
        viewModel.StartNew();
        viewModel.Code = "ML1436";
        viewModel.Name = "Manulife fund";
        viewModel.SelectedType = InvestmentType.SegregatedFund;
        viewModel.CurrencyCode = "CAD";
        viewModel.Provider = "Manulife";
        viewModel.PricePrecision = 6;
        viewModel.Notes = "Long-term holding";

        await viewModel.SaveAsync();

        Assert.NotNull(api.LastCreatedItem);
        Assert.Equal(InvestmentType.SegregatedFund, api.LastCreatedItem.Type);
        Assert.Equal(6, api.LastCreatedItem.PricePrecision);
        Assert.Equal("Manulife", api.LastCreatedItem.Provider);
        Assert.Equal("Long-term holding", api.LastCreatedItem.Notes);
    }

    [Fact]
    public async Task Accounts_LoadAndAssignReusesCatalogInvestment()
    {
        var api = new FakeApiClient();
        var account = new InvestmentAccountResponse(
            AccountId,
            UserId,
            "Retirement",
            InvestmentAccountType.RegisteredRetirementSavingsPlan,
            "CAD",
            false);
        api.Accounts.Add(account);
        api.Items.Add(Item());
        var viewModel = new AccountsViewModel(api);
        viewModel.SetCurrentUser(User());

        await viewModel.RefreshAsync();
        viewModel.SelectedAccount = account;
        await viewModel.LoadAssignmentsAsync();
        viewModel.SelectedAvailableInvestment = Assert.Single(viewModel.AvailableInvestments);
        await viewModel.AssignAsync();

        Assert.Equal((AccountId, ItemId), api.LastAssignment);
        Assert.Equal(ItemId, Assert.Single(viewModel.AssignedInvestments).Id);
        Assert.Empty(viewModel.AvailableInvestments);
    }

    [Fact]
    public async Task Accounts_NewFormUsesSignedInUserWithoutGuidEntry()
    {
        var api = new FakeApiClient();
        var viewModel = new AccountsViewModel(api);
        viewModel.SetCurrentUser(User());
        viewModel.StartNew();
        viewModel.Name = "RRSP";
        viewModel.SelectedType = InvestmentAccountType.RegisteredRetirementSavingsPlan;
        viewModel.CurrencyCode = "CAD";

        await viewModel.SaveAsync();

        Assert.Equal(UserId, api.LastCreatedAccount?.UserId);
        Assert.Equal("RRSP", api.LastCreatedAccount?.Name);
        Assert.Equal("Account saved.", viewModel.StatusMessage);
    }

    [Fact]
    public async Task Accounts_WithoutLoginProducesActionableError()
    {
        var viewModel = new AccountsViewModel(new FakeApiClient());

        await viewModel.RefreshAsync();

        Assert.Contains("Sign in", viewModel.ErrorMessage);
        Assert.Empty(viewModel.Accounts);
    }

    private static InvestmentItemResponse Item() =>
        new(ItemId, "TDB3046", "TD Canadian Index Fund", InvestmentType.MutualFund, "CAD", "TD", 4, null, false);

    private static UserResponse User() =>
        new(UserId, "Jun", "Kim", "Jun Kim", "jun@example.test", "junkim");

    private sealed class FakeApiClient : IInvestManageApiClient
    {
        public List<InvestmentAccountResponse> Accounts { get; } = [];
        public List<InvestmentItemResponse> Items { get; } = [];
        public List<InvestmentItemResponse> Assigned { get; } = [];
        public InvestmentItemFilter? LastFilter { get; private set; }
        public CreateInvestmentItemRequest? LastCreatedItem { get; private set; }
        public RegisterUserRequest? LastRegistration { get; private set; }
        public CreateInvestmentAccountRequest? LastCreatedAccount { get; private set; }
        public (Guid AccountId, Guid ItemId)? LastAssignment { get; private set; }

        public Task<SystemStatusResponse> GetSystemStatusAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new SystemStatusResponse("InvestManage API", "1.0.0", "Test", DateTimeOffset.UtcNow));

        public Task<UserResponse> RegisterUserAsync(RegisterUserRequest request, CancellationToken cancellationToken = default)
        {
            LastRegistration = request;
            return Task.FromResult(User());
        }

        public Task<UserResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult(User());

        public Task<IReadOnlyList<InvestmentAccountResponse>> GetAccountsAsync(Guid userId, bool includeArchived, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<InvestmentAccountResponse>>(Accounts.Where(item => includeArchived || !item.IsArchived).ToList());

        public Task<InvestmentAccountResponse> CreateAccountAsync(CreateInvestmentAccountRequest request, CancellationToken cancellationToken = default)
        {
            LastCreatedAccount = request;
            var account = new InvestmentAccountResponse(Guid.NewGuid(), request.UserId, request.Name ?? string.Empty, request.Type, request.CurrencyCode ?? string.Empty, false);
            Accounts.Add(account);
            return Task.FromResult(account);
        }

        public Task<InvestmentAccountResponse> UpdateAccountAsync(Guid id, UpdateInvestmentAccountRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult(new InvestmentAccountResponse(id, UserId, request.Name ?? string.Empty, request.Type, request.CurrencyCode ?? string.Empty, false));

        public Task ArchiveAccountAsync(Guid id, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task ReactivateAccountAsync(Guid id, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<IReadOnlyList<InvestmentItemResponse>> GetInvestmentItemsAsync(InvestmentItemFilter filter, CancellationToken cancellationToken = default)
        {
            LastFilter = filter;
            return Task.FromResult<IReadOnlyList<InvestmentItemResponse>>(Items.Where(item => filter.IncludeArchived || !item.IsArchived).ToList());
        }

        public Task<InvestmentItemResponse> CreateInvestmentItemAsync(CreateInvestmentItemRequest request, CancellationToken cancellationToken = default)
        {
            LastCreatedItem = request;
            var item = new InvestmentItemResponse(Guid.NewGuid(), request.Code ?? string.Empty, request.Name ?? string.Empty, request.Type, request.CurrencyCode ?? string.Empty, request.Provider, request.PricePrecision, request.Notes, false);
            Items.Add(item);
            return Task.FromResult(item);
        }

        public Task<InvestmentItemResponse> UpdateInvestmentItemAsync(Guid id, UpdateInvestmentItemRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult(new InvestmentItemResponse(id, request.Code ?? string.Empty, request.Name ?? string.Empty, request.Type, request.CurrencyCode ?? string.Empty, request.Provider, request.PricePrecision, request.Notes, false));

        public Task ArchiveInvestmentItemAsync(Guid id, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task ReactivateInvestmentItemAsync(Guid id, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<IReadOnlyList<InvestmentItemResponse>> GetAccountInvestmentsAsync(Guid accountId, bool includeArchived, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<InvestmentItemResponse>>(Assigned.ToList());

        public Task<InvestmentAssignmentResponse> AssignInvestmentAsync(Guid accountId, Guid investmentItemId, CancellationToken cancellationToken = default)
        {
            LastAssignment = (accountId, investmentItemId);
            var item = Items.Single(value => value.Id == investmentItemId);
            if (Assigned.All(value => value.Id != investmentItemId))
            {
                Assigned.Add(item);
            }

            return Task.FromResult(new InvestmentAssignmentResponse(Guid.NewGuid(), accountId, investmentItemId));
        }
    }
}
