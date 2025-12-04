using Microsoft.EntityFrameworkCore;
using YHABudget.Data.Context;
using YHABudget.Data.Enums;
using YHABudget.Data.Models;
using YHABudget.Data.Queries;

namespace YHABudget.Data.Services;

public class RecurringTransactionService : IRecurringTransactionService
{
    private readonly BudgetDbContext _context;
    private readonly RecurringTransactionQueries _queries;

    public RecurringTransactionService(BudgetDbContext context)
    {
        _context = context;
        _queries = new RecurringTransactionQueries(context);
    }

    public RecurringTransaction AddRecurringTransaction(RecurringTransaction transaction)
    {
        _context.RecurringTransactions.Add(transaction);
        _context.SaveChanges();
        return transaction;
    }

    public IEnumerable<RecurringTransaction> GetAllRecurringTransactions()
    {
        return _queries.GetAllRecurringTransactions();
    }

    public RecurringTransaction? GetRecurringTransactionById(int id)
    {
        return _queries.GetRecurringTransactionById(id);
    }

    public IEnumerable<RecurringTransaction> GetActiveRecurringTransactions()
    {
        return _queries.GetActiveRecurringTransactions();
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
        var monthStart = new DateTime(month.Year, month.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

        var activeRecurring = GetRecurringTransactionsForMonth(monthStart, monthEnd);

        foreach (var recurring in activeRecurring)
        {
            var transactionDate = TryCalculateTransactionDate(recurring, month);
            if (!transactionDate.HasValue)
                continue;

            if (!ShouldCreateTransaction(transactionDate.Value, recurring.StartDate.Date))
                continue;

            if (TransactionAlreadyExists(recurring, monthStart, monthEnd))
                continue;

            var newTransaction = CreateTransactionFromRecurring(recurring, transactionDate.Value);
            _context.Transactions.Add(newTransaction);
            newTransactions.Add(newTransaction);
        }

        _context.SaveChanges();
        return newTransactions;
    }

    private List<RecurringTransaction> GetRecurringTransactionsForMonth(DateTime monthStart, DateTime monthEnd)
    {
        return _context.RecurringTransactions
            .Include(rt => rt.Category)
            .Where(rt => rt.IsActive &&
                         rt.StartDate <= monthEnd &&
                         (rt.EndDate == null || rt.EndDate >= monthStart))
            .ToList();
    }

    private DateTime? TryCalculateTransactionDate(RecurringTransaction recurring, DateTime month)
    {
        if (recurring.RecurrenceType == RecurrenceType.Monthly)
        {
            int day = Math.Min(recurring.StartDate.Day, DateTime.DaysInMonth(month.Year, month.Month));
            return new DateTime(month.Year, month.Month, day);
        }

        if (recurring.RecurrenceType == RecurrenceType.Yearly && recurring.RecurrenceMonth.HasValue)
        {
            if (month.Month == recurring.RecurrenceMonth.Value)
            {
                int day = Math.Min(recurring.StartDate.Day, DateTime.DaysInMonth(month.Year, month.Month));
                return new DateTime(month.Year, month.Month, day);
            }
        }

        return null;
    }

    private bool ShouldCreateTransaction(DateTime transactionDate, DateTime startDate)
    {
        return transactionDate <= DateTime.Today && transactionDate >= startDate;
    }

    private bool TransactionAlreadyExists(RecurringTransaction recurring, DateTime monthStart, DateTime monthEnd)
    {
        return _context.Transactions
            .Any(t => t.Description == recurring.Description &&
                     t.Amount == recurring.Amount &&
                     t.CategoryId == recurring.CategoryId &&
                     t.Type == recurring.Type &&
                     t.IsRecurring == true &&
                     t.Date >= monthStart &&
                     t.Date <= monthEnd);
    }

    private Transaction CreateTransactionFromRecurring(RecurringTransaction recurring, DateTime date)
    {
        return new Transaction
        {
            Description = recurring.Description,
            Amount = recurring.Amount,
            CategoryId = recurring.CategoryId,
            Category = recurring.Category,
            Type = recurring.Type,
            Date = date,
            IsRecurring = true
        };
    }
}
