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
            oldViewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        if (e.NewValue is TransactionDialogViewModel newViewModel)
        {
            newViewModel.PropertyChanged += OnViewModelPropertyChanged;
        }
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TransactionDialogViewModel.DialogResult) && 
            sender is TransactionDialogViewModel viewModel)
        {
            DialogResult = viewModel.DialogResult;
            if (viewModel.DialogResult)
            {
                Close();
            }
        }
    }
}
