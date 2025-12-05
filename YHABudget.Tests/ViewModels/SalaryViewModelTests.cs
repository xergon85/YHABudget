using Microsoft.EntityFrameworkCore;
using YHABudget.Core.ViewModels;
using YHABudget.Data.Context;
using YHABudget.Data.Models;
using YHABudget.Data.Services;

namespace YHABudget.Tests.ViewModels;

public class SalaryViewModelTests : IDisposable
{
    private readonly BudgetDbContext _context;
    private readonly SalarySettingsService _salarySettingsService;
    private readonly CalculationService _calculationService;
    private readonly SalaryViewModel _viewModel;

    public SalaryViewModelTests()
    {
        var options = new DbContextOptionsBuilder<BudgetDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BudgetDbContext(options);
        _salarySettingsService = new SalarySettingsService(_context);
        _calculationService = new CalculationService(_context);
        _viewModel = new SalaryViewModel(_salarySettingsService, _calculationService);
    }

    [Fact]
    public void Constructor_InitializesWithEmptyList()
    {
        // Assert
        Assert.NotNull(_viewModel.Salaries);
        Assert.Empty(_viewModel.Salaries);
        Assert.Null(_viewModel.SelectedSalary);
        Assert.Equal(0, _viewModel.TotalMonthlyIncome);
        Assert.False(_viewModel.IsEditing);
    }

    [Fact]
    public void LoadDataCommand_LoadsAllSalaries()
    {
        // Arrange
        _context.SalarySettings.Add(new SalarySettings
        {
            AnnualIncome = 410000,
            AnnualHours = 1920,
            Note = "Main job",
            UpdatedAt = DateTime.Now
        });
        _context.SalarySettings.Add(new SalarySettings
        {
            AnnualIncome = 100000,
            AnnualHours = 500,
            Note = "Side hustle",
            UpdatedAt = DateTime.Now
        });
        _context.SaveChanges();

        // Act
        _viewModel.LoadDataCommand.Execute(null);

        // Assert
        Assert.Equal(2, _viewModel.Salaries.Count);
        Assert.Equal("Main job", _viewModel.Salaries[0].Note);
        Assert.Equal("Side hustle", _viewModel.Salaries[1].Note);
    }

    [Fact]
    public void AddSalaryCommand_CreatesNewSalaryInEditMode()
    {
        // Act
        _viewModel.AddSalaryCommand.Execute(null);

        // Assert
        Assert.NotNull(_viewModel.SelectedSalary);
        Assert.Equal(0, _viewModel.SelectedSalary.Id);
        Assert.Equal(0, _viewModel.SelectedSalary.AnnualIncome);
        Assert.Equal(0, _viewModel.SelectedSalary.AnnualHours);
        Assert.Equal(string.Empty, _viewModel.SelectedSalary.Note);
        Assert.True(_viewModel.IsEditing);
    }

    [Fact]
    public void SaveSalaryCommand_AddsNewSalaryToDatabase()
    {
        // Arrange
        _viewModel.AddSalaryCommand.Execute(null);
        _viewModel.SelectedSalary!.AnnualIncome = 450000;
        _viewModel.SelectedSalary.AnnualHours = 2000;
        _viewModel.SelectedSalary.Note = "New job";

        // Act
        _viewModel.SaveSalaryCommand.Execute(null);

        // Assert
        Assert.Single(_viewModel.Salaries);
        Assert.Equal(450000, _viewModel.Salaries[0].AnnualIncome);
        Assert.Equal("New job", _viewModel.Salaries[0].Note);
        Assert.Null(_viewModel.SelectedSalary);
        Assert.False(_viewModel.IsEditing);

        var fromDb = _context.SalarySettings.ToList();
        Assert.Single(fromDb);
    }

    [Fact]
    public void SaveSalaryCommand_UpdatesExistingSalary()
    {
        // Arrange
        var existingSalary = new SalarySettings
        {
            AnnualIncome = 410000,
            AnnualHours = 1920,
            Note = "Old note",
            UpdatedAt = DateTime.Now
        };
        _context.SalarySettings.Add(existingSalary);
        _context.SaveChanges();
        _viewModel.LoadDataCommand.Execute(null);

        _viewModel.SelectedSalary = _viewModel.Salaries[0];
        _viewModel.SelectedSalary.AnnualIncome = 450000;
        _viewModel.SelectedSalary.Note = "Updated note";

        // Act
        _viewModel.SaveSalaryCommand.Execute(null);

        // Assert
        Assert.Single(_viewModel.Salaries);
        Assert.Equal(450000, _viewModel.Salaries[0].AnnualIncome);
        Assert.Equal("Updated note", _viewModel.Salaries[0].Note);
        Assert.Null(_viewModel.SelectedSalary);

        var fromDb = _context.SalarySettings.First();
        Assert.Equal(450000, fromDb.AnnualIncome);
        Assert.Equal("Updated note", fromDb.Note);
    }

