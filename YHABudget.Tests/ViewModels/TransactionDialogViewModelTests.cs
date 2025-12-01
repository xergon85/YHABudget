using YHABudget.Core.ViewModels;
using YHABudget.Data.Context;
using YHABudget.Data.Enums;
using YHABudget.Data.Models;
using YHABudget.Data.Services;
using Microsoft.EntityFrameworkCore;

namespace YHABudget.Tests.ViewModels;

public class TransactionDialogViewModelTests : IDisposable
{
    private readonly BudgetDbContext _context;
    private readonly ICategoryService _categoryService;
    private readonly ITransactionService _transactionService;

    public TransactionDialogViewModelTests()
    {
        var options = new DbContextOptionsBuilder<BudgetDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BudgetDbContext(options);
        _context.Database.EnsureCreated();

        _categoryService = new CategoryService(_context);
        _transactionService = new TransactionService(_context);
    }

    [Fact]
    public void Constructor_LoadsCategoriesForDefaultExpenseType()
    {
        // Arrange & Act
        var viewModel = new TransactionDialogViewModel(_categoryService, _transactionService);

        // Assert
        Assert.NotEmpty(viewModel.AvailableCategories);
        Assert.All(viewModel.AvailableCategories, c => Assert.Equal(TransactionType.Expense, c.Type));
    }

    [Fact]
    public void Amount_WhenSetToZero_SetsErrorMessage()
    {
        // Arrange
        var viewModel = new TransactionDialogViewModel(_categoryService, _transactionService);
        viewModel.Amount = 100; // Start with valid value

        // Act
        viewModel.Amount = 0;

        // Assert
        Assert.Equal("Belopp måste vara större än 0", viewModel.ErrorMessage);
    }

    [Fact]
    public void Amount_WhenSetToPositive_ClearsErrorMessage()
    {
        // Arrange
        var viewModel = new TransactionDialogViewModel(_categoryService, _transactionService);
        viewModel.Amount = 0; // Set error first

        // Act
        viewModel.Amount = 100;

        // Assert
        Assert.Empty(viewModel.ErrorMessage);
    }

    [Fact]
    public void Description_WhenEmpty_SetsErrorMessage()
    {
        // Arrange
        var viewModel = new TransactionDialogViewModel(_categoryService, _transactionService);
        viewModel.Description = "Something"; // Start with valid value

        // Act
        viewModel.Description = "";

        // Assert
        Assert.Equal("Beskrivning måste anges", viewModel.ErrorMessage);
    }

    [Fact]
    public void Description_WhenNotEmpty_ClearsErrorMessage()
    {
        // Arrange
        var viewModel = new TransactionDialogViewModel(_categoryService, _transactionService);
        viewModel.Description = ""; // Set error first

        // Act
        viewModel.Description = "Test transaction";

        // Assert
        Assert.Empty(viewModel.ErrorMessage);
    }

    [Fact]
    public void SelectedCategoryId_WhenNull_SetsErrorMessage()
    {
        // Arrange
        var viewModel = new TransactionDialogViewModel(_categoryService, _transactionService);
        viewModel.SelectedCategoryId = 1; // Start with valid value

        // Act
        viewModel.SelectedCategoryId = null;

        // Assert
        Assert.Equal("Kategori måste väljas", viewModel.ErrorMessage);
    }

    [Fact]
    public void TransactionType_WhenChanged_LoadsAppropriateCategories()
    {
        // Arrange
        var viewModel = new TransactionDialogViewModel(_categoryService, _transactionService);
        var expenseCount = viewModel.AvailableCategories.Count;

        // Act
        viewModel.TransactionType = TransactionType.Income;

        // Assert
        Assert.NotEmpty(viewModel.AvailableCategories);
        Assert.All(viewModel.AvailableCategories, c => Assert.Equal(TransactionType.Income, c.Type));
        Assert.NotEqual(expenseCount, viewModel.AvailableCategories.Count);
    }

