using YHABudget.Core.Services;
using YHABudget.Core.ViewModels;
using YHABudget.Data.Context;
using YHABudget.Data.Models;
using YHABudget.Data.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace YHABudget.Tests.ViewModels;

public class MainViewModelTests : IDisposable
{
    private readonly BudgetDbContext _context;
    private readonly ITransactionService _transactionService;
    private readonly ICategoryService _categoryService;
    private readonly ICalculationService _calculationService;
    private readonly IRecurringTransactionService _recurringTransactionService;
    private readonly Mock<IDialogService> _mockDialogService;

    public MainViewModelTests()
    {
        var options = new DbContextOptionsBuilder<BudgetDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BudgetDbContext(options);
        _transactionService = new TransactionService(_context);
        _categoryService = new CategoryService(_context);
        _calculationService = new CalculationService(_context);
        _recurringTransactionService = new RecurringTransactionService(_context);
        _mockDialogService = new Mock<IDialogService>();
    }

    [Fact]
    public void Constructor_InitializesWithOverviewAsCurrentViewModel()
    {
        // Arrange & Act
        var mainViewModel = new MainViewModel(_transactionService, _categoryService, _calculationService, _recurringTransactionService, _mockDialogService.Object);

        // Assert
        Assert.NotNull(mainViewModel.CurrentViewModel);
        Assert.IsType<OverviewViewModel>(mainViewModel.CurrentViewModel);
    }

    [Fact]
    public void NavigateToOverviewCommand_ChangesCurrentViewModelToOverview()
    {
        // Arrange
        var mainViewModel = new MainViewModel(_transactionService, _categoryService, _calculationService, _recurringTransactionService, _mockDialogService.Object);
        mainViewModel.CurrentViewModel = null; // Set to null to test navigation

        // Act
        mainViewModel.NavigateToOverviewCommand.Execute(null);

        // Assert
        Assert.NotNull(mainViewModel.CurrentViewModel);
        Assert.IsType<OverviewViewModel>(mainViewModel.CurrentViewModel);
    }

    [Fact]
    public void NavigateToRecurringTransactionsCommand_ChangesCurrentViewModelToRecurringTransactions()
    {
        // Arrange
        var mainViewModel = new MainViewModel(_transactionService, _categoryService, _calculationService, _recurringTransactionService, _mockDialogService.Object);

        // Act
        mainViewModel.NavigateToTransactionsCommand.Execute(null);

        // Assert
        Assert.NotNull(mainViewModel.CurrentViewModel);
        Assert.IsType<TransactionViewModel>(mainViewModel.CurrentViewModel);
    }

    [Fact]
    public void NavigateToRecurringCommand_ChangesCurrentViewModelToRecurring()
    {
        // Arrange
        var mainViewModel = new MainViewModel(_transactionService, _categoryService, _calculationService, _recurringTransactionService, _mockDialogService.Object);

        // Act
        mainViewModel.NavigateToRecurringCommand.Execute(null);

        // Assert
        Assert.NotNull(mainViewModel.CurrentViewModel);
        Assert.IsType<RecurringTransactionViewModel>(mainViewModel.CurrentViewModel);
    }

    [Fact]
    public void NavigateToSettingsCommand_ChangesCurrentViewModelToSettings()
    {
        // Arrange
        var mainViewModel = new MainViewModel(_transactionService, _categoryService, _calculationService, _recurringTransactionService, _mockDialogService.Object);

        // Act
        mainViewModel.NavigateToSettingsCommand.Execute(null);

        // Assert
        Assert.NotNull(mainViewModel.CurrentViewModel);
        Assert.IsType<SettingsViewModel>(mainViewModel.CurrentViewModel);
    }

    [Fact]
    public void CurrentViewModel_WhenChanged_RaisesPropertyChangedEvent()
    {
        // Arrange
        var mainViewModel = new MainViewModel(_transactionService, _categoryService, _calculationService, _recurringTransactionService, _mockDialogService.Object);
        bool propertyChangedRaised = false;
        mainViewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(MainViewModel.CurrentViewModel))
                propertyChangedRaised = true;
        };

        // Act
        mainViewModel.CurrentViewModel = new OverviewViewModel(_transactionService, _recurringTransactionService, _calculationService);

        // Assert
        Assert.True(propertyChangedRaised);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
