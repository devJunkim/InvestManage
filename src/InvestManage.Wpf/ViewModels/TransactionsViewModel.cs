using System.Collections.ObjectModel;
using System.Globalization;
using InvestManage.Client.Services;
using InvestManage.Contracts.Accounts;
using InvestManage.Contracts.Investments;
using InvestManage.Contracts.Transactions;
using InvestManage.Contracts.Users;

namespace InvestManage.Wpf.ViewModels;

public sealed class TransactionsViewModel : OperationalViewModel
{
    private readonly IInvestManageApiClient apiClient;
    private UserResponse? currentUser;
    private InvestmentAccountResponse? selectedAccount;
    private InvestmentItemResponse? selectedInvestment;
    private TransactionType selectedType = TransactionType.Buy;
    private DateTime? tradeDate = DateTime.Today;
    private DateTime? settlementDate;
    private string quantityText = string.Empty;
    private string unitPriceText = string.Empty;
    private string amountText = string.Empty;
    private decimal fees;
    private string currencyCode = "CAD";
    private string? notes;
    private bool calculationIsCurrent;

    public TransactionsViewModel(IInvestManageApiClient apiClient)
    {
        this.apiClient = apiClient;
        TransactionTypes = Enum.GetValues<TransactionType>();
        RefreshAccountsCommand = new AsyncCommand(RefreshAccountsAsync);
        SaveCommand = new AsyncCommand(SaveAsync);
    }

    public ObservableCollection<InvestmentAccountResponse> Accounts { get; } = [];

    public ObservableCollection<InvestmentItemResponse> Investments { get; } = [];

    public IReadOnlyList<TransactionType> TransactionTypes { get; }

    public AsyncCommand RefreshAccountsCommand { get; }

    public AsyncCommand SaveCommand { get; }

    public InvestmentAccountResponse? SelectedAccount
    {
        get => selectedAccount;
        set
        {
            if (SetProperty(ref selectedAccount, value))
            {
                SelectedInvestment = null;
                _ = LoadInvestmentsAsync();
            }
        }
    }

    public InvestmentItemResponse? SelectedInvestment
    {
        get => selectedInvestment;
        set
        {
            if (SetProperty(ref selectedInvestment, value) && value is not null)
            {
                CurrencyCode = value.CurrencyCode;
                calculationIsCurrent = false;
                OnPropertyChanged(nameof(RequiresCalculationConfirmation));
            }
        }
    }

    public TransactionType SelectedType { get => selectedType; set => SetProperty(ref selectedType, value); }

    public DateTime? TradeDate { get => tradeDate; set => SetProperty(ref tradeDate, value); }

    public DateTime? SettlementDate { get => settlementDate; set => SetProperty(ref settlementDate, value); }

    public string QuantityText
    {
        get => quantityText;
        set => SetEntryText(ref quantityText, value, nameof(QuantityText));
    }

    public string UnitPriceText
    {
        get => unitPriceText;
        set => SetEntryText(ref unitPriceText, value, nameof(UnitPriceText));
    }

    public string AmountText
    {
        get => amountText;
        set => SetEntryText(ref amountText, value, nameof(AmountText));
    }

    public decimal Quantity
    {
        get => ParseEntry(QuantityText);
        set => QuantityText = FormatInput(value);
    }

    public decimal UnitPrice
    {
        get => ParseEntry(UnitPriceText);
        set => UnitPriceText = FormatInput(value);
    }

    public decimal Amount
    {
        get => ParseEntry(AmountText);
        set => AmountText = FormatInput(value);
    }

    public decimal Fees { get => fees; set => SetProperty(ref fees, value); }

    public string CurrencyCode { get => currencyCode; set => SetProperty(ref currencyCode, value); }

    public string? Notes { get => notes; set => SetProperty(ref notes, value); }

    public bool RequiresCalculationConfirmation =>
        !calculationIsCurrent && Quantity > 0 && UnitPrice > 0 && Amount > 0;

