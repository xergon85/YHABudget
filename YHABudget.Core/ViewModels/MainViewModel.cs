using System.Windows.Input;
using YHABudget.Core.Commands;
using YHABudget.Core.MVVM;
using YHABudget.Data.Services;

namespace YHABudget.Core.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly ITransactionService _transactionService;
    private readonly ICategoryService _categoryService;
    private readonly ICalculationService _calculationService;
    private ViewModelBase? _currentViewModel;

    public MainViewModel(
        ITransactionService transactionService, 
        ICategoryService categoryService,
        ICalculationService calculationService)
    {
        _transactionService = transactionService;
        _categoryService = categoryService;
        _calculationService = calculationService;
        
        NavigateToOverviewCommand = new RelayCommand(() => CurrentViewModel = new OverviewViewModel(_transactionService, _calculationService));
        NavigateToTransactionsCommand = new RelayCommand(() => CurrentViewModel = new TransactionViewModel(_transactionService, _categoryService));
        NavigateToRecurringCommand = new RelayCommand(() => CurrentViewModel = new RecurringTransactionViewModel());
        NavigateToSettingsCommand = new RelayCommand(() => CurrentViewModel = new SettingsViewModel());

        // Start with Overview
        CurrentViewModel = new OverviewViewModel(_transactionService, _calculationService);
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
}
