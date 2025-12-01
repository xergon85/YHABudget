using System.Windows;
using System.Windows.Data;
using YHABudget.Core.ViewModels;
using YHABudget.WPF.Converters;

namespace YHABudget.WPF.Dialogs;

public partial class TransactionDialog : Window
{
    public TransactionDialog()
    {
        InitializeComponent();

        // Bind Title after resources are loaded
        var titleBinding = new Binding("IsEditMode")
        {
            Converter = (EditModeTitleConverter)Resources["EditModeTitleConverter"]
        };
        SetBinding(TitleProperty, titleBinding);

        // Subscribe to DataContext changes to monitor DialogResult
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is TransactionDialogViewModel oldViewModel)
        {
            oldViewModel.RequestClose -= OnRequestClose;
        }

        if (e.NewValue is TransactionDialogViewModel newViewModel)
        {
            newViewModel.RequestClose += OnRequestClose;
        }
    }

    private void OnRequestClose(object? sender, EventArgs e)
    {
        if (sender is TransactionDialogViewModel viewModel)
        {
            DialogResult = viewModel.SaveSuccessful;
        }
        Close();
    }
}
