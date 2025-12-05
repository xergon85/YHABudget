using YHABudget.Data.Models;
using YHABudget.Data.Services;

namespace YHABudget.Core.Services;

public interface IDialogService
{
    bool? ShowTransactionDialog(Transaction? transaction = null);
    bool? ShowRecurringTransactionDialog(RecurringTransaction? recurringTransaction = null);
    SalarySettings? ShowSalaryDialog(SalarySettings? salary = null);
    Absence? ShowAbsenceDialog(Absence? absence, IAbsenceService absenceService);
}
