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

    public DialogService(ICategoryService categoryService, ITransactionService transactionService)
    {
        _categoryService = categoryService;
        _transactionService = transactionService;
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
}
