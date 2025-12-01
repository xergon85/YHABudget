using Microsoft.EntityFrameworkCore;
using YHABudget.Data.Context;
using YHABudget.Data.Enums;
using YHABudget.Data.Models;
using YHABudget.Data.Services;

namespace YHABudget.Tests.Services;

public class RecurringTransactionServiceTests : IDisposable
{
    private readonly BudgetDbContext _context;
    private readonly RecurringTransactionService _service;
    
    public RecurringTransactionServiceTests()
    {
        var options = new DbContextOptionsBuilder<BudgetDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new BudgetDbContext(options);
        _context.Database.EnsureCreated();
        _service = new RecurringTransactionService(_context);
    }
    
    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
    
    [Fact]
    public void AddRecurringTransaction_WithMonthlyRecurrence_ReturnsTransactionWithId()
    {
        // Arrange
        var recurringTransaction = new RecurringTransaction
        {
            Description = "Hyra",
            Amount = 8500m,
            CategoryId = 2, // Hus & drift
            Type = TransactionType.Expense,
            RecurrenceType = RecurrenceType.Monthly,
            StartDate = new DateTime(2025, 1, 1),
            IsActive = true
        };
        
        // Act
        var result = _service.AddRecurringTransaction(recurringTransaction);
        
        // Assert
        Assert.NotEqual(0, result.Id);
        Assert.Equal(recurringTransaction.Description, result.Description);
        Assert.Equal(RecurrenceType.Monthly, result.RecurrenceType);
        Assert.Null(result.RecurrenceMonth);
    }
    
    [Fact]
    public void AddRecurringTransaction_WithYearlyRecurrence_StoresRecurrenceMonth()
    {
        // Arrange
        var recurringTransaction = new RecurringTransaction
        {
            Description = "Bilförsäkring",
            Amount = 8900m,
            CategoryId = 8, // Försäkring
            Type = TransactionType.Expense,
            RecurrenceType = RecurrenceType.Yearly,
            RecurrenceMonth = 1, // January
            StartDate = new DateTime(2025, 1, 1),
            IsActive = true
        };
        
        // Act
        var result = _service.AddRecurringTransaction(recurringTransaction);
        
        // Assert
        Assert.NotEqual(0, result.Id);
        Assert.Equal(RecurrenceType.Yearly, result.RecurrenceType);
        Assert.Equal(1, result.RecurrenceMonth);
    }
    
    [Fact]
    public void GetAllRecurringTransactions_ReturnsAllTransactions()
    {
        // Arrange
        _service.AddRecurringTransaction(new RecurringTransaction 
        { 
            Description = "Monthly", 
            Amount = 100m, 
            CategoryId = 1, 
            Type = TransactionType.Expense, 
            RecurrenceType = RecurrenceType.Monthly, 
            StartDate = DateTime.Now,
            IsActive = true 
        });
        _service.AddRecurringTransaction(new RecurringTransaction 
        { 
            Description = "Yearly", 
            Amount = 200m, 
            CategoryId = 1, 
            Type = TransactionType.Expense, 
            RecurrenceType = RecurrenceType.Yearly, 
            RecurrenceMonth = 6,
            StartDate = DateTime.Now,
            IsActive = false 
        });
        
        // Act
        var result = _service.GetAllRecurringTransactions();
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }
    
    [Fact]
    public void GetActiveRecurringTransactions_ReturnsOnlyActiveTransactions()
    {
        // Arrange
        _service.AddRecurringTransaction(new RecurringTransaction 
        { 
            Description = "Active", 
            Amount = 100m, 
            CategoryId = 1, 
            Type = TransactionType.Expense, 
            RecurrenceType = RecurrenceType.Monthly, 
            StartDate = DateTime.Now,
            IsActive = true 
        });
        _service.AddRecurringTransaction(new RecurringTransaction 
        { 
            Description = "Inactive", 
            Amount = 200m, 
            CategoryId = 1, 
            Type = TransactionType.Expense, 
            RecurrenceType = RecurrenceType.Monthly, 
            StartDate = DateTime.Now,
            IsActive = false 
        });
        
        // Act
        var result = _service.GetActiveRecurringTransactions();
        
        // Assert
        Assert.Single(result);
        Assert.All(result, t => Assert.True(t.IsActive));
    }
    
    [Fact]
    public void ToggleActive_TogglesIsActiveStatus()
    {
        // Arrange
        var transaction = _service.AddRecurringTransaction(new RecurringTransaction 
        { 
            Description = "Test", 
            Amount = 100m, 
            CategoryId = 1, 
            Type = TransactionType.Expense, 
            RecurrenceType = RecurrenceType.Monthly, 
            StartDate = DateTime.Now,
            IsActive = true 
        });
        
        // Act
        _service.ToggleActive(transaction.Id);
        var result = _service.GetRecurringTransactionById(transaction.Id);
        
        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsActive);
        
