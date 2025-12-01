using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using YHABudget.Core.ViewModels;

namespace YHABudget.WPF.Views;

public partial class TransactionView : UserControl
{
    public TransactionView()
    {
        InitializeComponent();
    }

    private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is TransactionViewModel viewModel)
        {
            viewModel.EditTransactionCommand.Execute(null);
        }
    }
}
