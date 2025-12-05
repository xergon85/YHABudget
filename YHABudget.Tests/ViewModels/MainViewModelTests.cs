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
    private readonly ISalarySettingsService _salarySettingsService;
    private readonly IAbsenceService _absenceService;
    private readonly Mock<IDialogService> _mockDialogService;
    private readonly OverviewViewModel _overviewViewModel;
    private readonly TransactionViewModel _transactionViewModel;
    private readonly RecurringTransactionViewModel _recurringTransactionViewModel;
    private readonly SalaryViewModel _salaryViewModel;
    private readonly AbsenceViewModel _absenceViewModel;

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
        _salarySettingsService = new SalarySettingsService(_context);
        _absenceService = new AbsenceService(_context, _salarySettingsService);
        _mockDialogService = new Mock<IDialogService>();

        // Create child ViewModels
        _overviewViewModel = new OverviewViewModel(_recurringTransactionService, _calculationService);
        _transactionViewModel = new TransactionViewModel(_transactionService, _categoryService, _recurringTransactionService, _mockDialogService.Object);
        _recurringTransactionViewModel = new RecurringTransactionViewModel(_recurringTransactionService, _mockDialogService.Object);
        _salaryViewModel = new SalaryViewModel(_salarySettingsService, _mockDialogService.Object, _recurringTransactionService, _categoryService);
        _absenceViewModel = new AbsenceViewModel(_absenceService, _mockDialogService.Object, _salarySettingsService);
    }

    [Fact]
    public void Constructor_InitializesWithOverviewAsCurrentViewModel()
    {
        // Arrange & Act
        var mainViewModel = new MainViewModel(_recurringTransactionService, _overviewViewModel, _transactionViewModel, _recurringTransactionViewModel, _salaryViewModel, _absenceViewModel);

        // Assert
        Assert.NotNull(mainViewModel.CurrentViewModel);
        Assert.IsType<OverviewViewModel>(mainViewModel.CurrentViewModel);
    }

    [Fact]
    public void NavigateToOverviewCommand_ChangesCurrentViewModelToOverview()
    {
        // Arrange
        var mainViewModel = new MainViewModel(_recurringTransactionService, _overviewViewModel, _transactionViewModel, _recurringTransactionViewModel, _salaryViewModel, _absenceViewModel);
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
        var mainViewModel = new MainViewModel(_recurringTransactionService, _overviewViewModel, _transactionViewModel, _recurringTransactionViewModel, _salaryViewModel, _absenceViewModel);

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
        var mainViewModel = new MainViewModel(_recurringTransactionService, _overviewViewModel, _transactionViewModel, _recurringTransactionViewModel, _salaryViewModel, _absenceViewModel);

        // Act
        mainViewModel.NavigateToRecurringCommand.Execute(null);

        // Assert
        Assert.NotNull(mainViewModel.CurrentViewModel);
        Assert.IsType<RecurringTransactionViewModel>(mainViewModel.CurrentViewModel);
    }

    [Fact]
    public void NavigateToSalaryCommand_ChangesCurrentViewModelToSalary()
    {
        // Arrange
        var mainViewModel = new MainViewModel(_recurringTransactionService, _overviewViewModel, _transactionViewModel, _recurringTransactionViewModel, _salaryViewModel, _absenceViewModel);

        // Act
        mainViewModel.NavigateToSalaryCommand.Execute(null);

        // Assert
        Assert.NotNull(mainViewModel.CurrentViewModel);
        Assert.IsType<SalaryViewModel>(mainViewModel.CurrentViewModel);
    }

    [Fact]
    public void CurrentViewModel_WhenChanged_RaisesPropertyChangedEvent()
    {
        // Arrange
        var mainViewModel = new MainViewModel(_recurringTransactionService, _overviewViewModel, _transactionViewModel, _recurringTransactionViewModel, _salaryViewModel, _absenceViewModel);
        bool propertyChangedRaised = false;
        mainViewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(MainViewModel.CurrentViewModel))
                propertyChangedRaised = true;
        };

        // Act
        mainViewModel.CurrentViewModel = new OverviewViewModel(_recurringTransactionService, _calculationService);

        // Assert
        Assert.True(propertyChangedRaised);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
