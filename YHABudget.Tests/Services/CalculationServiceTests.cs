using YHABudget.Data.Context;
using YHABudget.Data.Enums;
using YHABudget.Data.Models;
using YHABudget.Data.Services;
using Microsoft.EntityFrameworkCore;

namespace YHABudget.Tests.Services;

public class CalculationServiceTests : IDisposable
{
    private readonly BudgetDbContext _context;
    private readonly CalculationService _service;

    public CalculationServiceTests()
    {
        var options = new DbContextOptionsBuilder<BudgetDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BudgetDbContext(options);
        _service = new CalculationService(_context);
    }

    [Fact]
    public void CalculateMonthlyIncome_WithAnnualValues_ReturnsCorrectMonthlyAmount()
    {
        // Arrange
        decimal annualIncome = 480000m; // 480,000 kr/year
        decimal annualHours = 1920m; // 40h/week * 48 weeks

        // Act
        var result = _service.CalculateMonthlyIncome(annualIncome, annualHours);

        // Assert
        Assert.Equal(40000m, result); // 480,000 / 12 = 40,000 kr/month
    }

    [Fact]
    public void CalculateMonthlyIncome_WithZeroAnnualIncome_ReturnsZero()
    {
        // Arrange
        decimal annualIncome = 0m;
        decimal annualHours = 1920m;

        // Act
        var result = _service.CalculateMonthlyIncome(annualIncome, annualHours);

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public async Task GenerateTransactionsFromRecurring_CreatesMonthlyTransactions()
    {
        // Arrange
        var category = new Category { Id = 1, Name = "Hyra", Type = TransactionType.Expense };
        _context.Categories.Add(category);

        var recurringTransaction = new RecurringTransaction
        {
            Id = 1,
            Amount = 10000m,
            Description = "Hyra",
            CategoryId = 1,
            Type = TransactionType.Expense,
            RecurrenceType = RecurrenceType.Monthly,
            StartDate = new DateTime(2025, 1, 1),
            IsActive = true
        };
        _context.RecurringTransactions.Add(recurringTransaction);
        await _context.SaveChangesAsync();

        var targetMonth = new DateTime(2025, 11, 1);

        // Act
        var result = await _service.GenerateTransactionsFromRecurring(targetMonth);

        // Assert
        Assert.Single(result);
        var transaction = result.First();
        Assert.Equal(10000m, transaction.Amount);
        Assert.Equal("Hyra", transaction.Description);
        Assert.Equal(1, transaction.CategoryId);
        Assert.Equal(TransactionType.Expense, transaction.Type);
        Assert.Equal(new DateTime(2025, 11, 1), transaction.Date);
        Assert.True(transaction.IsRecurring);
    }

    [Fact]
    public async Task GenerateTransactionsFromRecurring_WithYearlyRecurrence_CreatesTransactionInCorrectMonth()
    {
        // Arrange
        var category = new Category { Id = 1, Name = "Försäkring", Type = TransactionType.Expense };
        _context.Categories.Add(category);

        var recurringTransaction = new RecurringTransaction
        {
            Id = 1,
            Amount = 5000m,
            Description = "Årlig försäkring",
            CategoryId = 1,
            Type = TransactionType.Expense,
            RecurrenceType = RecurrenceType.Yearly,
            RecurrenceMonth = 3, // March
            StartDate = new DateTime(2024, 3, 1),
            IsActive = true
        };
        _context.RecurringTransactions.Add(recurringTransaction);
        await _context.SaveChangesAsync();

        // Act - Check March 2025
        var marchResult = await _service.GenerateTransactionsFromRecurring(new DateTime(2025, 3, 1));
        // Act - Check April 2025
        var aprilResult = await _service.GenerateTransactionsFromRecurring(new DateTime(2025, 4, 1));

        // Assert
        Assert.Single(marchResult); // Should generate in March
        Assert.Empty(aprilResult); // Should NOT generate in April
        Assert.Equal("Årlig försäkring", marchResult.First().Description);
    }

    [Fact]
    public async Task GenerateTransactionsFromRecurring_IgnoresInactiveRecurringTransactions()
    {
        // Arrange
        var category = new Category { Id = 1, Name = "Test", Type = TransactionType.Expense };
        _context.Categories.Add(category);

        var inactiveRecurring = new RecurringTransaction
        {
            Id = 1,
            Amount = 1000m,
            Description = "Inactive",
            CategoryId = 1,
            Type = TransactionType.Expense,
            RecurrenceType = RecurrenceType.Monthly,
            StartDate = new DateTime(2025, 1, 1),
            IsActive = false // Inactive
        };
        _context.RecurringTransactions.Add(inactiveRecurring);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GenerateTransactionsFromRecurring(new DateTime(2025, 11, 1));

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GenerateTransactionsFromRecurring_RespectsStartAndEndDates()
    {
        // Arrange
        var category = new Category { Id = 1, Name = "Test", Type = TransactionType.Expense };
        _context.Categories.Add(category);

        var recurringTransaction = new RecurringTransaction
        {
            Id = 1,
            Amount = 1000m,
            Description = "Limited period",
            CategoryId = 1,
            Type = TransactionType.Expense,
            RecurrenceType = RecurrenceType.Monthly,
            StartDate = new DateTime(2025, 3, 1),
            EndDate = new DateTime(2025, 6, 30),
            IsActive = true
        };
        _context.RecurringTransactions.Add(recurringTransaction);
        await _context.SaveChangesAsync();

        // Act
        var beforeStart = await _service.GenerateTransactionsFromRecurring(new DateTime(2025, 2, 1));
        var withinPeriod = await _service.GenerateTransactionsFromRecurring(new DateTime(2025, 5, 1));
        var afterEnd = await _service.GenerateTransactionsFromRecurring(new DateTime(2025, 7, 1));

        // Assert
        Assert.Empty(beforeStart);
        Assert.Single(withinPeriod);
        Assert.Empty(afterEnd);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
