using Microsoft.EntityFrameworkCore;
using YHABudget.Data.Context;
using YHABudget.Data.DTOs;
using YHABudget.Data.Enums;
using YHABudget.Data.Models;
using YHABudget.Data.Queries;

namespace YHABudget.Data.Services;

public class CalculationService : ICalculationService
{
    private readonly BudgetDbContext _context;
    private readonly TransactionQueries _transactionQueries;
    private readonly RecurringTransactionQueries _recurringQueries;

    public CalculationService(BudgetDbContext context)
    {
        _context = context;
        _transactionQueries = new TransactionQueries(context);
        _recurringQueries = new RecurringTransactionQueries(context);
    }

    public decimal CalculateMonthlyIncome(decimal annualIncome, decimal annualHours)
    {
        if (annualIncome <= 0 || annualHours <= 0)
            return 0m;

        // Calculate hourly rate and multiply by average monthly hours (160 hours = 40h/week * 4 weeks)
        decimal hourlyRate = annualIncome / annualHours;
        decimal monthlyHours = 160m; // Standard monthly working hours
        return hourlyRate * monthlyHours;
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

    public ExpectedMonthResult CalculateExpectedMonthResult(DateTime selectedMonth)
    {
        var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var selectedMonthStart = new DateTime(selectedMonth.Year, selectedMonth.Month, 1);
        var isCurrentMonth = selectedMonthStart == currentMonth;
        var isPastMonth = selectedMonthStart < currentMonth;
        var monthEnd = selectedMonthStart.AddMonths(1).AddDays(-1);

        var currentNetBalance = CalculateMonthNetBalance(selectedMonthStart, monthEnd, isPastMonth);
        var currentAccountBalance = CalculateAccountBalance();

        if (!isCurrentMonth)
        {
            if (isPastMonth)
            {
                return CalculatePastMonthResult(monthEnd, currentNetBalance);
            }

            // Future month - return zeros
            return new ExpectedMonthResult
            {
                IsCurrentMonth = false,
                IsPastMonth = false,
                CurrentNetBalance = currentNetBalance,
                ProjectedNetBalance = 0,
                ScheduledIncome = 0,
                ScheduledExpenses = 0,
                ExpectedAccountBalance = 0
            };
        }

        var (scheduledIncome, scheduledExpenses, scheduledIncomeTransactions, scheduledExpenseTransactions) =
            CalculateScheduledTransactions(currentMonth, monthEnd);

        var projectedNetBalance = currentNetBalance + scheduledIncome - scheduledExpenses;
        var expectedAccountBalance = currentAccountBalance + scheduledIncome - scheduledExpenses;

        return new ExpectedMonthResult
        {
            IsCurrentMonth = true,
            CurrentNetBalance = currentNetBalance,
            ScheduledIncome = scheduledIncome,
            ScheduledExpenses = scheduledExpenses,
            ProjectedNetBalance = projectedNetBalance,
            ExpectedAccountBalance = expectedAccountBalance,
            ScheduledIncomeTransactions = scheduledIncomeTransactions.OrderBy(t => t.Date).ToList(),
            ScheduledExpenseTransactions = scheduledExpenseTransactions.OrderBy(t => t.Date).ToList()
        };
    }

    public MonthOverview GetMonthOverview(DateTime selectedMonth)
    {
        var selectedMonthStart = new DateTime(selectedMonth.Year, selectedMonth.Month, 1);
        var monthEnd = selectedMonthStart.AddMonths(1).AddDays(-1);

        // Get all transactions for the selected month
        var transactions = _transactionQueries.GetTransactionsForMonth(selectedMonth);

        // Calculate income by category
        var incomeByCategory = transactions
            .Where(t => t.Type == TransactionType.Income)
            .GroupBy(t => t.Category?.Name ?? "Okategoriserad")
            .Select(g => new CategorySummary
            {
                CategoryName = g.Key,
                Total = g.Sum(t => t.Amount)
            })
            .OrderByDescending(x => x.Total)
            .ToList();

        // Calculate expenses by category
        var expensesByCategory = transactions
            .Where(t => t.Type == TransactionType.Expense)
            .GroupBy(t => t.Category?.Name ?? "Okategoriserad")
            .Select(g => new CategorySummary
            {
                CategoryName = g.Key,
                Total = g.Sum(t => t.Amount)
            })
            .OrderByDescending(x => x.Total)
            .ToList();

        var totalIncome = incomeByCategory.Sum(x => x.Total);
        var totalExpenses = expensesByCategory.Sum(x => x.Total);

        // Calculate account balance (all transactions up to today)
        var allTransactions = _transactionQueries.GetTransactionsUpToDate(DateTime.Today);
        var accountBalance = CalculateNetBalance(allTransactions);

        // Get available months from all transactions
        var allMonths = _transactionQueries.GetDistinctTransactionMonths();

        // If no transactions, add current month
        if (!allMonths.Any())
        {
            allMonths.Add(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1));
        }

        return new MonthOverview
        {
            TotalIncome = totalIncome,
            TotalExpenses = totalExpenses,
            NetBalance = totalIncome - totalExpenses,
            AccountBalance = accountBalance,
            IncomeByCategory = incomeByCategory,
            ExpensesByCategory = expensesByCategory,
            AvailableMonths = allMonths
        };
    }

