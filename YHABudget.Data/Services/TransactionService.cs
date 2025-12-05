using Microsoft.EntityFrameworkCore;
using YHABudget.Data.Context;
using YHABudget.Data.Enums;
using YHABudget.Data.Models;
using YHABudget.Data.Queries;

namespace YHABudget.Data.Services;

public class TransactionService : ITransactionService
{
    private readonly BudgetDbContext _context;
    private readonly TransactionQueries _queries;
    
    public TransactionService(BudgetDbContext context)
    {
        _context = context;
        _queries = new TransactionQueries(context);
    }
    
    public Transaction AddTransaction(Transaction transaction)
    {
        _context.Transactions.Add(transaction);
        _context.SaveChanges();
        return transaction;
    }
    
    public IEnumerable<Transaction> GetAllTransactions()
    {
        return _queries.GetAllTransactions();
    }
    
    public Transaction? GetTransactionById(int id)
    {
        return _queries.GetTransactionById(id);
    }
    
    public IEnumerable<Transaction> GetTransactionsByMonth(DateTime month)
    {
        var startDate = new DateTime(month.Year, month.Month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);
        
        return _queries.GetTransactionsForDateRangeWithCategory(startDate, endDate);
    }
    
    public IEnumerable<Transaction> GetTransactionsByFilter(TransactionType? type, int? categoryId, DateTime? month)
    {
        return _queries.GetTransactionsByFilter(type, categoryId, month);
    }

    public IEnumerable<DateTime> GetMonthsWithTransactions()
    {
        var transactions = _context.Transactions.ToList();
        return transactions
            .Select(t => new DateTime(t.Date.Year, t.Date.Month, 1))
            .Distinct()
            .OrderByDescending(d => d)
            .ToList();
    }
    
    public void UpdateTransaction(Transaction transaction)
    {
        _context.Transactions.Update(transaction);
        _context.SaveChanges();
    }
    
    public void DeleteTransaction(int id)
    {
        var transaction = _context.Transactions.Find(id);
        if (transaction != null)
        {
            _context.Transactions.Remove(transaction);
            _context.SaveChanges();
        }
    }
}
