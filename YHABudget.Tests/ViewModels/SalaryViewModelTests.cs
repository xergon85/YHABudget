using Microsoft.EntityFrameworkCore;
using YHABudget.Core.Services;
using YHABudget.Core.ViewModels;
using YHABudget.Data.Context;
using YHABudget.Data.Enums;
using YHABudget.Data.Models;
using YHABudget.Data.Services;
using Xunit;

namespace YHABudget.Tests.ViewModels;

public class SalaryViewModelTests : IDisposable
{
    private readonly BudgetDbContext _context;
    private readonly ISalarySettingsService _salarySettingsService;
    private readonly IRecurringTransactionService _recurringTransactionService;
    private readonly ICategoryService _categoryService;
    private readonly IDialogService _dialogService;

    public SalaryViewModelTests()
    {
        var options = new DbContextOptionsBuilder<BudgetDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BudgetDbContext(options);

        // This applies seed data including categories
        _context.Database.EnsureCreated();

        _salarySettingsService = new SalarySettingsService(_context);
        _recurringTransactionService = new RecurringTransactionService(_context);
        _categoryService = new CategoryService(_context);
        _dialogService = new MockDialogService();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public void CreateRecurringTransaction_WithValidSalary_CreatesRecurringTransaction()
    {
        // Arrange
        var salary = new SalarySettings
        {
            Note = "Test Lön",
            AnnualIncome = 480000,
            AnnualHours = 1920
        };
        var addedSalary = _salarySettingsService.AddSettings(salary);

        var viewModel = new SalaryViewModel(
            _salarySettingsService,
            _dialogService,
            _recurringTransactionService,
            _categoryService);

        // Act
        viewModel.CreateRecurringTransactionCommand.Execute(addedSalary);

        // Assert
        var recurringTransactions = _recurringTransactionService.GetAllRecurringTransactions();
        var salaryTransaction = recurringTransactions.FirstOrDefault(rt => rt.Description.Contains(salary.Note));

        Assert.NotNull(salaryTransaction);
        Assert.Equal(40000m, salaryTransaction.Amount); // (480000/1920) * 160 = 40000
        Assert.Equal(RecurrenceType.Monthly, salaryTransaction.RecurrenceType);
        Assert.Equal(TransactionType.Income, salaryTransaction.Type);
        Assert.True(salaryTransaction.IsActive);
    }

    [Fact]
    public void CreateRecurringTransaction_WhenCalledTwiceForSameSalary_CreatesOnlyOneTransaction()
    {
        // Arrange
        var salary = new SalarySettings
        {
            Note = "Test Lön",
            AnnualIncome = 480000,
            AnnualHours = 1920
        };
        var addedSalary = _salarySettingsService.AddSettings(salary);

        var viewModel = new SalaryViewModel(
            _salarySettingsService,
            _dialogService,
            _recurringTransactionService,
            _categoryService);

        // Act - Try to create twice
        viewModel.CreateRecurringTransactionCommand.Execute(addedSalary);
        viewModel.CreateRecurringTransactionCommand.Execute(addedSalary);

        // Assert
        var recurringTransactions = _recurringTransactionService.GetAllRecurringTransactions();
        var salaryTransactions = recurringTransactions.Where(rt => rt.Description.Contains(salary.Note)).ToList();

        Assert.Single(salaryTransactions); // Should only have one transaction
    }

    [Fact]
    public void CreateRecurringTransaction_WithZeroHours_DoesNotCreateTransaction()
    {
        // Arrange
        var salary = new SalarySettings
        {
            Note = "Invalid Salary",
            AnnualIncome = 480000,
            AnnualHours = 0
        };
        var addedSalary = _salarySettingsService.AddSettings(salary);

        var viewModel = new SalaryViewModel(
            _salarySettingsService,
            _dialogService,
            _recurringTransactionService,
            _categoryService);

        // Act
        viewModel.CreateRecurringTransactionCommand.Execute(addedSalary);

        // Assert
        var recurringTransactions = _recurringTransactionService.GetAllRecurringTransactions();
        Assert.Empty(recurringTransactions);
    }

    [Fact]
    public void CalculateTotalMonthlyIncome_WithMultipleSalaries_ReturnsSumOfAllSalaries()
    {
        // Arrange - Clear seed data first
        var seedSalaries = _salarySettingsService.GetAllSettings().ToList();
        foreach (var s in seedSalaries)
        {
            _salarySettingsService.DeleteSettings(s.Id);
        }

        var salary1 = new SalarySettings
        {
            Note = "Main Job",
            AnnualIncome = 480000,
            AnnualHours = 1920
        };
        var salary2 = new SalarySettings
        {
            Note = "Side Job",
            AnnualIncome = 120000,
            AnnualHours = 480
        };
        _salarySettingsService.AddSettings(salary1);
        _salarySettingsService.AddSettings(salary2);

        var viewModel = new SalaryViewModel(
            _salarySettingsService,
            _dialogService,
            _recurringTransactionService,
            _categoryService);

        // Assert
        // Main job: (480000/1920) * 160 = 40000
        // Side job: (120000/480) * 160 = 40000
        // Total: 80000
        Assert.Equal(80000m, viewModel.TotalMonthlyIncome);
    }
}