    private decimal CalculateMonthNetBalance(DateTime monthStart, DateTime monthEnd, bool isPastMonth)
    {
        var transactions = _transactionQueries.GetTransactionsForDateRange(monthStart, monthEnd);
        
        if (!isPastMonth)
        {
            transactions = transactions.Where(t => t.Date <= DateTime.Today).ToList();
        }

        return CalculateNetBalance(transactions);
    }

    private decimal CalculateAccountBalance()
    {
        var transactions = _transactionQueries.GetTransactionsUpToDate(DateTime.Today);
        return CalculateNetBalance(transactions);
    }

    private decimal CalculateNetBalance(List<Transaction> transactions)
    {
        var income = SumTransactionsByType(transactions, TransactionType.Income);
        var expenses = SumTransactionsByType(transactions, TransactionType.Expense);
        return income - expenses;
    }

    private decimal SumTransactionsByType(List<Transaction> transactions, TransactionType type)
    {
        return transactions
            .Where(t => t.Type == type)
            .Sum(t => t.Amount);
    }

    private ExpectedMonthResult CalculatePastMonthResult(DateTime monthEnd, decimal currentNetBalance)
    {
        var transactionsUpToMonthEnd = _transactionQueries.GetTransactionsUpToDate(monthEnd);
        var accountBalanceAtMonthEnd = CalculateNetBalance(transactionsUpToMonthEnd);

        return new ExpectedMonthResult
        {
            IsCurrentMonth = false,
            IsPastMonth = true,
            CurrentNetBalance = currentNetBalance,
            ProjectedNetBalance = currentNetBalance,
            ScheduledIncome = 0,
            ScheduledExpenses = 0,
            ExpectedAccountBalance = accountBalanceAtMonthEnd
        };
    }

    private (decimal scheduledIncome,
            decimal scheduledExpenses,
            List<ScheduledTransactionSummary> incomeTransactions,
            List<ScheduledTransactionSummary> expenseTransactions)
        CalculateScheduledTransactions(DateTime currentMonth, DateTime monthEnd)
    {
        decimal scheduledIncome = 0;
        decimal scheduledExpenses = 0;
        var scheduledIncomeTransactions = new List<ScheduledTransactionSummary>();
        var scheduledExpenseTransactions = new List<ScheduledTransactionSummary>();

        // 1. Get future non-recurring transactions in current month
        var futureTransactions = _transactionQueries.GetFutureTransactionsForMonth(currentMonth, monthEnd);

        foreach (var transaction in futureTransactions)
        {
            var summary = new ScheduledTransactionSummary
            {
                Description = transaction.Description,
                Amount = transaction.Amount,
                Date = transaction.Date,
                CategoryName = transaction.Category?.Name ?? "Okategoriserad"
            };

            if (transaction.Type == TransactionType.Income)
            {
                scheduledIncome += transaction.Amount;
                scheduledIncomeTransactions.Add(summary);
            }
            else
            {
                scheduledExpenses += transaction.Amount;
                scheduledExpenseTransactions.Add(summary);
            }
        }

        // 2. Get active recurring transactions that apply to current month but haven't been generated yet
        var activeRecurring = _recurringQueries.GetActiveRecurringTransactionsStartingBefore(monthEnd)
            .Where(rt => rt.EndDate == null || rt.EndDate >= currentMonth)
            .ToList();

        foreach (var recurring in activeRecurring)
        {
            var scheduledDate = TryGetScheduledDate(recurring, currentMonth);
            if (!scheduledDate.HasValue)
                continue;

            // Skip if already processed for this month
            if (TransactionAlreadyExistsForRecurring(recurring, currentMonth, monthEnd))
                continue;

            var summary = CreateScheduledTransactionSummary(recurring, scheduledDate.Value);
            AddScheduledTransaction(summary, recurring.Type, ref scheduledIncome, ref scheduledExpenses,
                scheduledIncomeTransactions, scheduledExpenseTransactions);
        }

        return (scheduledIncome, scheduledExpenses, scheduledIncomeTransactions, scheduledExpenseTransactions);
    }

    private DateTime? TryGetScheduledDate(RecurringTransaction recurring, DateTime currentMonth)
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

        // Show the transaction if it applies to this month and started before or during this month
        if (!appliesToMonth || transactionDate < recurring.StartDate.Date)
            return null;

        return transactionDate;
    }

    private bool TransactionAlreadyExistsForRecurring(RecurringTransaction recurring, DateTime monthStart, DateTime monthEnd)
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

    private ScheduledTransactionSummary CreateScheduledTransactionSummary(RecurringTransaction recurring, DateTime date)
    {
        return new ScheduledTransactionSummary
        {
            Description = recurring.Description,
            Amount = recurring.Amount,
            Date = date,
            CategoryName = recurring.Category?.Name ?? "Okategoriserad"
        };
    }

    private void AddScheduledTransaction(
        ScheduledTransactionSummary summary,
        TransactionType type,
        ref decimal scheduledIncome,
        ref decimal scheduledExpenses,
        List<ScheduledTransactionSummary> incomeTransactions,
        List<ScheduledTransactionSummary> expenseTransactions)
    {
        if (type == TransactionType.Income)
        {
            scheduledIncome += summary.Amount;
            incomeTransactions.Add(summary);
        }
        else
        {
            scheduledExpenses += summary.Amount;
            expenseTransactions.Add(summary);
        }
    }
}
