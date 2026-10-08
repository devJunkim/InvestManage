using System.Collections.ObjectModel;
using InvestManage.Client.Services;
using InvestManage.Contracts.Investments;

namespace InvestManage.Wpf.ViewModels;

public sealed class InvestmentsViewModel : OperationalViewModel
{
    private readonly IInvestManageApiClient apiClient;
    private InvestmentItemResponse? selectedInvestment;
    private string? searchText;
    private string? filterCurrency;
    private string? filterProvider;
    private string selectedFilterType = "All";
    private bool includeArchived;
    private string code = string.Empty;
    private string name = string.Empty;
    private InvestmentType selectedType = InvestmentType.MutualFund;
    private string currencyCode = "CAD";
    private string? provider;
    private int pricePrecision = 4;
    private string? notes;

    public InvestmentsViewModel(IInvestManageApiClient apiClient)
    {
        this.apiClient = apiClient;
        InvestmentTypes = Enum.GetValues<InvestmentType>();
        FilterTypes = new[] { "All" }.Concat(Enum.GetNames<InvestmentType>()).ToArray();
        PricePrecisions = Enumerable.Range(0, 9).ToArray();
        RefreshCommand = new AsyncCommand(RefreshAsync);
        SaveCommand = new AsyncCommand(SaveAsync);
        ArchiveOrReactivateCommand = new AsyncCommand(ChangeLifecycleAsync);
        NewCommand = new RelayCommand(StartNew);
    }

    public ObservableCollection<InvestmentItemResponse> Investments { get; } = [];

    public IReadOnlyList<InvestmentType> InvestmentTypes { get; }

    public IReadOnlyList<string> FilterTypes { get; }

    public IReadOnlyList<int> PricePrecisions { get; }

    public AsyncCommand RefreshCommand { get; }

    public AsyncCommand SaveCommand { get; }

    public AsyncCommand ArchiveOrReactivateCommand { get; }

    public RelayCommand NewCommand { get; }

    public InvestmentItemResponse? SelectedInvestment
    {
        get => selectedInvestment;
        set
        {
            if (SetProperty(ref selectedInvestment, value))
            {
                LoadEditor(value);
                OnPropertyChanged(nameof(EditorTitle));
                OnPropertyChanged(nameof(LifecycleAction));
                OnPropertyChanged(nameof(CanChangeLifecycle));
            }
        }
    }

    public string EditorTitle => SelectedInvestment is null ? "New investment" : "Investment details";

    public string LifecycleAction => SelectedInvestment?.IsArchived == true ? "Reactivate" : "Archive";

    public bool CanChangeLifecycle => SelectedInvestment is not null;

    public string? SearchText { get => searchText; set => SetProperty(ref searchText, value); }

    public string? FilterCurrency { get => filterCurrency; set => SetProperty(ref filterCurrency, value); }

    public string? FilterProvider { get => filterProvider; set => SetProperty(ref filterProvider, value); }

    public string SelectedFilterType { get => selectedFilterType; set => SetProperty(ref selectedFilterType, value); }

    public bool IncludeArchived { get => includeArchived; set => SetProperty(ref includeArchived, value); }

    public string Code { get => code; set => SetProperty(ref code, value); }

    public string Name { get => name; set => SetProperty(ref name, value); }

    public InvestmentType SelectedType { get => selectedType; set => SetProperty(ref selectedType, value); }

    public string CurrencyCode { get => currencyCode; set => SetProperty(ref currencyCode, value); }

    public string? Provider { get => provider; set => SetProperty(ref provider, value); }

    public int PricePrecision { get => pricePrecision; set => SetProperty(ref pricePrecision, value); }

    public string? Notes { get => notes; set => SetProperty(ref notes, value); }

    public Task RefreshAsync() => RunAsync(async () =>
    {
        InvestmentType? type = SelectedFilterType == "All"
            ? null
            : Enum.Parse<InvestmentType>(SelectedFilterType);
        var result = await apiClient.GetInvestmentItemsAsync(
            new InvestmentItemFilter(SearchText, type, FilterCurrency, FilterProvider, IncludeArchived));
        Investments.Clear();
        foreach (var investment in result)
        {
            Investments.Add(investment);
        }

        StatusMessage = Investments.Count == 0
            ? "No investments match the current filters."
            : $"Loaded {Investments.Count} investment(s).";
    }, "Loading investments…");

    public Task SaveAsync() => RunAsync(async () =>
    {
        InvestmentItemResponse saved;
        if (SelectedInvestment is null)
        {
            saved = await apiClient.CreateInvestmentItemAsync(
                new CreateInvestmentItemRequest(Code, Name, SelectedType, CurrencyCode, Provider, PricePrecision, Notes));
        }
        else
        {
            saved = await apiClient.UpdateInvestmentItemAsync(
                SelectedInvestment.Id,
                new UpdateInvestmentItemRequest(Code, Name, SelectedType, CurrencyCode, Provider, PricePrecision, Notes));
        }

        await RefreshCoreAsync();
        SelectedInvestment = Investments.SingleOrDefault(item => item.Id == saved.Id) ?? saved;
        StatusMessage = "Investment saved.";
    }, "Saving investment…");

    public Task ChangeLifecycleAsync() => RunAsync(async () =>
    {
        if (SelectedInvestment is null)
        {
            throw new InvalidOperationException("Select an investment first.");
        }

        if (SelectedInvestment.IsArchived)
        {
            await apiClient.ReactivateInvestmentItemAsync(SelectedInvestment.Id);
        }
        else
        {
            await apiClient.ArchiveInvestmentItemAsync(SelectedInvestment.Id);
        }

        SelectedInvestment = null;
        await RefreshCoreAsync();
        StatusMessage = "Investment lifecycle updated.";
    }, "Updating investment…");

    public void StartNew()
    {
        SelectedInvestment = null;
        LoadEditor(null);
        StatusMessage = "Enter the new investment details.";
    }

    private async Task RefreshCoreAsync()
    {
        InvestmentType? type = SelectedFilterType == "All" ? null : Enum.Parse<InvestmentType>(SelectedFilterType);
        var result = await apiClient.GetInvestmentItemsAsync(
            new InvestmentItemFilter(SearchText, type, FilterCurrency, FilterProvider, IncludeArchived));
        Investments.Clear();
        foreach (var investment in result)
        {
            Investments.Add(investment);
        }
    }

    private void LoadEditor(InvestmentItemResponse? investment)
    {
        Code = investment?.Code ?? string.Empty;
        Name = investment?.Name ?? string.Empty;
        SelectedType = investment?.Type ?? InvestmentType.MutualFund;
        CurrencyCode = investment?.CurrencyCode ?? "CAD";
        Provider = investment?.Provider;
        PricePrecision = investment?.PricePrecision ?? 4;
        Notes = investment?.Notes;
    }
}
