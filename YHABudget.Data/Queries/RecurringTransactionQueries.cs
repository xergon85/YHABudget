using Microsoft.EntityFrameworkCore;
using YHABudget.Data.Context;
using YHABudget.Data.Models;

namespace YHABudget.Data.Queries;

public class RecurringTransactionQueries
{
    private readonly BudgetDbContext _context;

    public RecurringTransactionQueries(BudgetDbContext context)
    {
        _context = context;
    }

    public List<RecurringTransaction> GetActiveRecurringTransactions()
    {
        return _context.RecurringTransactions
            .Include(rt => rt.Category)
            .Where(rt => rt.IsActive)
            .ToList();
    }

    public List<RecurringTransaction> GetActiveRecurringTransactionsStartingBefore(DateTime date)
    {
        return _context.RecurringTransactions
            .Include(rt => rt.Category)
            .Where(rt => rt.IsActive && rt.StartDate.Date <= date)
            .ToList();
    }

    public List<RecurringTransaction> GetAllRecurringTransactions()
    {
        return _context.RecurringTransactions
            .Include(rt => rt.Category)
            .OrderBy(rt => rt.Description)
            .ToList();
    }

    public RecurringTransaction? GetRecurringTransactionById(int id)
    {
        return _context.RecurringTransactions
            .Include(rt => rt.Category)
            .FirstOrDefault(rt => rt.Id == id);
    }
}
