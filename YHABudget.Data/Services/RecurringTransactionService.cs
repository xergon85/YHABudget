using Microsoft.EntityFrameworkCore;
using YHABudget.Data.Context;
using YHABudget.Data.Enums;
using YHABudget.Data.Models;

namespace YHABudget.Data.Services;

public class RecurringTransactionService : IRecurringTransactionService
{
    private readonly BudgetDbContext _context;

    public RecurringTransactionService(BudgetDbContext context)
    {
        _context = context;
    }

    public RecurringTransaction AddRecurringTransaction(RecurringTransaction transaction)
    {
        _context.RecurringTransactions.Add(transaction);
        _context.SaveChanges();
        return transaction;
    }

    public IEnumerable<RecurringTransaction> GetAllRecurringTransactions()
    {
        return _context.RecurringTransactions
            .Include(t => t.Category)
            .OrderBy(t => t.Description)
            .ToList();
    }

    public RecurringTransaction? GetRecurringTransactionById(int id)
    {
        return _context.RecurringTransactions
            .Include(t => t.Category)
            .FirstOrDefault(t => t.Id == id);
    }

    public IEnumerable<RecurringTransaction> GetActiveRecurringTransactions()
    {
        return _context.RecurringTransactions
            .Include(t => t.Category)
            .Where(t => t.IsActive)
            .OrderBy(t => t.Description)
            .ToList();
    }

    public void UpdateRecurringTransaction(RecurringTransaction transaction)
    {
        _context.RecurringTransactions.Update(transaction);
        _context.SaveChanges();
    }

    public void DeleteRecurringTransaction(int id)
    {
        var transaction = _context.RecurringTransactions.Find(id);
        if (transaction != null)
        {
            _context.RecurringTransactions.Remove(transaction);
            _context.SaveChanges();
        }
    }

    public void ToggleActive(int id)
    {
        var transaction = _context.RecurringTransactions.Find(id);
        if (transaction != null)
        {
            transaction.IsActive = !transaction.IsActive;
            _context.SaveChanges();
        }
    }

    public IEnumerable<Transaction> ProcessRecurringTransactionsForMonth(DateTime month)
    {
        var newTransactions = new List<Transaction>();

        // Get first day of month
        var monthStart = new DateTime(month.Year, month.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

        // Get all active recurring transactions that should apply to this month
        var activeRecurring = _context.RecurringTransactions
            .Include(rt => rt.Category)
            .Where(rt => rt.IsActive &&
                         rt.StartDate <= monthEnd &&
                         (rt.EndDate == null || rt.EndDate >= monthStart))
            .ToList();

        foreach (var recurring in activeRecurring)
        {
            // Determine if this recurring transaction applies to this month
            bool appliesToMonth = false;
            DateTime transactionDate = monthStart;

            if (recurring.RecurrenceType == RecurrenceType.Monthly)
            {
                // Monthly: applies to every month
                appliesToMonth = true;
                // Use the day from StartDate, or 1st if StartDate day > days in month
                int day = Math.Min(recurring.StartDate.Day, DateTime.DaysInMonth(month.Year, month.Month));
                transactionDate = new DateTime(month.Year, month.Month, day);
            }
            else if (recurring.RecurrenceType == RecurrenceType.Yearly && recurring.RecurrenceMonth.HasValue)
            {
                // Yearly: only applies if this is the specified month
                if (month.Month == recurring.RecurrenceMonth.Value)
                {
                    appliesToMonth = true;
                    int day = Math.Min(recurring.StartDate.Day, DateTime.DaysInMonth(month.Year, month.Month));
                    transactionDate = new DateTime(month.Year, month.Month, day);
                }
            }

            // Only create transaction if the date has already occurred (today or earlier)
            // and if it's not before the StartDate
            if (appliesToMonth &&
                transactionDate <= DateTime.Today &&
                transactionDate >= recurring.StartDate.Date)
            {
                // Check if transaction already exists for this month
                var existingTransaction = _context.Transactions
                    .FirstOrDefault(t =>
                        t.Description == recurring.Description &&
                        t.Amount == recurring.Amount &&
                        t.CategoryId == recurring.CategoryId &&
                        t.Type == recurring.Type &&
                        t.IsRecurring == true &&
                        t.Date >= monthStart &&
                        t.Date <= monthEnd);

                if (existingTransaction == null)
                {
                    // Create new transaction from recurring template
                    var newTransaction = new Transaction
                    {
                        Description = recurring.Description,
                        Amount = recurring.Amount,
                        CategoryId = recurring.CategoryId,
                        Category = recurring.Category,
                        Type = recurring.Type,
                        Date = transactionDate,
                        IsRecurring = true
                    };

                    _context.Transactions.Add(newTransaction);
                    newTransactions.Add(newTransaction);
                }
            }
        }

        _context.SaveChanges();
        return newTransactions;
    }
}
