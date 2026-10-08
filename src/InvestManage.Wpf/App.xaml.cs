using System.Net.Http;
using System.Windows;
using InvestManage.Client.Services;
using InvestManage.Wpf.Configuration;
using InvestManage.Wpf.ViewModels;

namespace InvestManage.Wpf;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var httpClient = new HttpClient
        {
            BaseAddress = ApiEndpoint.BaseAddress,
            Timeout = TimeSpan.FromSeconds(15)
        };
        var apiClient = new InvestManageApiClient(httpClient);
        var viewModel = new MainViewModel(apiClient);
        var window = new MainWindow { DataContext = viewModel };
        MainWindow = window;
        window.Show();
    }
}
