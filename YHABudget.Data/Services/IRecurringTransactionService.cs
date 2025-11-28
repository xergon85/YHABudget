using YHABudget.Data.Models;

namespace YHABudget.Data.Services;

public interface IRecurringTransactionService
{
    RecurringTransaction AddRecurringTransaction(RecurringTransaction transaction);
    IEnumerable<RecurringTransaction> GetAllRecurringTransactions();
    RecurringTransaction? GetRecurringTransactionById(int id);
    IEnumerable<RecurringTransaction> GetActiveRecurringTransactions();
    void UpdateRecurringTransaction(RecurringTransaction transaction);
    void DeleteRecurringTransaction(int id);
    void ToggleActive(int id);
}