    [Fact]
    public void DeleteSalaryCommand_RemovesSalaryFromDatabase()
    {
        // Arrange
        var salary = new SalarySettings
        {
            AnnualIncome = 400000,
            AnnualHours = 1800,
            Note = "To delete",
            UpdatedAt = DateTime.Now
        };
        _context.SalarySettings.Add(salary);
        _context.SaveChanges();
        _viewModel.LoadDataCommand.Execute(null);

        _viewModel.SelectedSalary = _viewModel.Salaries[0];

        // Act
        _viewModel.DeleteSalaryCommand.Execute(null);

        // Assert
        Assert.Empty(_viewModel.Salaries);
        Assert.Null(_viewModel.SelectedSalary);
        Assert.Empty(_context.SalarySettings.ToList());
    }

    [Fact]
    public void CancelEditCommand_ClearsSelectedSalary()
    {
        // Arrange
        _viewModel.AddSalaryCommand.Execute(null);
        Assert.NotNull(_viewModel.SelectedSalary);

        // Act
        _viewModel.CancelEditCommand.Execute(null);

        // Assert
        Assert.Null(_viewModel.SelectedSalary);
        Assert.False(_viewModel.IsEditing);
    }

    [Fact]
    public void TotalMonthlyIncome_CalculatesCorrectly_WithMultipleSalaries()
    {
        // Arrange
        _context.SalarySettings.Add(new SalarySettings
        {
            AnnualIncome = 410000,
            AnnualHours = 1920,
            Note = "Main job",
            UpdatedAt = DateTime.Now
        });
        _context.SalarySettings.Add(new SalarySettings
        {
            AnnualIncome = 100000,
            AnnualHours = 2000,
            Note = "Side hustle",
            UpdatedAt = DateTime.Now
        });
        _context.SaveChanges();

        // Act
        _viewModel.LoadDataCommand.Execute(null);

        // Assert
        // Calculate expected: (410000/1920 * 160) + (100000/2000 * 160)
        var expected1 = _calculationService.CalculateMonthlyIncome(410000, 1920);
        var expected2 = _calculationService.CalculateMonthlyIncome(100000, 2000);
        var expectedTotal = expected1 + expected2;

        Assert.Equal(expectedTotal, _viewModel.TotalMonthlyIncome);
    }

    [Fact]
    public void TotalMonthlyIncome_UpdatesAfterSave()
    {
        // Arrange
        Assert.Equal(0, _viewModel.TotalMonthlyIncome);

        _viewModel.AddSalaryCommand.Execute(null);
        _viewModel.SelectedSalary!.AnnualIncome = 410000;
        _viewModel.SelectedSalary.AnnualHours = 1920;
        _viewModel.SelectedSalary.Note = "Test";

        // Act
        _viewModel.SaveSalaryCommand.Execute(null);

        // Assert
        var expected = _calculationService.CalculateMonthlyIncome(410000, 1920);
        Assert.Equal(expected, _viewModel.TotalMonthlyIncome);
    }

    [Fact]
    public void TotalMonthlyIncome_UpdatesAfterDelete()
    {
        // Arrange
        _context.SalarySettings.Add(new SalarySettings
        {
            AnnualIncome = 410000,
            AnnualHours = 1920,
            Note = "Test",
            UpdatedAt = DateTime.Now
        });
        _context.SaveChanges();
        _viewModel.LoadDataCommand.Execute(null);
        Assert.True(_viewModel.TotalMonthlyIncome > 0);

        _viewModel.SelectedSalary = _viewModel.Salaries[0];

        // Act
        _viewModel.DeleteSalaryCommand.Execute(null);

        // Assert
        Assert.Equal(0, _viewModel.TotalMonthlyIncome);
    }

    [Fact]
    public void IsEditing_ReturnsTrueWhenSalarySelected()
    {
        // Arrange
        Assert.False(_viewModel.IsEditing);

        // Act
        _viewModel.AddSalaryCommand.Execute(null);

        // Assert
        Assert.True(_viewModel.IsEditing);
    }

    [Fact]
    public void SaveSalaryCommand_CanExecute_OnlyWhenSalarySelected()
    {
        // Assert - Initially false
        Assert.False(_viewModel.SaveSalaryCommand.CanExecute(null));

        // Act - Select a salary
        _viewModel.AddSalaryCommand.Execute(null);

        // Assert - Now true
        Assert.True(_viewModel.SaveSalaryCommand.CanExecute(null));

        // Act - Cancel edit
        _viewModel.CancelEditCommand.Execute(null);

        // Assert - Back to false
        Assert.False(_viewModel.SaveSalaryCommand.CanExecute(null));
    }

    [Fact]
    public void DeleteSalaryCommand_CanExecute_OnlyWhenSalarySelected()
    {
        // Arrange
        _context.SalarySettings.Add(new SalarySettings
        {
            AnnualIncome = 410000,
            AnnualHours = 1920,
            Note = "Test",
            UpdatedAt = DateTime.Now
        });
        _context.SaveChanges();
        _viewModel.LoadDataCommand.Execute(null);

        // Assert - Initially false
        Assert.False(_viewModel.DeleteSalaryCommand.CanExecute(null));

        // Act - Select a salary
        _viewModel.SelectedSalary = _viewModel.Salaries[0];

        // Assert - Now true
        Assert.True(_viewModel.DeleteSalaryCommand.CanExecute(null));
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
