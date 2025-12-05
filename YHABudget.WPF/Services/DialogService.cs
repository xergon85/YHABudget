using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using YHABudget.Core.Services;
using YHABudget.Core.ViewModels;
using YHABudget.Data.Models;
using YHABudget.Data.Services;
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

    public SalarySettings? ShowSalaryDialog(SalarySettings? salary = null)
    {
        var viewModel = _serviceProvider.GetRequiredService<SalaryDialogViewModel>();

        viewModel.LoadSalary(salary);

        var dialog = new SalaryDialog(viewModel)
        {
            Owner = Application.Current.MainWindow
        };

        var result = dialog.ShowDialog();
        return result == true ? viewModel.ToSalarySettings() : null;
    }

    public Absence? ShowAbsenceDialog(Absence? absence, IAbsenceService absenceService)
    {
        var viewModel = new AbsenceDialogViewModel(absenceService);

        viewModel.LoadAbsence(absence);

        var dialog = new AbsenceDialog(viewModel)
        {
            Owner = Application.Current.MainWindow
        };

        var result = dialog.ShowDialog();
        return result == true ? viewModel.ToAbsence() : null;
    }
}
