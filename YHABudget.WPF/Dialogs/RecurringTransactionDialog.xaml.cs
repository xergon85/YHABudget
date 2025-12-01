using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Text.RegularExpressions;
using YHABudget.Core.ViewModels;
using YHABudget.WPF.Converters;

namespace YHABudget.WPF.Dialogs;

public partial class RecurringTransactionDialog : Window
{
    public RecurringTransactionDialog()
    {
        InitializeComponent();

        // Bind Title after resources are loaded
        var titleBinding = new Binding("IsEditMode")
        {
            Converter = (RecurringEditModeTitleConverter)Resources["RecurringEditModeTitleConverter"]
        };
        SetBinding(TitleProperty, titleBinding);

        // Subscribe to DataContext changes to monitor RequestClose
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is RecurringTransactionDialogViewModel oldViewModel)
        {
            oldViewModel.RequestClose -= OnRequestClose;
        }

        if (e.NewValue is RecurringTransactionDialogViewModel newViewModel)
        {
            newViewModel.RequestClose += OnRequestClose;
        }
    }

    private void OnRequestClose(object? sender, EventArgs e)
    {
        if (sender is RecurringTransactionDialogViewModel viewModel)
        {
            DialogResult = viewModel.SaveSuccessful;
        }
        Close();
    }

    private void TextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.TextBox textBox)
        {
            textBox.SelectAll();
        }
    }

    private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        // Allow only digits, comma, and period for decimal numbers
        var regex = new Regex(@"^[0-9,\.]+$");
        e.Handled = !regex.IsMatch(e.Text);
    }
}