    [Fact]
    public void LoadTransaction_PopulatesAllFields()
    {
        // Arrange
        var viewModel = new TransactionDialogViewModel(_categoryService, _transactionService);
        var transaction = new Transaction
        {
            Id = 1,
            Amount = 500,
            Description = "Test",
            Date = new DateTime(2025, 11, 15),
            CategoryId = 1,
            Type = TransactionType.Expense
        };

        // Act
        viewModel.LoadTransaction(transaction);

        // Assert
        Assert.True(viewModel.IsEditMode);
        Assert.Equal(1, viewModel.TransactionId);
        Assert.Equal(500, viewModel.Amount);
        Assert.Equal("Test", viewModel.Description);
        Assert.Equal(new DateTime(2025, 11, 15), viewModel.Date);
        Assert.Equal(1, viewModel.SelectedCategoryId);
        Assert.Equal(TransactionType.Expense, viewModel.TransactionType);
    }

    [Fact]
    public void SaveCommand_CannotExecute_WhenAmountIsZero()
    {
        // Arrange
        var viewModel = new TransactionDialogViewModel(_categoryService, _transactionService)
        {
            Amount = 0,
            Description = "Test",
            SelectedCategoryId = 1
        };

        // Act
        var canExecute = viewModel.SaveCommand.CanExecute(null);

        // Assert
        Assert.False(canExecute);
    }

    [Fact]
    public void SaveCommand_CannotExecute_WhenDescriptionIsEmpty()
    {
        // Arrange
        var viewModel = new TransactionDialogViewModel(_categoryService, _transactionService)
        {
            Amount = 100,
            Description = "",
            SelectedCategoryId = 1
        };

        // Act
        var canExecute = viewModel.SaveCommand.CanExecute(null);

        // Assert
        Assert.False(canExecute);
    }

    [Fact]
    public void SaveCommand_CannotExecute_WhenCategoryNotSelected()
    {
        // Arrange
        var viewModel = new TransactionDialogViewModel(_categoryService, _transactionService)
        {
            Amount = 100,
            Description = "Test",
            SelectedCategoryId = null
        };

        // Act
        var canExecute = viewModel.SaveCommand.CanExecute(null);

        // Assert
        Assert.False(canExecute);
    }

    [Fact]
    public void SaveCommand_CanExecute_WhenAllFieldsValid()
    {
        // Arrange
        var viewModel = new TransactionDialogViewModel(_categoryService, _transactionService)
        {
            Amount = 100,
            Description = "Test",
            SelectedCategoryId = 1
        };

        // Act
        var canExecute = viewModel.SaveCommand.CanExecute(null);

        // Assert
        Assert.True(canExecute);
    }

    [Fact]
    public void SaveCommand_AddsNewTransaction_WhenNotInEditMode()
    {
        // Arrange
        var viewModel = new TransactionDialogViewModel(_categoryService, _transactionService)
        {
            Amount = 100,
            Description = "New Transaction",
            Date = DateTime.Now,
            SelectedCategoryId = 1,
            TransactionType = TransactionType.Expense
        };

        var initialCount = _transactionService.GetAllTransactions().Count();

        // Act
        viewModel.SaveCommand.Execute(null);

        // Assert
        var transactions = _transactionService.GetAllTransactions();
        Assert.Equal(initialCount + 1, transactions.Count());
        Assert.True(viewModel.DialogResult);
    }

    [Fact]
    public void SaveCommand_UpdatesExistingTransaction_WhenInEditMode()
    {
        // Arrange
        var transaction = new Transaction
        {
            Amount = 100,
            Description = "Original",
            Date = DateTime.Now,
            CategoryId = 1,
            Type = TransactionType.Expense,
            IsRecurring = false
        };
        _transactionService.AddTransaction(transaction);

        var viewModel = new TransactionDialogViewModel(_categoryService, _transactionService);
        viewModel.LoadTransaction(transaction);
        viewModel.Description = "Updated";
        viewModel.Amount = 200;

        // Act
        viewModel.SaveCommand.Execute(null);

        // Assert
        var updated = _transactionService.GetTransactionById(transaction.Id);
        Assert.NotNull(updated);
        Assert.Equal("Updated", updated.Description);
        Assert.Equal(200, updated.Amount);
        Assert.True(viewModel.DialogResult);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