    public bool Calculate(bool allValuesConfirmed)
    {
        ErrorMessage = null;
        if (SelectedInvestment is null)
        {
            ErrorMessage = "Select an investment before calculating transaction values.";
            StatusMessage = "The calculation failed.";
            return false;
        }

        var quantity = Quantity;
        var unitPrice = UnitPrice;
        var amount = Amount;
        var suppliedValues = (quantity > 0 ? 1 : 0) + (unitPrice > 0 ? 1 : 0) + (amount > 0 ? 1 : 0);
        if (suppliedValues < 2)
        {
            ErrorMessage = "Enter any two positive values for quantity, unit price, and amount.";
            StatusMessage = "The calculation failed.";
            return false;
        }

        if (suppliedValues == 3 && !allValuesConfirmed)
        {
            StatusMessage = "Confirm recalculating the unit price from quantity and amount.";
            return false;
        }

        calculationIsCurrent = true;
        if (quantity > 0 && amount > 0 && suppliedValues == 3)
        {
            var precision = SelectedInvestment.PricePrecision;
            var calculated = Math.Round(amount / quantity, precision, MidpointRounding.AwayFromZero);
            SetCalculatedText(
                ref unitPriceText,
                calculated.ToString($"F{precision}", CultureInfo.CurrentCulture),
                nameof(UnitPriceText));
            StatusMessage = $"Unit price recalculated to {precision} decimal place(s).";
        }
        else if (quantity > 0 && unitPrice > 0)
        {
            SetCalculatedText(
                ref amountText,
                Math.Round(quantity * unitPrice, 2, MidpointRounding.AwayFromZero).ToString("F2", CultureInfo.CurrentCulture),
                nameof(AmountText));
            StatusMessage = "Amount calculated from quantity and unit price.";
        }
        else if (quantity > 0 && amount > 0)
        {
            var precision = SelectedInvestment.PricePrecision;
            var calculated = Math.Round(amount / quantity, precision, MidpointRounding.AwayFromZero);
            SetCalculatedText(
                ref unitPriceText,
                calculated.ToString($"F{precision}", CultureInfo.CurrentCulture),
                nameof(UnitPriceText));
            StatusMessage = $"Unit price calculated to {precision} decimal place(s).";
        }
        else
        {
            var calculated = Math.Round(amount / unitPrice, 8, MidpointRounding.AwayFromZero);
            SetCalculatedText(
                ref quantityText,
                calculated.ToString("F8", CultureInfo.CurrentCulture),
                nameof(QuantityText));
            StatusMessage = "Quantity calculated to eight decimal places.";
        }

        OnPropertyChanged(nameof(RequiresCalculationConfirmation));
        return true;
    }

    public Task RefreshAccountsAsync() => RunAsync(async () =>
    {
        var user = currentUser ?? throw new InvalidOperationException("Sign in before entering transactions.");
        var accounts = await apiClient.GetAccountsAsync(user.Id, false);
        Accounts.Clear();
        foreach (var account in accounts)
        {
            Accounts.Add(account);
        }

        StatusMessage = Accounts.Count == 0
            ? "Create an active account before entering transactions."
            : $"Loaded {Accounts.Count} active account(s).";
    }, "Loading accounts…");

    public Task LoadInvestmentsAsync() => RunAsync(async () =>
    {
        Investments.Clear();
        if (SelectedAccount is null)
        {
            StatusMessage = "Select an account.";
            return;
        }

        var investments = await apiClient.GetAccountInvestmentsAsync(SelectedAccount.Id, false);
        foreach (var investment in investments)
        {
            Investments.Add(investment);
        }

        CurrencyCode = SelectedAccount.CurrencyCode;
        StatusMessage = Investments.Count == 0
            ? "Assign an active investment to this account before entering transactions."
            : $"Loaded {Investments.Count} assigned investment(s).";
    }, "Loading assigned investments…");

    public Task SaveAsync() => RunAsync(async () =>
    {
        var account = SelectedAccount ?? throw new InvalidOperationException("Select an account.");
        var investment = SelectedInvestment ?? throw new InvalidOperationException("Select an investment.");
        var selectedTradeDate = TradeDate ?? throw new InvalidOperationException("Select a trade date.");
        if (!Calculate(true))
        {
            return;
        }

        await apiClient.CreateTransactionAsync(
            account.Id,
            investment.Id,
            new CreateTransactionRequest(
                SelectedType,
                DateOnly.FromDateTime(selectedTradeDate),
                SettlementDate.HasValue ? DateOnly.FromDateTime(SettlementDate.Value) : null,
                Quantity,
                UnitPrice,
                Fees,
                CurrencyCode,
                Notes));

        Quantity = 0;
        UnitPrice = 0;
        Amount = 0;
        Fees = 0;
        Notes = null;
        StatusMessage = $"{SelectedType} transaction saved.";
    }, "Saving transaction…");

    public void SetCurrentUser(UserResponse? user)
    {
        currentUser = user;
        SelectedAccount = null;
        Accounts.Clear();
        Investments.Clear();
        StatusMessage = user is null ? "Sign in to enter transactions." : "Select Load accounts to continue.";
        ErrorMessage = null;
    }

    private void SetEntryText(ref string field, string value, string propertyName)
    {
        if (SetProperty(ref field, value, propertyName))
        {
            calculationIsCurrent = false;
            OnPropertyChanged(nameof(RequiresCalculationConfirmation));
        }
    }

    private void SetCalculatedText(ref string field, string value, string propertyName) =>
        SetProperty(ref field, value, propertyName);

    private static decimal ParseEntry(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0;
        }

        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out var currentCultureValue))
        {
            return currentCultureValue;
        }

        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var invariantValue)
            ? invariantValue
            : 0;
    }

    private static string FormatInput(decimal value) =>
        value == 0 ? string.Empty : value.ToString(CultureInfo.CurrentCulture);
}
