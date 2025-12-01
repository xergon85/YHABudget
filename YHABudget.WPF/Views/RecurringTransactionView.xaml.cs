using System.Windows.Controls;
using System.Windows.Input;
using YHABudget.Core.ViewModels;

namespace YHABudget.WPF.Views;

public partial class RecurringTransactionView : UserControl
{
    public RecurringTransactionView()
    {
        InitializeComponent();
    }

    private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is RecurringTransactionViewModel viewModel && viewModel.EditRecurringTransactionCommand.CanExecute(null))
        {
            viewModel.EditRecurringTransactionCommand.Execute(null);
        }
    }
}