        // Toggle back
        _service.ToggleActive(transaction.Id);
        result = _service.GetRecurringTransactionById(transaction.Id);
        Assert.True(result!.IsActive);
    }
    
    [Fact]
    public void UpdateRecurringTransaction_UpdatesExistingTransaction()
    {
        // Arrange
        var transaction = _service.AddRecurringTransaction(new RecurringTransaction 
        { 
            Description = "Original", 
            Amount = 1000m, 
            CategoryId = 1, 
            Type = TransactionType.Expense, 
            RecurrenceType = RecurrenceType.Monthly, 
            StartDate = DateTime.Now,
            IsActive = true 
        });
        
        // Act
        transaction.Description = "Updated";
        transaction.Amount = 2000m;
        _service.UpdateRecurringTransaction(transaction);
        var result = _service.GetRecurringTransactionById(transaction.Id);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated", result.Description);
        Assert.Equal(2000m, result.Amount);
    }
    
    [Fact]
    public void DeleteRecurringTransaction_RemovesTransactionFromDatabase()
    {
        // Arrange
        var transaction = _service.AddRecurringTransaction(new RecurringTransaction 
        { 
            Description = "ToDelete", 
            Amount = 100m, 
            CategoryId = 1, 
            Type = TransactionType.Expense, 
            RecurrenceType = RecurrenceType.Monthly, 
            StartDate = DateTime.Now,
            IsActive = true 
        });
        
        // Act
        _service.DeleteRecurringTransaction(transaction.Id);
        var result = _service.GetRecurringTransactionById(transaction.Id);
        
        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ProcessRecurringTransactionsForMonth_MonthlyRecurrence_CreatesTransaction()
    {
        // Arrange
        var category = new Category { Name = "Lön", Type = TransactionType.Income };
        _context.Categories.Add(category);
        _context.SaveChanges();

        var recurring = new RecurringTransaction
        {
            Description = "Månadslön",
            Amount = 35000m,
            CategoryId = category.Id,
            Type = TransactionType.Income,
            RecurrenceType = RecurrenceType.Monthly,
            StartDate = new DateTime(2025, 1, 25),
            IsActive = true
        };
        _service.AddRecurringTransaction(recurring);

        var targetMonth = new DateTime(2025, 11, 1);

        // Act
        _service.ProcessRecurringTransactionsForMonth(targetMonth);

        // Assert
        var transactions = _context.Transactions
            .Where(t => t.Date >= targetMonth && t.Date < targetMonth.AddMonths(1))
            .ToList();

        Assert.Single(transactions);
        var transaction = transactions.First();
        Assert.Equal("Månadslön", transaction.Description);
        Assert.Equal(35000m, transaction.Amount);
        Assert.Equal(TransactionType.Income, transaction.Type);
        Assert.Equal(new DateTime(2025, 11, 25), transaction.Date);
        Assert.True(transaction.IsRecurring);
    }

    [Fact]
    public void ProcessRecurringTransactionsForMonth_YearlyRecurrence_CreatesTransactionInCorrectMonth()
    {
        // Arrange
        var category = new Category { Name = "Bilförsäkring", Type = TransactionType.Expense };
        _context.Categories.Add(category);
        _context.SaveChanges();

        var recurring = new RecurringTransaction
        {
            Description = "Årlig bilförsäkring",
            Amount = 8900m,
            CategoryId = category.Id,
            Type = TransactionType.Expense,
            RecurrenceType = RecurrenceType.Yearly,
            RecurrenceMonth = 6, // June
            StartDate = new DateTime(2024, 6, 15),
            IsActive = true
        };
        _service.AddRecurringTransaction(recurring);

        var targetMonth = new DateTime(2025, 6, 1);

        // Act
        _service.ProcessRecurringTransactionsForMonth(targetMonth);

        // Assert
        var transactions = _context.Transactions
            .Where(t => t.Date >= targetMonth && t.Date < targetMonth.AddMonths(1))
            .ToList();

        Assert.Single(transactions);
        var transaction = transactions.First();
        Assert.Equal("Årlig bilförsäkring", transaction.Description);
        Assert.Equal(8900m, transaction.Amount);
        Assert.Equal(TransactionType.Expense, transaction.Type);
        Assert.Equal(new DateTime(2025, 6, 15), transaction.Date);
        Assert.True(transaction.IsRecurring);
    }

    [Fact]
    public void ProcessRecurringTransactionsForMonth_YearlyRecurrence_DoesNotCreateInWrongMonth()
    {
        // Arrange
        var category = new Category { Name = "Bilförsäkring", Type = TransactionType.Expense };
        _context.Categories.Add(category);
        _context.SaveChanges();

        var recurring = new RecurringTransaction
        {
            Description = "Årlig bilförsäkring",
            Amount = 8900m,
            CategoryId = category.Id,
            Type = TransactionType.Expense,
            RecurrenceType = RecurrenceType.Yearly,
            RecurrenceMonth = 6, // June
            StartDate = new DateTime(2024, 6, 15),
            IsActive = true
        };
        _service.AddRecurringTransaction(recurring);

        var targetMonth = new DateTime(2025, 11, 1); // November - should not create

        // Act
        _service.ProcessRecurringTransactionsForMonth(targetMonth);

        // Assert
        var transactions = _context.Transactions
            .Where(t => t.Date >= targetMonth && t.Date < targetMonth.AddMonths(1))
            .ToList();

        Assert.Empty(transactions);
    }

    [Fact]
    public void ProcessRecurringTransactionsForMonth_InactiveRecurring_DoesNotCreateTransaction()
    {
        // Arrange
        var category = new Category { Name = "Lön", Type = TransactionType.Income };
        _context.Categories.Add(category);
        _context.SaveChanges();

        var recurring = new RecurringTransaction
        {
            Description = "Inaktiv lön",
            Amount = 35000m,
            CategoryId = category.Id,
            Type = TransactionType.Income,
            RecurrenceType = RecurrenceType.Monthly,
            StartDate = new DateTime(2025, 1, 25),
            IsActive = false // Inactive
        };
        _service.AddRecurringTransaction(recurring);

        var targetMonth = new DateTime(2025, 11, 1);

        // Act
        _service.ProcessRecurringTransactionsForMonth(targetMonth);

        // Assert
        var transactions = _context.Transactions
            .Where(t => t.Date >= targetMonth && t.Date < targetMonth.AddMonths(1))
            .ToList();

        Assert.Empty(transactions);
    }

    [Fact]
    public void ProcessRecurringTransactionsForMonth_CalledTwice_DoesNotCreateDuplicates()
    {
        // Arrange
        var category = new Category { Name = "Lön", Type = TransactionType.Income };
        _context.Categories.Add(category);
        _context.SaveChanges();

        var recurring = new RecurringTransaction
        {
            Description = "Månadslön",
            Amount = 35000m,
            CategoryId = category.Id,
            Type = TransactionType.Income,
            RecurrenceType = RecurrenceType.Monthly,
            StartDate = new DateTime(2025, 1, 25),
            IsActive = true
        };
        _service.AddRecurringTransaction(recurring);

        var targetMonth = new DateTime(2025, 11, 1);

        // Act
        _service.ProcessRecurringTransactionsForMonth(targetMonth);
        _service.ProcessRecurringTransactionsForMonth(targetMonth); // Call twice

        // Assert
        var transactions = _context.Transactions
            .Where(t => t.Date >= targetMonth && t.Date < targetMonth.AddMonths(1))
            .ToList();

        Assert.Single(transactions); // Should only have one transaction
    }

    [Fact]
    public void ProcessRecurringTransactionsForMonth_MultipleRecurring_CreatesAllApplicable()
    {
        // Arrange
        var incomeCategory = new Category { Name = "Lön", Type = TransactionType.Income };
        var expenseCategory = new Category { Name = "Hyra", Type = TransactionType.Expense };
        _context.Categories.AddRange(incomeCategory, expenseCategory);
        _context.SaveChanges();

        _service.AddRecurringTransaction(new RecurringTransaction
        {
            Description = "Månadslön",
            Amount = 35000m,
            CategoryId = incomeCategory.Id,
            Type = TransactionType.Income,
            RecurrenceType = RecurrenceType.Monthly,
            StartDate = new DateTime(2025, 1, 25),
            IsActive = true
        });

        _service.AddRecurringTransaction(new RecurringTransaction
        {
            Description = "Hyra",
            Amount = 8500m,
            CategoryId = expenseCategory.Id,
            Type = TransactionType.Expense,
            RecurrenceType = RecurrenceType.Monthly,
            StartDate = new DateTime(2025, 1, 1),
            IsActive = true
        });

        var targetMonth = new DateTime(2025, 11, 1);

        // Act
        _service.ProcessRecurringTransactionsForMonth(targetMonth);

        // Assert
        var transactions = _context.Transactions
            .Where(t => t.Date >= targetMonth && t.Date < targetMonth.AddMonths(1))
            .ToList();

        Assert.Equal(2, transactions.Count);
        Assert.Contains(transactions, t => t.Description == "Månadslön" && t.Type == TransactionType.Income);
        Assert.Contains(transactions, t => t.Description == "Hyra" && t.Type == TransactionType.Expense);
    }
}
