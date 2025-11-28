using Microsoft.EntityFrameworkCore;
using YHABudget.Data.Context;
using YHABudget.Data.Enums;
using YHABudget.Data.Models;

namespace YHABudget.Data.Services;

public class TransactionService : ITransactionService
{
    private readonly BudgetDbContext _context;
    
    public TransactionService(BudgetDbContext context)
    {
        _context = context;
    }
    
    public Transaction AddTransaction(Transaction transaction)
    {
        _context.Transactions.Add(transaction);
        _context.SaveChanges();
        return transaction;
    }
    
    public IEnumerable<Transaction> GetAllTransactions()
    {
        return _context.Transactions
            .Include(t => t.Category)
            .OrderByDescending(t => t.Date)
            .ToList();
    }
    
    public Transaction? GetTransactionById(int id)
    {
        return _context.Transactions
            .Include(t => t.Category)
            .FirstOrDefault(t => t.Id == id);
    }
    
    public IEnumerable<Transaction> GetTransactionsByMonth(DateTime month)
    {
        var startDate = new DateTime(month.Year, month.Month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);
        
        return _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.Date >= startDate && t.Date <= endDate)
            .OrderByDescending(t => t.Date)
            .ToList();
    }
    
    public IEnumerable<Transaction> GetTransactionsByFilter(TransactionType? type, int? categoryId, DateTime? month)
    {
        var query = _context.Transactions
            .Include(t => t.Category)
            .AsQueryable();
        
        if (type.HasValue)
        {
            query = query.Where(t => t.Type == type.Value);
        }
        
        if (categoryId.HasValue)
        {
            query = query.Where(t => t.CategoryId == categoryId.Value);
        }
        
        if (month.HasValue)
        {
            var startDate = new DateTime(month.Value.Year, month.Value.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            query = query.Where(t => t.Date >= startDate && t.Date <= endDate);
        }
        
        return query.OrderByDescending(t => t.Date).ToList();
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
