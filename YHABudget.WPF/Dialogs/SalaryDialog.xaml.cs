using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using YHABudget.Core.ViewModels;

namespace YHABudget.WPF.Dialogs;

public partial class SalaryDialog : Window
{
    public SalaryDialogViewModel ViewModel { get; }

    public SalaryDialog(SalaryDialogViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
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

    private void TextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            textBox.SelectAll();
        }
    }

    private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        // Allow digits, decimal point, and comma
        e.Handled = !IsTextAllowed(e.Text);
    }

    private static bool IsTextAllowed(string text)
    {
        // Allow numbers, comma, and period
        return System.Text.RegularExpressions.Regex.IsMatch(text, @"^[0-9,\.]+$");
    }
}
