using System.Collections.ObjectModel;
using System.Windows.Input;
using YHABudget.Core.Commands;
using YHABudget.Core.DTOs;
using YHABudget.Core.MVVM;
using YHABudget.Data.Enums;
using YHABudget.Data.Services;

namespace YHABudget.Core.ViewModels;

public class OverviewViewModel : ViewModelBase
{
    private readonly ITransactionService _transactionService;
    private readonly ICalculationService _calculationService;
    
    private DateTime _selectedMonth;
    private decimal _totalIncome;
    private decimal _totalExpenses;
    private decimal _netBalance;
    private ObservableCollection<CategorySummary> _incomeByCategory;
    private ObservableCollection<CategorySummary> _expensesByCategory;

    public OverviewViewModel(ITransactionService transactionService, ICalculationService calculationService)
    {
        _transactionService = transactionService;
        _calculationService = calculationService;
        
        _selectedMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        _incomeByCategory = new ObservableCollection<CategorySummary>();
        _expensesByCategory = new ObservableCollection<CategorySummary>();
        
        LoadDataCommand = new RelayCommand(async () => await LoadData());
        
        // Load data initially
        _ = LoadData();
    }

    public DateTime SelectedMonth
    {
        get => _selectedMonth;
        set
        {
            if (SetProperty(ref _selectedMonth, value))
            {
                _ = LoadData();
            }
        }
    }

    public decimal TotalIncome
    {
        get => _totalIncome;
        private set => SetProperty(ref _totalIncome, value);
    }

    public decimal TotalExpenses
    {
        get => _totalExpenses;
        private set => SetProperty(ref _totalExpenses, value);
    }

    public decimal NetBalance
    {
        get => _netBalance;
        private set => SetProperty(ref _netBalance, value);
    }

    public ObservableCollection<CategorySummary> IncomeByCategory
    {
        get => _incomeByCategory;
        private set => SetProperty(ref _incomeByCategory, value);
    }

    public ObservableCollection<CategorySummary> ExpensesByCategory
    {
        get => _expensesByCategory;
        private set => SetProperty(ref _expensesByCategory, value);
    }

    public ICommand LoadDataCommand { get; }

    private Task LoadData()
    {
        // Get all transactions for the selected month
        var transactions = _transactionService.GetTransactionsByMonth(SelectedMonth);
        
        // Calculate income by category
        var incomeGroups = transactions
            .Where(t => t.Type == TransactionType.Income)
            .GroupBy(t => t.Category?.Name ?? "Okategoriserad")
            .Select(g => new CategorySummary
            {
                CategoryName = g.Key,
                Total = g.Sum(t => t.Amount)
            })
            .OrderByDescending(x => x.Total)
            .ToList();
        
        IncomeByCategory = new ObservableCollection<CategorySummary>(incomeGroups);
        TotalIncome = incomeGroups.Sum(x => x.Total);
        
        // Calculate expenses by category
        var expenseGroups = transactions
            .Where(t => t.Type == TransactionType.Expense)
            .GroupBy(t => t.Category?.Name ?? "Okategoriserad")
            .Select(g => new CategorySummary
            {
                CategoryName = g.Key,
                Total = g.Sum(t => t.Amount)
            })
            .OrderByDescending(x => x.Total)
            .ToList();
        
        ExpensesByCategory = new ObservableCollection<CategorySummary>(expenseGroups);
        TotalExpenses = expenseGroups.Sum(x => x.Total);
        
        // Calculate net balance
        NetBalance = TotalIncome - TotalExpenses;
        
        return Task.CompletedTask;
    }
}
