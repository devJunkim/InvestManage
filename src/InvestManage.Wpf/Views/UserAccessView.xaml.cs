using System.Windows;
using System.Windows.Controls;
using InvestManage.Wpf.ViewModels;

namespace InvestManage.Wpf.Views;

public partial class UserAccessView : UserControl
{
    public UserAccessView()
    {
        InitializeComponent();
    }

    private void LoginPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is UserAccessViewModel viewModel && sender is PasswordBox passwordBox)
        {
            viewModel.LoginPassword = passwordBox.Password;
        }
    }

    private void RegistrationPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is UserAccessViewModel viewModel && sender is PasswordBox passwordBox)
        {
            viewModel.RegistrationPassword = passwordBox.Password;
        }
    }

    private async void SignInClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is UserAccessViewModel viewModel)
        {
            await viewModel.LoginAsync();
            if (viewModel.IsAuthenticated)
            {
                LoginPasswordBox.Clear();
            }
        }
    }

    private async void CreateUserClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is UserAccessViewModel viewModel)
        {
            await viewModel.RegisterAsync();
            if (viewModel.IsAuthenticated)
            {
                RegistrationPasswordBox.Clear();
            }
        }
    }

    private void CancelRegistrationClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is UserAccessViewModel viewModel)
        {
            viewModel.CancelRegistrationCommand.Execute(null);
            RegistrationPasswordBox.Clear();
        }
    }
}
