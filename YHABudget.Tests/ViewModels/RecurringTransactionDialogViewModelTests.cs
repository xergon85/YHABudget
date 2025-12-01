using YHABudget.Core.ViewModels;
using YHABudget.Data.Context;
using YHABudget.Data.Enums;
using YHABudget.Data.Models;
using YHABudget.Data.Services;
using Microsoft.EntityFrameworkCore;

namespace YHABudget.Tests.ViewModels;

public class RecurringTransactionDialogViewModelTests : IDisposable
{
    private readonly BudgetDbContext _context;
    private readonly ICategoryService _categoryService;
    private readonly IRecurringTransactionService _recurringTransactionService;

    public RecurringTransactionDialogViewModelTests()
    {
        var options = new DbContextOptionsBuilder<BudgetDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BudgetDbContext(options);
        _context.Database.EnsureCreated();

        _categoryService = new CategoryService(_context);
        _recurringTransactionService = new RecurringTransactionService(_context);
    }

    [Fact]
    public void Constructor_LoadsCategoriesForDefaultExpenseType()
    {
        // Arrange & Act
        var viewModel = new RecurringTransactionDialogViewModel(_categoryService, _recurringTransactionService);

        // Assert
        Assert.NotEmpty(viewModel.AvailableCategories);
        Assert.All(viewModel.AvailableCategories, c => Assert.Equal(TransactionType.Expense, c.Type));
    }

    [Fact]
    public void Constructor_SetsDefaultValues()
    {
        // Arrange & Act
        var viewModel = new RecurringTransactionDialogViewModel(_categoryService, _recurringTransactionService);

        // Assert
        Assert.Equal(TransactionType.Expense, viewModel.TransactionType);
        Assert.Equal(RecurrenceType.Monthly, viewModel.RecurrenceType);
        Assert.True(viewModel.IsActive);
        Assert.Null(viewModel.Amount);
        Assert.Null(viewModel.RecurrenceMonth);
        Assert.Null(viewModel.EndDate);
    }

    [Fact]
    public void Amount_WhenSetToZero_SetsErrorMessage()
    {
        // Arrange
        var viewModel = new RecurringTransactionDialogViewModel(_categoryService, _recurringTransactionService);
        viewModel.Amount = 100;

        // Act
        viewModel.Amount = 0;

        // Assert
        Assert.Equal("Belopp måste vara större än 0", viewModel.ErrorMessage);
    }

    [Fact]
    public void Amount_WhenSetToNull_SetsErrorMessage()
    {
        // Arrange
        var viewModel = new RecurringTransactionDialogViewModel(_categoryService, _recurringTransactionService);
        viewModel.Amount = 100;

        // Act
        viewModel.Amount = null;

        // Assert
        Assert.Equal("Belopp måste vara större än 0", viewModel.ErrorMessage);
    }

    [Fact]
    public void Amount_WhenSetToValidValue_ClearsErrorMessage()
    {
        // Arrange
        var viewModel = new RecurringTransactionDialogViewModel(_categoryService, _recurringTransactionService);
        viewModel.Amount = 0; // Sets error

        // Act
        viewModel.Amount = 100;

        // Assert
        Assert.Empty(viewModel.ErrorMessage);
    }

    [Fact]
    public void Description_WhenEmpty_SetsErrorMessage()
    {
        // Arrange
        var viewModel = new RecurringTransactionDialogViewModel(_categoryService, _recurringTransactionService);
        viewModel.Description = "Valid";

        // Act
        viewModel.Description = "";

        // Assert
        Assert.Equal("Beskrivning måste anges", viewModel.ErrorMessage);
    }

    [Fact]
    public void Description_WhenValid_ClearsErrorMessage()
    {
        // Arrange
        var viewModel = new RecurringTransactionDialogViewModel(_categoryService, _recurringTransactionService);
        viewModel.Description = ""; // Sets error

        // Act
        viewModel.Description = "Valid Description";

        // Assert
        Assert.Empty(viewModel.ErrorMessage);
    }

    [Fact]
    public void SelectedCategoryId_WhenNull_SetsErrorMessage()
    {
        // Arrange
        var viewModel = new RecurringTransactionDialogViewModel(_categoryService, _recurringTransactionService);
        viewModel.SelectedCategoryId = 1;

        // Act
        viewModel.SelectedCategoryId = null;

        // Assert
        Assert.Equal("Kategori måste väljas", viewModel.ErrorMessage);
    }

