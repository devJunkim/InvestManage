using System.Windows;

namespace InvestManage.Wpf;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void LoginPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.MainViewModel viewModel && sender is System.Windows.Controls.PasswordBox passwordBox)
        {
            viewModel.UserAccess.LoginPassword = passwordBox.Password;
        }
    }

    private void RegistrationPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.MainViewModel viewModel && sender is System.Windows.Controls.PasswordBox passwordBox)
        {
            viewModel.UserAccess.RegistrationPassword = passwordBox.Password;
        }
    }

    private async void SignInClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.MainViewModel viewModel)
        {
            await viewModel.UserAccess.LoginAsync();
            if (viewModel.UserAccess.IsAuthenticated)
            {
                LoginPasswordBox.Clear();
            }
        }
    }

    private async void CreateUserClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.MainViewModel viewModel)
        {
            await viewModel.UserAccess.RegisterAsync();
            if (viewModel.UserAccess.IsAuthenticated)
            {
                RegistrationPasswordBox.Clear();
            }
        }
    }

    private void CancelRegistrationClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.MainViewModel viewModel)
        {
            viewModel.UserAccess.CancelRegistrationCommand.Execute(null);
            RegistrationPasswordBox.Clear();
        }
    }
}
