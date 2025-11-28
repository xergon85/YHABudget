using YHABudget.Core.ViewModels;
using YHABudget.Data.Context;
using YHABudget.Data.Enums;
using YHABudget.Data.Models;
using YHABudget.Data.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace YHABudget.Tests.ViewModels;

public class OverviewViewModelTests : IDisposable
{
    private readonly BudgetDbContext _context;
    private readonly OverviewViewModel _viewModel;

    public OverviewViewModelTests()
    {
        var options = new DbContextOptionsBuilder<BudgetDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BudgetDbContext(options);
        
        var transactionService = new TransactionService(_context);
        var calculationService = new CalculationService(_context);
        
        _viewModel = new OverviewViewModel(transactionService, calculationService);
    }

    [Fact]
    public void Constructor_InitializesWithCurrentMonth()
    {
        // Assert
        Assert.Equal(DateTime.Now.Year, _viewModel.SelectedMonth.Year);
        Assert.Equal(DateTime.Now.Month, _viewModel.SelectedMonth.Month);
    }

    [Fact]
    public void LoadDataCommand_CalculatesTotalIncome()
    {
        // Arrange
        var category = new Category { Id = 1, Name = "Lön", Type = TransactionType.Income };
        _context.Categories.Add(category);
        
        var transaction1 = new Transaction 
        { 
            Amount = 35000m, 
            Date = new DateTime(2025, 11, 1), 
            CategoryId = 1, 
            Type = TransactionType.Income,
            Description = "Lön"
        };
        var transaction2 = new Transaction 
        { 
            Amount = 8000m, 
            Date = new DateTime(2025, 11, 15), 
            CategoryId = 1, 
            Type = TransactionType.Income,
            Description = "Bidrag"
        };
        _context.Transactions.AddRange(transaction1, transaction2);
        _context.SaveChanges();

        _viewModel.SelectedMonth = new DateTime(2025, 11, 1);

        // Act
        _viewModel.LoadDataCommand.Execute(null);

        // Assert
        Assert.Equal(43000m, _viewModel.TotalIncome);
    }

    [Fact]
    public void LoadDataCommand_CalculatesTotalExpenses()
    {
        // Arrange
        var category = new Category { Id = 1, Name = "Mat", Type = TransactionType.Expense };
        _context.Categories.Add(category);
        
        var transaction1 = new Transaction 
        { 
            Amount = 1250m, 
            Date = new DateTime(2025, 11, 12), 
            CategoryId = 1, 
            Type = TransactionType.Expense,
            Description = "ICA"
        };
        var transaction2 = new Transaction 
        { 
            Amount = 850m, 
            Date = new DateTime(2025, 11, 20), 
            CategoryId = 1, 
            Type = TransactionType.Expense,
            Description = "Bensin"
        };
        _context.Transactions.AddRange(transaction1, transaction2);
        _context.SaveChanges();

        _viewModel.SelectedMonth = new DateTime(2025, 11, 1);

        // Act
        _viewModel.LoadDataCommand.Execute(null);

        // Assert
        Assert.Equal(2100m, _viewModel.TotalExpenses);
    }

    [Fact]
    public void LoadDataCommand_CalculatesNetBalance()
    {
        // Arrange
        var incomeCategory = new Category { Id = 1, Name = "Lön", Type = TransactionType.Income };
        var expenseCategory = new Category { Id = 2, Name = "Mat", Type = TransactionType.Expense };
        _context.Categories.AddRange(incomeCategory, expenseCategory);
        
        _context.Transactions.Add(new Transaction 
        { 
            Amount = 35000m, 
            Date = new DateTime(2025, 11, 1), 
            CategoryId = 1, 
            Type = TransactionType.Income,
            Description = "Lön"
        });
        _context.Transactions.Add(new Transaction 
        { 
            Amount = 22500m, 
            Date = new DateTime(2025, 11, 5), 
            CategoryId = 2, 
            Type = TransactionType.Expense,
            Description = "Utgifter"
        });
        _context.SaveChanges();

        _viewModel.SelectedMonth = new DateTime(2025, 11, 1);

        // Act
        _viewModel.LoadDataCommand.Execute(null);

        // Assert
        Assert.Equal(12500m, _viewModel.NetBalance);
    }

    [Fact]
    public void LoadDataCommand_GroupsIncomeByCategory()
    {
        // Arrange
        var category1 = new Category { Id = 1, Name = "Lön", Type = TransactionType.Income };
        var category2 = new Category { Id = 2, Name = "Bidrag", Type = TransactionType.Income };
        _context.Categories.AddRange(category1, category2);
        
        _context.Transactions.Add(new Transaction 
        { 
            Amount = 35000m, 
            Date = new DateTime(2025, 11, 1), 
            CategoryId = 1, 
            Type = TransactionType.Income,
            Description = "Lön"
        });
        _context.Transactions.Add(new Transaction 
        { 
            Amount = 8000m, 
            Date = new DateTime(2025, 11, 15), 
            CategoryId = 2, 
            Type = TransactionType.Income,
            Description = "Bidrag"
        });
        _context.SaveChanges();

        _viewModel.SelectedMonth = new DateTime(2025, 11, 1);

        // Act
        _viewModel.LoadDataCommand.Execute(null);

        // Assert
        Assert.NotNull(_viewModel.IncomeByCategory);
        Assert.Equal(2, _viewModel.IncomeByCategory.Count);
        Assert.Contains(_viewModel.IncomeByCategory, x => x.CategoryName == "Lön" && x.Total == 35000m);
        Assert.Contains(_viewModel.IncomeByCategory, x => x.CategoryName == "Bidrag" && x.Total == 8000m);
    }