    [Fact]
    public void SelectedCategoryId_WhenValid_ClearsErrorMessage()
    {
        // Arrange
        var viewModel = new RecurringTransactionDialogViewModel(_categoryService, _recurringTransactionService);
        viewModel.SelectedCategoryId = null; // Sets error

        // Act
        viewModel.SelectedCategoryId = 1;

        // Assert
        Assert.Empty(viewModel.ErrorMessage);
    }

    [Fact]
    public void RecurrenceType_WhenSetToYearlyWithoutMonth_SetsErrorMessage()
    {
        // Arrange
        var viewModel = new RecurringTransactionDialogViewModel(_categoryService, _recurringTransactionService);
        viewModel.RecurrenceType = RecurrenceType.Monthly; // Default
        viewModel.RecurrenceMonth = null;

        // Act
        viewModel.RecurrenceType = RecurrenceType.Yearly;

        // Assert
        Assert.Equal("Månad måste väljas för årlig återkommande transaktion", viewModel.ErrorMessage);
    }

    [Fact]
    public void RecurrenceType_WhenSetToYearlyWithMonth_ClearsErrorMessage()
    {
        // Arrange
        var viewModel = new RecurringTransactionDialogViewModel(_categoryService, _recurringTransactionService);
        viewModel.RecurrenceMonth = 3; // March
        viewModel.RecurrenceType = RecurrenceType.Yearly;

        // Act - already set in arrange, validate it doesn't have error

        // Assert
        Assert.Empty(viewModel.ErrorMessage);
    }

    [Fact]
    public void RecurrenceType_WhenSetToMonthly_ClearsErrorMessage()
    {
        // Arrange
        var viewModel = new RecurringTransactionDialogViewModel(_categoryService, _recurringTransactionService);
        viewModel.RecurrenceType = RecurrenceType.Yearly;
        viewModel.RecurrenceMonth = null; // Sets error

        // Act
        viewModel.RecurrenceType = RecurrenceType.Monthly;

        // Assert
        Assert.Empty(viewModel.ErrorMessage);
    }

    [Fact]
    public void RecurrenceMonth_WhenSetForYearlyRecurrence_ClearsErrorMessage()
    {
        // Arrange
        var viewModel = new RecurringTransactionDialogViewModel(_categoryService, _recurringTransactionService);
        viewModel.RecurrenceType = RecurrenceType.Yearly;
        viewModel.RecurrenceMonth = null; // Sets error

        // Act
        viewModel.RecurrenceMonth = 6; // June

        // Assert
        Assert.Empty(viewModel.ErrorMessage);
    }

    [Fact]
    public void TransactionType_WhenChanged_ReloadsCategoriesForNewType()
    {
        // Arrange
        var viewModel = new RecurringTransactionDialogViewModel(_categoryService, _recurringTransactionService);
        var initialCategories = viewModel.AvailableCategories.ToList();

        // Act
        viewModel.TransactionType = TransactionType.Income;

        // Assert
        Assert.NotEqual(initialCategories, viewModel.AvailableCategories);
        Assert.All(viewModel.AvailableCategories, c => Assert.Equal(TransactionType.Income, c.Type));
    }

