using System.Windows.Controls;
using System.Windows;
using InvestManage.Wpf.ViewModels;

namespace InvestManage.Wpf.Views;

public partial class TransactionsView : UserControl
{
    public TransactionsView()
    {
        InitializeComponent();
    }

    private void CalculateClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is TransactionsViewModel viewModel && ConfirmRecalculation(viewModel))
        {
            viewModel.Calculate(true);
        }
    }

    private async void SaveClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is TransactionsViewModel viewModel && ConfirmRecalculation(viewModel))
        {
            await viewModel.SaveAsync();
        }
    }

    private static bool ConfirmRecalculation(TransactionsViewModel viewModel)
    {
        if (!viewModel.RequiresCalculationConfirmation)
        {
            return true;
        }

        return MessageBox.Show(
                   "Quantity, unit price, and amount are all entered. " +
                   "The unit price will be recalculated from amount ÷ quantity using the investment's price precision. Continue?",
                   "Confirm unit-price recalculation",
                   MessageBoxButton.YesNo,
                   MessageBoxImage.Question) == MessageBoxResult.Yes;
    }
}
