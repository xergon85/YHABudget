using System.Windows;
using YHABudget.Core.ViewModels;

namespace YHABudget.WPF.Dialogs;

public partial class AbsenceDialog : Window
{
    public AbsenceDialog(AbsenceDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
