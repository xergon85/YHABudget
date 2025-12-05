using YHABudget.Data.Models;

namespace YHABudget.Core.Services;

public interface IDialogService
{
    bool? ShowTransactionDialog(Transaction? transaction = null);
    bool? ShowRecurringTransactionDialog(RecurringTransaction? recurringTransaction = null);
    SalarySettings? ShowSalaryDialog(SalarySettings? salary = null);
}
