using Microsoft.EntityFrameworkCore;
using YHABudget.Core.Services;
using YHABudget.Core.ViewModels;
using YHABudget.Data.Context;
using YHABudget.Data.Enums;
using YHABudget.Data.Models;
using YHABudget.Data.Services;
using Xunit;

namespace YHABudget.Tests.ViewModels;

// Mock DialogService for testing
public class MockDialogService : IDialogService
{
    public bool? ShowTransactionDialog(Transaction? transaction = null) => null;
    public bool? ShowRecurringTransactionDialog(RecurringTransaction? recurringTransaction = null) => null;
}

public class RecurringTransactionViewModelTests : IDisposable
{
    private readonly BudgetDbContext _context;
    private readonly RecurringTransactionService _recurringTransactionService;
    private readonly IDialogService _dialogService;

    public RecurringTransactionViewModelTests()
    {
        var options = new DbContextOptionsBuilder<BudgetDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BudgetDbContext(options);
        _context.Database.EnsureCreated();

        _recurringTransactionService = new RecurringTransactionService(_context);
        _dialogService = new MockDialogService();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public void Constructor_InitializesCollectionsAndCommands()
    {
        // Act
        var viewModel = new RecurringTransactionViewModel(_recurringTransactionService, _dialogService);

        // Assert
        Assert.NotNull(viewModel.RecurringTransactions);
        Assert.NotNull(viewModel.LoadDataCommand);
        Assert.NotNull(viewModel.AddRecurringTransactionCommand);
        Assert.NotNull(viewModel.EditRecurringTransactionCommand);
        Assert.NotNull(viewModel.DeleteRecurringTransactionCommand);
        Assert.NotNull(viewModel.ToggleActiveCommand);
    }

    [Fact]
    public void LoadData_LoadsRecurringTransactions()
    {
        // Arrange
        var category = new Category { Name = "Test Category", Type = TransactionType.Expense };
        _context.Categories.Add(category);
        _context.SaveChanges();

        var recurring = new RecurringTransaction
        {
            Description = "Monthly Rent",
            Amount = 1000,
            CategoryId = category.Id,
            Type = TransactionType.Expense,
            RecurrenceType = RecurrenceType.Monthly,
            StartDate = DateTime.Now,
            IsActive = true
        };
        _recurringTransactionService.AddRecurringTransaction(recurring);

        // Act
        var viewModel = new RecurringTransactionViewModel(_recurringTransactionService, _dialogService);

        // Assert
        Assert.Single(viewModel.RecurringTransactions);
        Assert.Equal("Monthly Rent", viewModel.RecurringTransactions[0].Description);
    }

    [Fact]
    public void DeleteRecurringTransactionCommand_RemovesTransaction()
    {
        // Arrange
        var category = new Category { Name = "Test Category", Type = TransactionType.Expense };
        _context.Categories.Add(category);
        _context.SaveChanges();

        var recurring = new RecurringTransaction
        {
            Description = "Monthly Rent",
            Amount = 1000,
            CategoryId = category.Id,
            Type = TransactionType.Expense,
            RecurrenceType = RecurrenceType.Monthly,
            StartDate = DateTime.Now,
            IsActive = true
        };
        _recurringTransactionService.AddRecurringTransaction(recurring);

        var viewModel = new RecurringTransactionViewModel(_recurringTransactionService, _dialogService);
        Assert.Single(viewModel.RecurringTransactions);

        // Act
        viewModel.DeleteRecurringTransactionCommand.Execute(recurring.Id);

        // Assert
        Assert.Empty(viewModel.RecurringTransactions);
    }

    [Fact]
    public void ToggleActiveCommand_TogglesIsActive()
    {
        // Arrange
        var category = new Category { Name = "Test Category", Type = TransactionType.Expense };
        _context.Categories.Add(category);
        _context.SaveChanges();

        var recurring = new RecurringTransaction
        {
            Description = "Monthly Rent",
            Amount = 1000,
            CategoryId = category.Id,
            Type = TransactionType.Expense,
            RecurrenceType = RecurrenceType.Monthly,
            StartDate = DateTime.Now,
            IsActive = true
        };
        _recurringTransactionService.AddRecurringTransaction(recurring);

        var viewModel = new RecurringTransactionViewModel(_recurringTransactionService, _dialogService);
        Assert.True(viewModel.RecurringTransactions[0].IsActive);

        // Act
        viewModel.ToggleActiveCommand.Execute(recurring.Id);

        // Assert
        Assert.False(viewModel.RecurringTransactions[0].IsActive);

        // Act again
        viewModel.ToggleActiveCommand.Execute(recurring.Id);

        // Assert
        Assert.True(viewModel.RecurringTransactions[0].IsActive);
    }

    [Fact]
    public void SelectedRecurringTransaction_UpdatesEditCommandCanExecute()
    {
        // Arrange
        var viewModel = new RecurringTransactionViewModel(_recurringTransactionService, _dialogService);

        // Initially, EditCommand should not be executable
        Assert.False(viewModel.EditRecurringTransactionCommand.CanExecute(null));

        // Act
        var category = new Category { Name = "Test Category", Type = TransactionType.Expense };
        _context.Categories.Add(category);
        _context.SaveChanges();

        var recurring = new RecurringTransaction
        {
            Description = "Monthly Rent",
            Amount = 1000,
            CategoryId = category.Id,
            Type = TransactionType.Expense,
            RecurrenceType = RecurrenceType.Monthly,
            StartDate = DateTime.Now,
            IsActive = true
        };
        _recurringTransactionService.AddRecurringTransaction(recurring);
        viewModel.LoadDataCommand.Execute(null);
        
        viewModel.SelectedRecurringTransaction = viewModel.RecurringTransactions[0];

        // Assert
        Assert.True(viewModel.EditRecurringTransactionCommand.CanExecute(null));
    }

    [Fact]
    public void RecurringTransactions_UpdatesIncrementally()
    {
        // Arrange
        var category = new Category { Name = "Test Category", Type = TransactionType.Expense };
        _context.Categories.Add(category);
        _context.SaveChanges();

        var viewModel = new RecurringTransactionViewModel(_recurringTransactionService, _dialogService);
        Assert.Empty(viewModel.RecurringTransactions);

        // Act - Add first recurring transaction
        var recurring1 = new RecurringTransaction
        {
            Description = "Monthly Rent",
            Amount = 1000,
            CategoryId = category.Id,
            Type = TransactionType.Expense,
            RecurrenceType = RecurrenceType.Monthly,
            StartDate = DateTime.Now,
            IsActive = true
        };
        _recurringTransactionService.AddRecurringTransaction(recurring1);
        viewModel.LoadDataCommand.Execute(null);

        Assert.Single(viewModel.RecurringTransactions);

        // Act - Add second recurring transaction
        var recurring2 = new RecurringTransaction
        {
            Description = "Yearly Insurance",
            Amount = 5000,
            CategoryId = category.Id,
            Type = TransactionType.Expense,
            RecurrenceType = RecurrenceType.Yearly,
            RecurrenceMonth = 6,
            StartDate = DateTime.Now,
            IsActive = true
        };
        _recurringTransactionService.AddRecurringTransaction(recurring2);
        viewModel.LoadDataCommand.Execute(null);

        // Assert
        Assert.Equal(2, viewModel.RecurringTransactions.Count);
    }
}
