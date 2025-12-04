using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using YHABudget.Core.Services;
using YHABudget.Core.ViewModels;
using YHABudget.Data.Models;
using YHABudget.WPF.Dialogs;

namespace YHABudget.WPF.Services;

public class DialogService : IDialogService
{
    private readonly IServiceProvider _serviceProvider;

    public DialogService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public bool? ShowTransactionDialog(Transaction? transaction = null)
    {
        var viewModel = _serviceProvider.GetRequiredService<TransactionDialogViewModel>();

        if (transaction != null)
        {
            viewModel.LoadTransaction(transaction);
        }

        var dialog = new TransactionDialog
        {
            DataContext = viewModel,
            Owner = Application.Current.MainWindow
        };

        return dialog.ShowDialog();
    }

    public bool? ShowRecurringTransactionDialog(RecurringTransaction? recurringTransaction = null)
    {
        var viewModel = _serviceProvider.GetRequiredService<RecurringTransactionDialogViewModel>();

        if (recurringTransaction != null)
        {
            viewModel.LoadRecurringTransaction(recurringTransaction);
        }

        var dialog = new RecurringTransactionDialog
        {
            DataContext = viewModel,
            Owner = Application.Current.MainWindow
        };

        return dialog.ShowDialog();
    }
}
