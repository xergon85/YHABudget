using Microsoft.EntityFrameworkCore;
using YHABudget.Data.Context;
using YHABudget.Data.DTOs;
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

    public ExpectedMonthResult CalculateExpectedMonthResult(DateTime selectedMonth, decimal currentNetBalance)
    {
        var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var isCurrentMonth = new DateTime(selectedMonth.Year, selectedMonth.Month, 1) == currentMonth;

        if (!isCurrentMonth)
        {
            return new ExpectedMonthResult
            {
                IsCurrentMonth = false,
                CurrentNetBalance = currentNetBalance,
                ProjectedNetBalance = 0,
                ScheduledIncome = 0,
                ScheduledExpenses = 0
            };
        }

        var monthEnd = currentMonth.AddMonths(1).AddDays(-1);
        decimal scheduledIncome = 0;
        decimal scheduledExpenses = 0;

        // 1. Get future non-recurring transactions in current month
        var allTransactionsThisMonth = _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.Date >= currentMonth && t.Date <= monthEnd)
            .ToList();

        var futureTransactions = allTransactionsThisMonth
            .Where(t => t.Date > DateTime.Today)
            .ToList();

        foreach (var transaction in futureTransactions)
        {
            if (transaction.Type == TransactionType.Income)
            {
                scheduledIncome += transaction.Amount;
            }
            else
            {
                scheduledExpenses += transaction.Amount;
            }
        }

        // 2. Get active recurring transactions that apply to current month but haven't been generated yet
        var activeRecurring = _context.RecurringTransactions
            .Include(rt => rt.Category)
            .Where(rt => rt.IsActive)
            .Where(rt => rt.StartDate <= monthEnd)
            .Where(rt => rt.EndDate == null || rt.EndDate >= currentMonth)
            .ToList();

        foreach (var recurring in activeRecurring)
        {
            bool appliesToMonth = false;
            DateTime transactionDate = currentMonth;

            if (recurring.RecurrenceType == RecurrenceType.Monthly)
            {
                appliesToMonth = true;
                int day = Math.Min(recurring.StartDate.Day, DateTime.DaysInMonth(currentMonth.Year, currentMonth.Month));
                transactionDate = new DateTime(currentMonth.Year, currentMonth.Month, day);
            }
            else if (recurring.RecurrenceType == RecurrenceType.Yearly && recurring.RecurrenceMonth.HasValue)
            {
                if (currentMonth.Month == recurring.RecurrenceMonth.Value)
                {
                    appliesToMonth = true;
                    int day = Math.Min(recurring.StartDate.Day, DateTime.DaysInMonth(currentMonth.Year, currentMonth.Month));
                    transactionDate = new DateTime(currentMonth.Year, currentMonth.Month, day);
                }
            }

            // Only count scheduled transactions (future dates) that haven't been generated yet
            if (appliesToMonth && transactionDate > DateTime.Today && transactionDate >= recurring.StartDate.Date)
            {
                if (recurring.Type == TransactionType.Income)
                {
                    scheduledIncome += recurring.Amount;
                }
                else
                {
                    scheduledExpenses += recurring.Amount;
                }
            }
        }

        return new ExpectedMonthResult
        {
            IsCurrentMonth = true,
            CurrentNetBalance = currentNetBalance,
            ScheduledIncome = scheduledIncome,
            ScheduledExpenses = scheduledExpenses,
            ProjectedNetBalance = currentNetBalance + scheduledIncome - scheduledExpenses
        };
    }
}