    [Fact]
    public void LoadDataCommand_GroupsExpensesByCategory()
    {
        // Arrange
        var category1 = new Category { Id = 1, Name = "Mat", Type = TransactionType.Expense };
        var category2 = new Category { Id = 2, Name = "Transport", Type = TransactionType.Expense };
        _context.Categories.AddRange(category1, category2);
        
        _context.Transactions.Add(new Transaction 
        { 
            Amount = 6500m, 
            Date = new DateTime(2025, 11, 12), 
            CategoryId = 1, 
            Type = TransactionType.Expense,
            Description = "Mat"
        });
        _context.Transactions.Add(new Transaction 
        { 
            Amount = 3000m, 
            Date = new DateTime(2025, 11, 15), 
            CategoryId = 2, 
            Type = TransactionType.Expense,
            Description = "Bensin"
        });
        _context.SaveChanges();

        _viewModel.SelectedMonth = new DateTime(2025, 11, 1);

        // Act
        _viewModel.LoadDataCommand.Execute(null);

        // Assert
        Assert.NotNull(_viewModel.ExpensesByCategory);
        Assert.Equal(2, _viewModel.ExpensesByCategory.Count);
        Assert.Contains(_viewModel.ExpensesByCategory, x => x.CategoryName == "Mat" && x.Total == 6500m);
        Assert.Contains(_viewModel.ExpensesByCategory, x => x.CategoryName == "Transport" && x.Total == 3000m);
    }

    [Fact]
    public void SelectedMonth_WhenChanged_RaisesPropertyChangedEvent()
    {
        // Arrange
        bool propertyChangedRaised = false;
        _viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(OverviewViewModel.SelectedMonth))
                propertyChangedRaised = true;
        };

        // Act
        _viewModel.SelectedMonth = new DateTime(2025, 10, 1);

        // Assert
        Assert.True(propertyChangedRaised);
    }

    [Fact]
    public void AccountBalance_CalculatesCorrectly_WithIncomeAndExpenses()
    {
        // Arrange
        var incomeCategory = new Category { Id = 1, Name = "Lön", Type = TransactionType.Income };
        var expenseCategory = new Category { Id = 2, Name = "Mat", Type = TransactionType.Expense };
        _context.Categories.AddRange(incomeCategory, expenseCategory);
        
        // Add transactions across different months
        _context.Transactions.Add(new Transaction 
        { 
            Amount = 35000m, 
            Date = new DateTime(2025, 10, 25), 
            CategoryId = 1, 
            Type = TransactionType.Income,
            Description = "Lön Oktober"
        });
        _context.Transactions.Add(new Transaction 
        { 
            Amount = 35000m, 
            Date = new DateTime(2025, 11, 25), 
            CategoryId = 1, 
            Type = TransactionType.Income,
            Description = "Lön November"
        });
        _context.Transactions.Add(new Transaction 
        { 
            Amount = 12000m, 
            Date = new DateTime(2025, 10, 5), 
            CategoryId = 2, 
            Type = TransactionType.Expense,
            Description = "Utgifter Oktober"
        });
        _context.Transactions.Add(new Transaction 
        { 
            Amount = 15000m, 
            Date = new DateTime(2025, 11, 5), 
            CategoryId = 2, 
            Type = TransactionType.Expense,
            Description = "Utgifter November"
        });
        _context.SaveChanges();

        // Create a fresh ViewModel to trigger CalculateAccountBalance
        var transactionService = new TransactionService(_context);
        var calculationService = new CalculationService(_context);
        var viewModel = new OverviewViewModel(transactionService, calculationService);

        // Act - Account balance should be calculated on initialization

        // Assert - Total income (70000) - Total expenses (27000) = 43000
        Assert.Equal(43000m, viewModel.AccountBalance);
    }

    [Fact]
    public void AccountBalance_RemainsConstant_WhenMonthSelectionChanges()
    {
        // Arrange
        var incomeCategory = new Category { Id = 1, Name = "Lön", Type = TransactionType.Income };
        var expenseCategory = new Category { Id = 2, Name = "Mat", Type = TransactionType.Expense };
        _context.Categories.AddRange(incomeCategory, expenseCategory);
        
        _context.Transactions.Add(new Transaction 
        { 
            Amount = 35000m, 
            Date = new DateTime(2025, 10, 25), 
            CategoryId = 1, 
            Type = TransactionType.Income,
            Description = "Lön Oktober"
        });
        _context.Transactions.Add(new Transaction 
        { 
            Amount = 35000m, 
            Date = new DateTime(2025, 11, 25), 
            CategoryId = 1, 
            Type = TransactionType.Income,
            Description = "Lön November"
        });
        _context.Transactions.Add(new Transaction 
        { 
            Amount = 15000m, 
            Date = new DateTime(2025, 10, 5), 
            CategoryId = 2, 
            Type = TransactionType.Expense,
            Description = "Utgifter Oktober"
        });
        _context.SaveChanges();

        // Create a fresh ViewModel
        var transactionService = new TransactionService(_context);
        var calculationService = new CalculationService(_context);
        var viewModel = new OverviewViewModel(transactionService, calculationService);

        var initialBalance = viewModel.AccountBalance;

        // Act - Change selected month
        viewModel.SelectedMonth = new DateTime(2025, 10, 1);

        // Assert - Account balance should remain the same
        Assert.Equal(initialBalance, viewModel.AccountBalance);
        Assert.Equal(55000m, viewModel.AccountBalance); // 70000 - 15000
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
