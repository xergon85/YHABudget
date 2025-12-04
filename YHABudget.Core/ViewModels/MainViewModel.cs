using System.Windows.Input;
using YHABudget.Core.Commands;
using YHABudget.Core.MVVM;
using YHABudget.Data.Services;

namespace YHABudget.Core.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly IRecurringTransactionService _recurringTransactionService;
    private ViewModelBase? _currentViewModel;

    private readonly OverviewViewModel _overviewViewModel;
    private readonly TransactionViewModel _transactionViewModel;
    private readonly RecurringTransactionViewModel _recurringTransactionViewModel;
    private readonly SalaryViewModel _salaryViewModel;

    public MainViewModel(
        IRecurringTransactionService recurringTransactionService,
        OverviewViewModel overviewViewModel,
        TransactionViewModel transactionViewModel,
        RecurringTransactionViewModel recurringTransactionViewModel,
        SalaryViewModel salaryViewModel)
    {
        _recurringTransactionService = recurringTransactionService;
        _overviewViewModel = overviewViewModel;
        _transactionViewModel = transactionViewModel;
        _recurringTransactionViewModel = recurringTransactionViewModel;
        _salaryViewModel = salaryViewModel;

        NavigateToOverviewCommand = new RelayCommand(() =>
        {
            CurrentViewModel = _overviewViewModel;
            _overviewViewModel.LoadDataCommand.Execute(null);
        });
        NavigateToTransactionsCommand = new RelayCommand(() =>
        {
            CurrentViewModel = _transactionViewModel;
            _transactionViewModel.LoadDataCommand.Execute(null);
        });
        NavigateToRecurringCommand = new RelayCommand(() =>
        {
            CurrentViewModel = _recurringTransactionViewModel;
            _recurringTransactionViewModel.LoadDataCommand.Execute(null);
        });
        NavigateToSalaryCommand = new RelayCommand(() => CurrentViewModel = _salaryViewModel);

        // Process recurring transactions for current month on startup
        _recurringTransactionService.ProcessRecurringTransactionsForMonth(DateTime.Now);

        // Start with Overview
        CurrentViewModel = _overviewViewModel;
    }
    public ViewModelBase? CurrentViewModel
    {
        get => _currentViewModel;
        set => SetProperty(ref _currentViewModel, value);
    }

    public ICommand NavigateToOverviewCommand { get; }
    public ICommand NavigateToTransactionsCommand { get; }
    public ICommand NavigateToRecurringCommand { get; }
    public ICommand NavigateToSalaryCommand { get; }
}