    [Fact]
    public void SaveCommand_CannotExecute_WhenAmountIsInvalid()
    {
        // Arrange
        var viewModel = new RecurringTransactionDialogViewModel(_categoryService, _recurringTransactionService)
        {
            Amount = 0, // Invalid
            Description = "Test",
            SelectedCategoryId = 1,
            RecurrenceType = RecurrenceType.Monthly
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
        var viewModel = new RecurringTransactionDialogViewModel(_categoryService, _recurringTransactionService)
        {
            Amount = 100,
            Description = "", // Invalid
            SelectedCategoryId = 1,
            RecurrenceType = RecurrenceType.Monthly
        };

        // Act
        var canExecute = viewModel.SaveCommand.CanExecute(null);

        // Assert
        Assert.False(canExecute);
    }

    [Fact]
    public void SaveCommand_CannotExecute_WhenCategoryIsNotSelected()
    {
        // Arrange
        var viewModel = new RecurringTransactionDialogViewModel(_categoryService, _recurringTransactionService)
        {
            Amount = 100,
            Description = "Test",
            SelectedCategoryId = null, // Invalid
            RecurrenceType = RecurrenceType.Monthly
        };

        // Act
        var canExecute = viewModel.SaveCommand.CanExecute(null);

        // Assert
        Assert.False(canExecute);
    }

    [Fact]
    public void SaveCommand_CannotExecute_WhenYearlyRecurrenceWithoutMonth()
    {
        // Arrange
        var viewModel = new RecurringTransactionDialogViewModel(_categoryService, _recurringTransactionService)
        {
            Amount = 100,
            Description = "Test",
            SelectedCategoryId = 1,
            RecurrenceType = RecurrenceType.Yearly,
            RecurrenceMonth = null // Invalid for Yearly
        };

        // Act
        var canExecute = viewModel.SaveCommand.CanExecute(null);

        // Assert
        Assert.False(canExecute);
    }

    [Fact]
    public void SaveCommand_CanExecute_WhenAllFieldsAreValidForMonthly()
    {
        // Arrange
        var viewModel = new RecurringTransactionDialogViewModel(_categoryService, _recurringTransactionService)
        {
            Amount = 100,
            Description = "Monthly subscription",
            SelectedCategoryId = 1,
            RecurrenceType = RecurrenceType.Monthly
        };

        // Act
        var canExecute = viewModel.SaveCommand.CanExecute(null);

        // Assert
        Assert.True(canExecute);
    }

    [Fact]
    public void SaveCommand_CanExecute_WhenAllFieldsAreValidForYearly()
    {
        // Arrange
        var viewModel = new RecurringTransactionDialogViewModel(_categoryService, _recurringTransactionService)
        {
            Amount = 8000,
            Description = "Car insurance",
            SelectedCategoryId = 1,
            RecurrenceType = RecurrenceType.Yearly,
            RecurrenceMonth = 3 // March
        };

        // Act
        var canExecute = viewModel.SaveCommand.CanExecute(null);

        // Assert
        Assert.True(canExecute);
    }

    [Fact]
    public void SaveCommand_AddsNewMonthlyRecurringTransaction()
    {
        // Arrange
        var viewModel = new RecurringTransactionDialogViewModel(_categoryService, _recurringTransactionService)
        {
            Amount = 149,
            Description = "Netflix",
            SelectedCategoryId = 1,
            RecurrenceType = RecurrenceType.Monthly,
            StartDate = DateTime.Now,
            IsActive = true
        };

        var initialCount = _recurringTransactionService.GetAllRecurringTransactions().Count();
        var requestCloseCalled = false;
        viewModel.RequestClose += (s, e) => requestCloseCalled = true;

        // Act
        viewModel.SaveCommand.Execute(null);

        // Assert
        var transactions = _recurringTransactionService.GetAllRecurringTransactions();
        Assert.Equal(initialCount + 1, transactions.Count());
        Assert.True(requestCloseCalled);
        Assert.True(viewModel.SaveSuccessful);

        var added = transactions.Last();
        Assert.Equal(149, added.Amount);
        Assert.Equal("Netflix", added.Description);
        Assert.Equal(RecurrenceType.Monthly, added.RecurrenceType);
        Assert.Null(added.RecurrenceMonth);
        Assert.True(added.IsActive);
    }

    [Fact]
    public void SaveCommand_AddsNewYearlyRecurringTransaction()
    {
        // Arrange
        var viewModel = new RecurringTransactionDialogViewModel(_categoryService, _recurringTransactionService)
        {
            Amount = 8000,
            Description = "Car Insurance",
            SelectedCategoryId = 1,
            RecurrenceType = RecurrenceType.Yearly,
            RecurrenceMonth = 3, // March
            StartDate = new DateTime(2025, 1, 1),
            IsActive = true
        };

        var initialCount = _recurringTransactionService.GetAllRecurringTransactions().Count();
        var requestCloseCalled = false;
        viewModel.RequestClose += (s, e) => requestCloseCalled = true;

        // Act
        viewModel.SaveCommand.Execute(null);

        // Assert
        var transactions = _recurringTransactionService.GetAllRecurringTransactions();
        Assert.Equal(initialCount + 1, transactions.Count());
        Assert.True(requestCloseCalled);
        Assert.True(viewModel.SaveSuccessful);

        var added = transactions.Last();
        Assert.Equal(8000, added.Amount);
        Assert.Equal("Car Insurance", added.Description);
        Assert.Equal(RecurrenceType.Yearly, added.RecurrenceType);
        Assert.Equal(3, added.RecurrenceMonth);
        Assert.True(added.IsActive);
    }

    [Fact]
    public void SaveCommand_UpdatesExistingRecurringTransaction()
    {
        // Arrange
        var recurringTransaction = new RecurringTransaction
        {
            Amount = 100,
            Description = "Original",
            CategoryId = 1,
            Type = TransactionType.Expense,
            RecurrenceType = RecurrenceType.Monthly,
            StartDate = DateTime.Now,
            IsActive = true
        };
        _recurringTransactionService.AddRecurringTransaction(recurringTransaction);

        var viewModel = new RecurringTransactionDialogViewModel(_categoryService, _recurringTransactionService);
        viewModel.LoadRecurringTransaction(recurringTransaction);
        viewModel.Description = "Updated Description";
        viewModel.Amount = 200;
        viewModel.RecurrenceType = RecurrenceType.Yearly;
        viewModel.RecurrenceMonth = 6; // June
        viewModel.IsActive = false;

        var requestCloseCalled = false;
        viewModel.RequestClose += (s, e) => requestCloseCalled = true;

        // Act
        viewModel.SaveCommand.Execute(null);

        // Assert
        var updated = _recurringTransactionService.GetRecurringTransactionById(recurringTransaction.Id);
        Assert.NotNull(updated);
        Assert.Equal("Updated Description", updated.Description);
        Assert.Equal(200, updated.Amount);
        Assert.Equal(RecurrenceType.Yearly, updated.RecurrenceType);
        Assert.Equal(6, updated.RecurrenceMonth);
        Assert.False(updated.IsActive);
        Assert.True(requestCloseCalled);
        Assert.True(viewModel.SaveSuccessful);
    }

    [Fact]
    public void LoadRecurringTransaction_LoadsAllFieldsCorrectly()
    {
        // Arrange
        var recurringTransaction = new RecurringTransaction
        {
            Id = 1,
            Amount = 5000,
            Description = "Rent",
            CategoryId = 2,
            Type = TransactionType.Expense,
            RecurrenceType = RecurrenceType.Monthly,
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2026, 12, 31),
            IsActive = false
        };

        var viewModel = new RecurringTransactionDialogViewModel(_categoryService, _recurringTransactionService);

        // Act
        viewModel.LoadRecurringTransaction(recurringTransaction);

        // Assert
        Assert.True(viewModel.IsEditMode);
        Assert.Equal(1, viewModel.RecurringTransactionId);
        Assert.Equal(5000, viewModel.Amount);
        Assert.Equal("Rent", viewModel.Description);
        Assert.Equal(2, viewModel.SelectedCategoryId);
        Assert.Equal(TransactionType.Expense, viewModel.TransactionType);
        Assert.Equal(RecurrenceType.Monthly, viewModel.RecurrenceType);
        Assert.Null(viewModel.RecurrenceMonth);
        Assert.Equal(new DateTime(2025, 1, 1), viewModel.StartDate);
        Assert.Equal(new DateTime(2026, 12, 31), viewModel.EndDate);
        Assert.False(viewModel.IsActive);
    }

