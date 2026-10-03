using InvestManage.Maui.Services;

namespace InvestManage.Maui;

public partial class MainPage : ContentPage
{
    private readonly IInvestManageApiClient apiClient;

    public MainPage(IInvestManageApiClient apiClient)
    {
        InitializeComponent();
        this.apiClient = apiClient;
    }

    private async void OnCheckApiClicked(object? sender, EventArgs e)
    {
        CheckApiButton.IsEnabled = false;
        ApiActivityIndicator.IsVisible = true;
        ApiActivityIndicator.IsRunning = true;
        ApiStatusLabel.Text = "Connecting…";

        try
        {
            var status = await apiClient.GetSystemStatusAsync();
            ApiStatusLabel.Text = $"Connected to {status.Service} ({status.Environment})";
        }
        catch (Exception exception)
        {
            ApiStatusLabel.Text = $"Connection failed: {exception.Message}";
        }
        finally
        {
            ApiActivityIndicator.IsRunning = false;
            ApiActivityIndicator.IsVisible = false;
            CheckApiButton.IsEnabled = true;
        }
    }
}
