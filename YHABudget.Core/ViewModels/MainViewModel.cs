using System.Windows.Input;
using YHABudget.Core.Commands;
using YHABudget.Core.MVVM;
using YHABudget.Core.Services;
using YHABudget.Data.Services;

namespace YHABudget.Core.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly ITransactionService _transactionService;
    private readonly ICategoryService _categoryService;
    private readonly ICalculationService _calculationService;
    private readonly IRecurringTransactionService _recurringTransactionService;
    private readonly IDialogService _dialogService;
    private ViewModelBase? _currentViewModel;

    private OverviewViewModel? _overviewViewModel;
    private TransactionViewModel? _transactionViewModel;
    private RecurringTransactionViewModel? _recurringTransactionViewModel;
    private SettingsViewModel? _settingsViewModel;

    public MainViewModel(
        ITransactionService transactionService,
        ICategoryService categoryService,
        ICalculationService calculationService,
        IRecurringTransactionService recurringTransactionService,
        IDialogService dialogService)
    {
        _transactionService = transactionService;
        _categoryService = categoryService;
        _calculationService = calculationService;
        _recurringTransactionService = recurringTransactionService;
        _dialogService = dialogService;

        NavigateToOverviewCommand = new RelayCommand(() =>
        {
            var vm = GetOverviewViewModel();
            vm.LoadDataCommand.Execute(null);
            CurrentViewModel = vm;
        });
        NavigateToTransactionsCommand = new RelayCommand(() =>
        {
            var vm = GetTransactionViewModel();
            vm.LoadDataCommand.Execute(null);
            CurrentViewModel = vm;
        });
        NavigateToRecurringCommand = new RelayCommand(() =>
        {
            var vm = GetRecurringTransactionViewModel();
            vm.LoadDataCommand.Execute(null);
            CurrentViewModel = vm;
        });
        NavigateToSettingsCommand = new RelayCommand(() => CurrentViewModel = GetSettingsViewModel());

        // Process recurring transactions for current month on startup
        _recurringTransactionService.ProcessRecurringTransactionsForMonth(DateTime.Now);

        // Start with Overview
        CurrentViewModel = GetOverviewViewModel();
    }
    public ViewModelBase? CurrentViewModel
    {
        get => _currentViewModel;
        set => SetProperty(ref _currentViewModel, value);
    }

    public ICommand NavigateToOverviewCommand { get; }
    public ICommand NavigateToTransactionsCommand { get; }
    public ICommand NavigateToRecurringCommand { get; }
    public ICommand NavigateToSettingsCommand { get; }

    private OverviewViewModel GetOverviewViewModel()
    {
        return _overviewViewModel ??= new OverviewViewModel(_transactionService, _recurringTransactionService, _calculationService);
    }

    private TransactionViewModel GetTransactionViewModel()
    {
        return _transactionViewModel ??= new TransactionViewModel(_transactionService, _categoryService, _recurringTransactionService, _dialogService);
    }

    private RecurringTransactionViewModel GetRecurringTransactionViewModel()
    {
        return _recurringTransactionViewModel ??= new RecurringTransactionViewModel(_recurringTransactionService, _dialogService);
    }

    private SettingsViewModel GetSettingsViewModel()
    {
        return _settingsViewModel ??= new SettingsViewModel();
    }
}
