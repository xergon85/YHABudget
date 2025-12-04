using Microsoft.EntityFrameworkCore;
using YHABudget.Data.Context;
using YHABudget.Data.Enums;
using YHABudget.Data.Models;

namespace YHABudget.Data.Queries;

public class TransactionQueries
{
    private readonly BudgetDbContext _context;

    public TransactionQueries(BudgetDbContext context)
    {
        _context = context;
    }

    public List<Transaction> GetTransactionsForDateRange(DateTime startDate, DateTime endDate)
    {
        return _context.Transactions
            .Where(t => t.Date >= startDate && t.Date <= endDate)
            .ToList();
    }

    public List<Transaction> GetTransactionsUpToDate(DateTime date)
    {
        return _context.Transactions
            .Where(t => t.Date <= date)
            .ToList();
    }

    public List<Transaction> GetTransactionsForMonth(DateTime month)
    {
        var startOfMonth = new DateTime(month.Year, month.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
        
        return _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.Date >= startOfMonth && t.Date <= endOfMonth)
            .ToList();
    }

    public List<Transaction> GetFutureTransactionsForMonth(DateTime monthStart, DateTime monthEnd)
    {
        return _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.Date > DateTime.Today && t.Date >= monthStart && t.Date <= monthEnd)
            .ToList();
    }

    public List<DateTime> GetDistinctTransactionMonths()
    {
        var dates = _context.Transactions
            .Select(t => t.Date)
            .ToList();
        
        return dates
            .Select(date => new DateTime(date.Year, date.Month, 1, 0, 0, 0, DateTimeKind.Unspecified))
            .Distinct()
            .OrderByDescending(date => date)
            .ToList();
    }

    public List<Transaction> GetAllTransactions()
    {
        return _context.Transactions
            .Include(t => t.Category)
            .OrderByDescending(t => t.Date)
            .ToList();
    }

    public Transaction? GetTransactionById(int id)
    {
        return _context.Transactions.Find(id);
    }

    public List<Transaction> GetTransactionsForDateRangeWithCategory(DateTime startDate, DateTime endDate)
    {
        return _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.Date >= startDate && t.Date <= endDate)
            .OrderByDescending(t => t.Date)
            .ToList();
    }

    public List<Transaction> GetTransactionsByCategory(int? categoryId)
    {
        return _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.CategoryId == categoryId)
            .OrderByDescending(t => t.Date)
            .ToList();
    }

    public List<Transaction> GetTransactionsByFilter(TransactionType? type, int? categoryId, DateTime? month)
    {
        var query = _context.Transactions
            .Include(t => t.Category)
            .AsQueryable();
        
        query = ApplyTypeFilter(query, type);
        query = ApplyCategoryFilter(query, categoryId);
        query = ApplyMonthFilter(query, month);
        
        return query.OrderByDescending(t => t.Date).ToList();
    }

    private IQueryable<Transaction> ApplyTypeFilter(IQueryable<Transaction> query, TransactionType? type)
    {
        return type.HasValue ? query.Where(t => t.Type == type.Value) : query;
    }

    private IQueryable<Transaction> ApplyCategoryFilter(IQueryable<Transaction> query, int? categoryId)
    {
        return categoryId.HasValue ? query.Where(t => t.CategoryId == categoryId.Value) : query;
    }

    private IQueryable<Transaction> ApplyMonthFilter(IQueryable<Transaction> query, DateTime? month)
    {
        if (!month.HasValue)
            return query;

        var startDate = new DateTime(month.Value.Year, month.Value.Month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);
        return query.Where(t => t.Date >= startDate && t.Date <= endDate);
    }
}
