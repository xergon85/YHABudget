using Microsoft.EntityFrameworkCore;
using YHABudget.Data.Context;
using YHABudget.Data.Models;
using YHABudget.Data.Services;

namespace YHABudget.Tests.Services;

public class SalarySettingsServiceTests : IDisposable
{
    private readonly BudgetDbContext _context;
    private readonly SalarySettingsService _service;

    public SalarySettingsServiceTests()
    {
        var options = new DbContextOptionsBuilder<BudgetDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BudgetDbContext(options);
        _service = new SalarySettingsService(_context);
    }

    [Fact]
    public void GetAllSettings_ReturnsEmptyList_WhenNoneExist()
    {
        // Act
        var settings = _service.GetAllSettings();

        // Assert
        Assert.NotNull(settings);
        Assert.Empty(settings);
    }

    [Fact]
    public void GetAllSettings_ReturnsAllSettings_WhenMultipleExist()
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
        var settings = _service.GetAllSettings();

        // Assert
        Assert.Equal(2, settings.Count);
        Assert.Equal("Main job", settings[0].Note);
        Assert.Equal("Side hustle", settings[1].Note);
    }

    [Fact]
    public void GetSettingsById_ReturnsNull_WhenNotFound()
    {
        // Act
        var settings = _service.GetSettingsById(999);

        // Assert
        Assert.Null(settings);
    }

    [Fact]
    public void GetSettingsById_ReturnsSettings_WhenFound()
    {
        // Arrange
        var newSettings = new SalarySettings
        {
            AnnualIncome = 410000,
            AnnualHours = 1920,
            Note = "Test salary",
            UpdatedAt = DateTime.Now
        };
        _context.SalarySettings.Add(newSettings);
        _context.SaveChanges();

        // Act
        var settings = _service.GetSettingsById(newSettings.Id);

        // Assert
        Assert.NotNull(settings);
        Assert.Equal(410000, settings.AnnualIncome);
        Assert.Equal("Test salary", settings.Note);
    }

    [Fact]
    public void AddSettings_CreatesNewEntry()
    {
        // Arrange
        var newSettings = new SalarySettings
        {
            AnnualIncome = 450000,
            AnnualHours = 2000,
            Note = "New salary",
            UpdatedAt = DateTime.Now
        };

        // Act
        var result = _service.AddSettings(newSettings);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal(450000, result.AnnualIncome);
        Assert.True((DateTime.Now - result.UpdatedAt).TotalSeconds < 5);

        var fromDb = _context.SalarySettings.Find(result.Id);
        Assert.NotNull(fromDb);
        Assert.Equal("New salary", fromDb.Note);
    }

    [Fact]
    public void UpdateSettings_UpdatesExistingEntry()
    {
        // Arrange
        var existingSettings = new SalarySettings
        {
            AnnualIncome = 410000,
            AnnualHours = 1920,
            Note = "Old note",
            UpdatedAt = DateTime.Now.AddDays(-1)
        };
        _context.SalarySettings.Add(existingSettings);
        _context.SaveChanges();

        existingSettings.AnnualIncome = 450000;
        existingSettings.Note = "Updated note";

        // Act
        _service.UpdateSettings(existingSettings);

        // Assert
        var result = _context.SalarySettings.Find(existingSettings.Id);
        Assert.NotNull(result);
        Assert.Equal(450000, result.AnnualIncome);
        Assert.Equal("Updated note", result.Note);
        Assert.True((DateTime.Now - result.UpdatedAt).TotalSeconds < 5);
    }

    [Fact]
    public void UpdateSettings_DoesNothing_WhenEntryNotFound()
    {
        // Arrange
        var nonExistentSettings = new SalarySettings
        {
            Id = 999,
            AnnualIncome = 500000,
            AnnualHours = 2000,
            Note = "Does not exist",
            UpdatedAt = DateTime.Now
        };

        // Act
        _service.UpdateSettings(nonExistentSettings);

        // Assert
        var result = _context.SalarySettings.Find(999);
        Assert.Null(result);
    }

    [Fact]
    public void DeleteSettings_RemovesEntry()
    {
        // Arrange
        var settings = new SalarySettings
        {
            AnnualIncome = 400000,
            AnnualHours = 1800,
            Note = "To be deleted",
            UpdatedAt = DateTime.Now
        };
        _context.SalarySettings.Add(settings);
        _context.SaveChanges();
        var id = settings.Id;

        // Act
        _service.DeleteSettings(id);

        // Assert
        var result = _context.SalarySettings.Find(id);
        Assert.Null(result);
    }

    [Fact]
    public void DeleteSettings_DoesNothing_WhenEntryNotFound()
    {
        // Act & Assert - should not throw
        _service.DeleteSettings(999);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
