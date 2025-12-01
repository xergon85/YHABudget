using System.Windows;
using YHABudget.Core.Services;
using YHABudget.Core.ViewModels;
using YHABudget.Data.Models;
using YHABudget.Data.Services;
using YHABudget.WPF.Dialogs;

namespace YHABudget.WPF.Services;

public class DialogService : IDialogService
{
    private readonly ICategoryService _categoryService;
    private readonly ITransactionService _transactionService;
    private readonly IRecurringTransactionService _recurringTransactionService;

    public DialogService(
        ICategoryService categoryService, 
        ITransactionService transactionService,
        IRecurringTransactionService recurringTransactionService)
    {
        _categoryService = categoryService;
        _transactionService = transactionService;
        _recurringTransactionService = recurringTransactionService;
    }

    public bool? ShowTransactionDialog(Transaction? transaction = null)
    {
        var viewModel = new TransactionDialogViewModel(_categoryService, _transactionService);

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
        var viewModel = new RecurringTransactionDialogViewModel(_categoryService, _recurringTransactionService);

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
