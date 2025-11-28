using Microsoft.EntityFrameworkCore;
using YHABudget.Data.Context;
using YHABudget.Data.Enums;
using YHABudget.Data.Models;
using YHABudget.Data.Services;

namespace YHABudget.Tests.Services;

public class CategoryServiceTests : IDisposable
{
    private readonly BudgetDbContext _context;
    private readonly CategoryService _service;
    
    public CategoryServiceTests()
    {
        var options = new DbContextOptionsBuilder<BudgetDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new BudgetDbContext(options);
        _context.Database.EnsureCreated();
        _service = new CategoryService(_context);
    }
    
    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
    
    [Fact]
    public void GetAllCategories_ReturnsSeedData()
    {
        // Act
        var result = _service.GetAllCategories();
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(12, result.Count()); // 9 expense + 3 income categories from seed
    }
    
    [Fact]
    public void GetCategoriesByType_ReturnsOnlyExpenseCategories()
    {
        // Act
        var result = _service.GetCategoriesByType(TransactionType.Expense);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(9, result.Count());
        Assert.All(result, c => Assert.Equal(TransactionType.Expense, c.Type));
    }
    
    [Fact]
    public void GetCategoriesByType_ReturnsOnlyIncomeCategories()
    {
        // Act
        var result = _service.GetCategoriesByType(TransactionType.Income);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
        Assert.All(result, c => Assert.Equal(TransactionType.Income, c.Type));
    }
    
    [Fact]
    public void AddCategory_WithValidData_ReturnsCategoryWithId()
    {
        // Arrange
        var category = new Category
        {
            Name = "Ny Kategori",
            Type = TransactionType.Expense
        };
        
        // Act
        var result = _service.AddCategory(category);
        
        // Assert
        Assert.NotEqual(0, result.Id);
        Assert.Equal(category.Name, result.Name);
        Assert.Equal(category.Type, result.Type);
    }
    
    [Fact]
    public void UpdateCategory_UpdatesExistingCategory()
    {
        // Arrange
        var category = _service.AddCategory(new Category { Name = "Original", Type = TransactionType.Expense });
        var updatedName = "Uppdaterad Kategori";
        
        // Act
        category.Name = updatedName;
        _service.UpdateCategory(category);
        var result = _service.GetCategoryById(category.Id);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(updatedName, result.Name);
    }
    
    [Fact]
    public void DeleteCategory_RemovesCategoryFromDatabase()
    {
        // Arrange
        var category = _service.AddCategory(new Category { Name = "ToDelete", Type = TransactionType.Expense });
        
        // Act
        _service.DeleteCategory(category.Id);
        var result = _service.GetCategoryById(category.Id);
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public void GetCategoryById_ReturnsCorrectCategory()
    {
        // Arrange - Use seeded category "Mat" with Id 1
        
        // Act
        var result = _service.GetCategoryById(1);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Mat", result.Name);
        Assert.Equal(TransactionType.Expense, result.Type);
    }
}
