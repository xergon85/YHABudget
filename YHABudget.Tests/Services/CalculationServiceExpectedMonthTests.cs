using Microsoft.EntityFrameworkCore;
using YHABudget.Data.Context;
using YHABudget.Data.Enums;
using YHABudget.Data.Models;
using YHABudget.Data.Services;

namespace YHABudget.Tests.Services;

public class CalculationServiceExpectedMonthTests : IDisposable
{
    private readonly BudgetDbContext _context;
    private readonly CalculationService _service;

    public CalculationServiceExpectedMonthTests()
    {
        var options = new DbContextOptionsBuilder<BudgetDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BudgetDbContext(options);
        _service = new CalculationService(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public void CalculateExpectedMonthResult_NotCurrentMonth_ReturnsZeroValues()
    {
        // Arrange
        var pastMonth = DateTime.Now.AddMonths(-1);

        // Act
        var result = _service.CalculateExpectedMonthResult(pastMonth);

        // Assert
        Assert.False(result.IsCurrentMonth);
        Assert.Equal(0, result.ScheduledIncome);
        Assert.Equal(0, result.ScheduledExpenses);
        Assert.Equal(0, result.ProjectedNetBalance);
        Assert.Equal(0, result.ExpectedAccountBalance);
        Assert.Empty(result.ScheduledIncomeTransactions);
        Assert.Empty(result.ScheduledExpenseTransactions);
    }

    [Fact]
    public void CalculateExpectedMonthResult_CurrentMonth_NoScheduledTransactions_ReturnsCurrentBalance()
    {
        // Arrange
        var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

        // Act
        var result = _service.CalculateExpectedMonthResult(currentMonth);

        // Assert
        Assert.True(result.IsCurrentMonth);
        Assert.Equal(0, result.ScheduledIncome);
        Assert.Equal(0, result.ScheduledExpenses);
        Assert.Equal(0, result.ProjectedNetBalance); // No transactions = 0 net balance
        Assert.Empty(result.ScheduledIncomeTransactions);
        Assert.Empty(result.ScheduledExpenseTransactions);
    }

    [Fact]
    public void CalculateExpectedMonthResult_IncludesFutureNonRecurringTransactions()
    {
        // Arrange
        var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var futureDate = DateTime.Today.AddDays(5);

        var category = new Category { Id = 1, Name = "Test Category", Type = TransactionType.Income };
        _context.Categories.Add(category);

        // Future income transaction
        _context.Transactions.Add(new Transaction
        {
            Id = 1,
            Description = "Future Income",
            Amount = 3000m,
            Date = futureDate,
            Type = TransactionType.Income,
            CategoryId = 1,
            Category = category,
            IsRecurring = false
        });

        // Future expense transaction
        _context.Transactions.Add(new Transaction
        {
            Id = 2,
            Description = "Future Expense",
            Amount = 500m,
            Date = futureDate,
            Type = TransactionType.Expense,
            CategoryId = 1,
            Category = category,
            IsRecurring = false
        });

        _context.SaveChanges();

        // Act
        var result = _service.CalculateExpectedMonthResult(currentMonth);

        // Assert
        Assert.True(result.IsCurrentMonth);
        Assert.Equal(3000m, result.ScheduledIncome);
        Assert.Equal(500m, result.ScheduledExpenses);
        Assert.Equal(2500m, result.ProjectedNetBalance); // 0 + 3000 - 500
        Assert.Single(result.ScheduledIncomeTransactions);
        Assert.Single(result.ScheduledExpenseTransactions);
        Assert.Equal("Future Income", result.ScheduledIncomeTransactions[0].Description);
        Assert.Equal("Future Expense", result.ScheduledExpenseTransactions[0].Description);
    }

    [Fact]
    public void CalculateExpectedMonthResult_ExcludesPastTransactions()
    {
        // Arrange
        var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var pastDate = DateTime.Today.AddDays(-1);

        var category = new Category { Id = 1, Name = "Test Category", Type = TransactionType.Income };
        _context.Categories.Add(category);

        // Past transaction (should not be included)
        _context.Transactions.Add(new Transaction
        {
            Id = 1,
            Description = "Past Income",
            Amount = 2000m,
            Date = pastDate,
            Type = TransactionType.Income,
            CategoryId = 1,
            Category = category,
            IsRecurring = false
        });

        _context.SaveChanges();

        // Act
        var result = _service.CalculateExpectedMonthResult(currentMonth);

        // Assert
        Assert.Equal(0, result.ScheduledIncome);
        Assert.Empty(result.ScheduledIncomeTransactions);
    }

    [Fact]
    public void CalculateExpectedMonthResult_IncludesMonthlyRecurringTransactions()
    {
        // Arrange
        var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var futureDate = DateTime.Today.AddDays(10);

        var category = new Category { Id = 1, Name = "Salary", Type = TransactionType.Income };
        _context.Categories.Add(category);

        // Monthly recurring transaction that hasn't been generated yet
        _context.RecurringTransactions.Add(new RecurringTransaction
        {
            Id = 1,
            Description = "Monthly Salary",
            Amount = 25000m,
            Type = TransactionType.Income,
            RecurrenceType = RecurrenceType.Monthly,
            StartDate = currentMonth.AddDays(24), // Future date this month
            IsActive = true,
            CategoryId = 1,
            Category = category
        });

        _context.SaveChanges();

        // Act
        var result = _service.CalculateExpectedMonthResult(currentMonth);

        // Assert
        Assert.True(result.IsCurrentMonth);
        Assert.Equal(25000m, result.ScheduledIncome);
        Assert.Equal(25000m, result.ProjectedNetBalance); // 0 + 25000
        Assert.Single(result.ScheduledIncomeTransactions);
        Assert.Equal("Monthly Salary", result.ScheduledIncomeTransactions[0].Description);
    }

    [Fact]
    public void CalculateExpectedMonthResult_ExcludesInactiveRecurringTransactions()
    {
        // Arrange
        var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

        var category = new Category { Id = 1, Name = "Test", Type = TransactionType.Income };
        _context.Categories.Add(category);

        // Inactive recurring transaction
        _context.RecurringTransactions.Add(new RecurringTransaction
        {
            Id = 1,
            Description = "Inactive Income",
            Amount = 5000m,
            Type = TransactionType.Income,
            RecurrenceType = RecurrenceType.Monthly,
            StartDate = currentMonth.AddDays(15),
            IsActive = false, // Inactive
            CategoryId = 1,
            Category = category
        });

        _context.SaveChanges();

        // Act
        var result = _service.CalculateExpectedMonthResult(currentMonth);

        // Assert
        Assert.Equal(0, result.ScheduledIncome);
        Assert.Empty(result.ScheduledIncomeTransactions);
    }

    [Fact]
    public void CalculateExpectedMonthResult_IncludesYearlyRecurringForCorrectMonth()
    {
        // Arrange
        var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var futureDate = currentMonth.AddDays(20);

        var category = new Category { Id = 1, Name = "Insurance", Type = TransactionType.Expense };
        _context.Categories.Add(category);

        // Yearly recurring transaction for current month
        _context.RecurringTransactions.Add(new RecurringTransaction
        {
            Id = 1,
            Description = "Annual Insurance",
            Amount = 12000m,
            Type = TransactionType.Expense,
            RecurrenceType = RecurrenceType.Yearly,
            RecurrenceMonth = DateTime.Now.Month,
            StartDate = futureDate,
            IsActive = true,
            CategoryId = 1,
            Category = category
        });

        _context.SaveChanges();

        // Act
        var result = _service.CalculateExpectedMonthResult(currentMonth);

        // Assert
        Assert.Equal(12000m, result.ScheduledExpenses);
        Assert.Equal(-12000m, result.ProjectedNetBalance); // 0 - 12000
        Assert.Single(result.ScheduledExpenseTransactions);
        Assert.Equal("Annual Insurance", result.ScheduledExpenseTransactions[0].Description);
    }

    [Fact]
    public void CalculateExpectedMonthResult_ExcludesYearlyRecurringForWrongMonth()
    {
        // Arrange
        var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

        var category = new Category { Id = 1, Name = "Insurance", Type = TransactionType.Expense };
        _context.Categories.Add(category);

        // Yearly recurring transaction for different month
        var differentMonth = DateTime.Now.Month == 12 ? 1 : DateTime.Now.Month + 1;
        _context.RecurringTransactions.Add(new RecurringTransaction
        {
            Id = 1,
            Description = "Annual Insurance",
            Amount = 12000m,
            Type = TransactionType.Expense,
            RecurrenceType = RecurrenceType.Yearly,
            RecurrenceMonth = differentMonth,
            StartDate = currentMonth.AddDays(15),
            IsActive = true,
            CategoryId = 1,
            Category = category
        });

        _context.SaveChanges();

        // Act
        var result = _service.CalculateExpectedMonthResult(currentMonth);

        // Assert
        Assert.Equal(0, result.ScheduledExpenses);
        Assert.Empty(result.ScheduledExpenseTransactions);
    }

    [Fact]
    public void CalculateExpectedMonthResult_TransactionsSortedByDate()
    {
        // Arrange
        var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

        var category = new Category { Id = 1, Name = "Test", Type = TransactionType.Income };
        _context.Categories.Add(category);

        // Add transactions in random order
        _context.Transactions.Add(new Transaction
        {
            Id = 1,
            Description = "Income 3",
            Amount = 500m,
            Date = DateTime.Today.AddDays(10),
            Type = TransactionType.Income,
            CategoryId = 1,
            Category = category
        });

        _context.Transactions.Add(new Transaction
        {
            Id = 2,
            Description = "Income 1",
            Amount = 300m,
            Date = DateTime.Today.AddDays(2),
            Type = TransactionType.Income,
            CategoryId = 1,
            Category = category
        });

        _context.Transactions.Add(new Transaction
        {
            Id = 3,
            Description = "Income 2",
            Amount = 400m,
            Date = DateTime.Today.AddDays(5),
            Type = TransactionType.Income,
            CategoryId = 1,
            Category = category
        });

        _context.SaveChanges();

        // Act
        var result = _service.CalculateExpectedMonthResult(currentMonth);

        // Assert
        Assert.Equal(3, result.ScheduledIncomeTransactions.Count);
        Assert.Equal("Income 1", result.ScheduledIncomeTransactions[0].Description);
        Assert.Equal("Income 2", result.ScheduledIncomeTransactions[1].Description);
        Assert.Equal("Income 3", result.ScheduledIncomeTransactions[2].Description);
    }

    [Fact]
    public void CalculateExpectedMonthResult_CombinesAllScheduledSources()
    {
        // Arrange
        var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

        var category = new Category { Id = 1, Name = "Mixed", Type = TransactionType.Income };
        _context.Categories.Add(category);

        // Future non-recurring transaction
        _context.Transactions.Add(new Transaction
        {
            Id = 1,
            Description = "One-time Income",
            Amount = 1000m,
            Date = DateTime.Today.AddDays(3),
            Type = TransactionType.Income,
            CategoryId = 1,
            Category = category
        });

        // Monthly recurring transaction
        _context.RecurringTransactions.Add(new RecurringTransaction
        {
            Id = 1,
            Description = "Monthly Salary",
            Amount = 20000m,
            Type = TransactionType.Income,
            RecurrenceType = RecurrenceType.Monthly,
            StartDate = currentMonth.AddDays(25),
            IsActive = true,
            CategoryId = 1,
            Category = category
        });

        // Future expense
        _context.Transactions.Add(new Transaction
        {
            Id = 2,
            Description = "Upcoming Bill",
            Amount = 1500m,
            Date = DateTime.Today.AddDays(7),
            Type = TransactionType.Expense,
            CategoryId = 1,
            Category = category
        });

        _context.SaveChanges();

        // Act
        var result = _service.CalculateExpectedMonthResult(currentMonth);

        // Assert
        Assert.Equal(21000m, result.ScheduledIncome); // 1000 + 20000
        Assert.Equal(1500m, result.ScheduledExpenses);
        Assert.Equal(19500m, result.ProjectedNetBalance); // 0 + 21000 - 1500
        Assert.Equal(2, result.ScheduledIncomeTransactions.Count);
        Assert.Single(result.ScheduledExpenseTransactions);
    }
}
