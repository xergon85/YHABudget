using Microsoft.EntityFrameworkCore;
using YHABudget.Data.Context;
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
}