    [Fact]
    public void CancelCommand_RaisesRequestCloseEvent()
    {
        // Arrange
        var viewModel = new RecurringTransactionDialogViewModel(_categoryService, _recurringTransactionService);
        var requestCloseCalled = false;
        viewModel.RequestClose += (s, e) => requestCloseCalled = true;

        // Act
        viewModel.CancelCommand.Execute(null);

        // Assert
        Assert.True(requestCloseCalled);
    }

    [Fact]
    public void CancelCommand_SetsSaveSuccessfulToFalse()
    {
        // Arrange
        var viewModel = new RecurringTransactionDialogViewModel(_categoryService, _recurringTransactionService);

        // Act
        viewModel.CancelCommand.Execute(null);

        // Assert
        Assert.False(viewModel.SaveSuccessful);
    }

    [Fact]
    public void SaveCommand_DoesNotRaiseRequestClose_OnValidationFailure()
    {
        // Arrange
        var viewModel = new RecurringTransactionDialogViewModel(_categoryService, _recurringTransactionService)
        {
            Amount = 0, // Invalid
            Description = "Test",
            SelectedCategoryId = 1
        };
        var requestCloseCalled = false;
        viewModel.RequestClose += (s, e) => requestCloseCalled = true;

        // Act
        viewModel.SaveCommand.Execute(null);

        // Assert
        Assert.False(requestCloseCalled);
        Assert.False(viewModel.SaveSuccessful);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
