using Microsoft.EntityFrameworkCore;
using YHABudget.Data.Context;
using YHABudget.Data.Enums;
using YHABudget.Data.Models;
using YHABudget.Data.Services;

namespace YHABudget.Tests.Services;

public class TransactionServiceTests : IDisposable
{
    private readonly BudgetDbContext _context;
    private readonly TransactionService _service;
    
    public TransactionServiceTests()
    {
        var options = new DbContextOptionsBuilder<BudgetDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new BudgetDbContext(options);
        _context.Database.EnsureCreated();
        _service = new TransactionService(_context);
    }
    
    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
    [Fact]
    public void AddTransaction_WithValidData_ReturnsTransactionWithId()
    {
        // Arrange
        var transaction = new Transaction
        {
            Amount = 1500m,
            Description = "ICA Maxi",
            Date = new DateTime(2025, 11, 28),
            CategoryId = 1, // Mat category from seed data
            Type = TransactionType.Expense
        };
        
        // Act
        var result = _service.AddTransaction(transaction);
        
        // Assert
        Assert.NotEqual(0, result.Id);
        Assert.Equal(transaction.Amount, result.Amount);
        Assert.Equal(transaction.Description, result.Description);
    }
    
    [Fact]
    public void GetAllTransactions_ReturnsAllTransactions()
    {
        // Arrange
        _service.AddTransaction(new Transaction { Amount = 100m, Description = "Test 1", Date = DateTime.Now, CategoryId = 1, Type = TransactionType.Expense });
        _service.AddTransaction(new Transaction { Amount = 200m, Description = "Test 2", Date = DateTime.Now, CategoryId = 10, Type = TransactionType.Income });
        
        // Act
        var result = _service.GetAllTransactions();
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }
    
    [Fact]
    public void GetTransactionsByMonth_ReturnsOnlyTransactionsForSpecifiedMonth()
    {
        // Arrange
        var month = new DateTime(2025, 11, 1);
        _service.AddTransaction(new Transaction { Amount = 100m, Description = "Nov", Date = new DateTime(2025, 11, 15), CategoryId = 1, Type = TransactionType.Expense });
        _service.AddTransaction(new Transaction { Amount = 200m, Description = "Dec", Date = new DateTime(2025, 12, 15), CategoryId = 1, Type = TransactionType.Expense });
        
        // Act
        var result = _service.GetTransactionsByMonth(month);
        
        // Assert
        Assert.Single(result);
        Assert.All(result, t => Assert.Equal(11, t.Date.Month));
        Assert.All(result, t => Assert.Equal(2025, t.Date.Year));
    }
    
    [Fact]
    public void UpdateTransaction_UpdatesExistingTransaction()
    {
        // Arrange
        var transaction = _service.AddTransaction(new Transaction { Amount = 1000m, Description = "Original", Date = DateTime.Now, CategoryId = 1, Type = TransactionType.Expense });
        var updatedDescription = "Updated description";
        var updatedAmount = 2000m;
        
        // Act
        transaction.Description = updatedDescription;
        transaction.Amount = updatedAmount;
        _service.UpdateTransaction(transaction);
        var result = _service.GetTransactionById(transaction.Id);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(updatedDescription, result.Description);
        Assert.Equal(updatedAmount, result.Amount);
    }
    
    [Fact]
    public void DeleteTransaction_RemovesTransactionFromDatabase()
    {
        // Arrange
        var transaction = _service.AddTransaction(new Transaction { Amount = 100m, Description = "ToDelete", Date = DateTime.Now, CategoryId = 1, Type = TransactionType.Expense });
        
        // Act
        _service.DeleteTransaction(transaction.Id);
        var result = _service.GetTransactionById(transaction.Id);
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public void GetTransactionsByType_ReturnsOnlyExpenses()
    {
        // Arrange
        _service.AddTransaction(new Transaction { Amount = 100m, Description = "Expense", Date = DateTime.Now, CategoryId = 1, Type = TransactionType.Expense });
        _service.AddTransaction(new Transaction { Amount = 200m, Description = "Income", Date = DateTime.Now, CategoryId = 10, Type = TransactionType.Income });
        
        // Act
        var result = _service.GetTransactionsByFilter(TransactionType.Expense, null, null);
        
        // Assert
        Assert.Single(result);
        Assert.All(result, t => Assert.Equal(TransactionType.Expense, t.Type));
    }
}
