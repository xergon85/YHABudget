using Microsoft.EntityFrameworkCore;
using YHABudget.Data.Context;
using YHABudget.Data.Enums;
using YHABudget.Data.Models;

namespace YHABudget.Data.Services;

public class CalculationService : ICalculationService
{
    private readonly BudgetDbContext _context;

    public CalculationService(BudgetDbContext context)
    {
        _context = context;
    }

    public decimal CalculateMonthlyIncome(decimal annualIncome, decimal annualHours)
    {
        if (annualIncome <= 0)
            return 0m;

        return annualIncome / 12m;
    }

    public async Task<List<Transaction>> GenerateTransactionsFromRecurring(DateTime month)
    {
        var firstDayOfMonth = new DateTime(month.Year, month.Month, 1);
        var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

        var activeRecurring = await _context.RecurringTransactions
            .Include(rt => rt.Category)
            .Where(rt => rt.IsActive)
            .Where(rt => rt.StartDate <= lastDayOfMonth)
            .Where(rt => rt.EndDate == null || rt.EndDate >= firstDayOfMonth)
            .ToListAsync();

        var transactions = new List<Transaction>();

        foreach (var recurring in activeRecurring)
        {
            bool shouldGenerate = false;

            if (recurring.RecurrenceType == RecurrenceType.Monthly)
            {
                shouldGenerate = true;
            }
            else if (recurring.RecurrenceType == RecurrenceType.Yearly)
            {
                shouldGenerate = recurring.RecurrenceMonth == month.Month;
            }

            if (shouldGenerate)
            {
                var transaction = new Transaction
                {
                    Amount = recurring.Amount,
                    Description = recurring.Description,
                    CategoryId = recurring.CategoryId,
                    Type = recurring.Type,
                    Date = firstDayOfMonth,
                    IsRecurring = true,
                    Category = recurring.Category
                };

                transactions.Add(transaction);
            }
        }

        return transactions;
    }
}
