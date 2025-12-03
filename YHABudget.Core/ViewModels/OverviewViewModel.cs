using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using YHABudget.Core.Commands;
using YHABudget.Core.MVVM;
using YHABudget.Data.DTOs;
using YHABudget.Data.Enums;
using YHABudget.Data.Services;

namespace YHABudget.Core.ViewModels;

public class MonthDisplay
{
    public DateTime Date { get; set; }
    public string DisplayName { get; set; } = string.Empty;
}

public class OverviewViewModel : ViewModelBase
{
    private readonly ITransactionService _transactionService;
    private readonly IRecurringTransactionService _recurringTransactionService;
    private readonly ICalculationService _calculationService;

    private DateTime _selectedMonth;
    private decimal _totalIncome;
    private decimal _totalExpenses;
    private decimal _netBalance;
    private decimal _accountBalance;
    private decimal _expectedMonthResult;
    private decimal _scheduledIncome;
    private decimal _scheduledExpenses;
    private bool _isCurrentMonth;
    private ObservableCollection<CategorySummary> _incomeByCategory;
    private ObservableCollection<CategorySummary> _expensesByCategory;
    private ObservableCollection<MonthDisplay> _availableMonths;

    public OverviewViewModel(ITransactionService transactionService, IRecurringTransactionService recurringTransactionService, ICalculationService calculationService)
    {
        _transactionService = transactionService;
        _recurringTransactionService = recurringTransactionService;
        _calculationService = calculationService;

        _selectedMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        _incomeByCategory = new ObservableCollection<CategorySummary>();
        _expensesByCategory = new ObservableCollection<CategorySummary>();
        _availableMonths = new ObservableCollection<MonthDisplay>();

        LoadDataCommand = new RelayCommand(() => LoadData());

        // Populate available months and load data
        PopulateAvailableMonths();
        CalculateAccountBalance();
        LoadData();
        CalculateExpectedMonthResult();
    }

    public DateTime SelectedMonth
    {
        get => _selectedMonth;
        set
        {
            if (SetProperty(ref _selectedMonth, value))
            {
                // Process recurring transactions for the new month
                _recurringTransactionService.ProcessRecurringTransactionsForMonth(value);
                LoadData();
                CalculateExpectedMonthResult();
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

    public decimal AccountBalance
    {
        get => _accountBalance;
        private set => SetProperty(ref _accountBalance, value);
    }

    public decimal ExpectedMonthResult
    {
        get => _expectedMonthResult;
        private set => SetProperty(ref _expectedMonthResult, value);
    }

    public decimal ScheduledIncome
    {
        get => _scheduledIncome;
        private set => SetProperty(ref _scheduledIncome, value);
    }

    public decimal ScheduledExpenses
    {
        get => _scheduledExpenses;
        private set => SetProperty(ref _scheduledExpenses, value);
    }

    public bool IsCurrentMonth
    {
        get => _isCurrentMonth;
        private set => SetProperty(ref _isCurrentMonth, value);
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

    public ObservableCollection<MonthDisplay> AvailableMonths
    {
        get => _availableMonths;
        private set => SetProperty(ref _availableMonths, value);
    }

    public ICommand LoadDataCommand { get; }

    private void LoadData()
    {
        // Get all transactions for the selected month
        var transactions = _transactionService.GetTransactionsByMonth(SelectedMonth);        // Calculate income by category
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

        // Replace entire collection with single assignment
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

        // Replace entire collection with single assignment
        ExpensesByCategory = new ObservableCollection<CategorySummary>(expenseGroups);
        TotalExpenses = expenseGroups.Sum(x => x.Total);

        // Calculate net balance
        NetBalance = TotalIncome - TotalExpenses;
    }

    private void CalculateExpectedMonthResult()
    {
        var result = _calculationService.CalculateExpectedMonthResult(SelectedMonth, NetBalance);
        
        IsCurrentMonth = result.IsCurrentMonth;
        ScheduledIncome = result.ScheduledIncome;
        ScheduledExpenses = result.ScheduledExpenses;
        ExpectedMonthResult = result.ProjectedNetBalance;
    }

    private void CalculateAccountBalance()
    {
        // Calculate account balance (all transactions, current balance)
        var allTransactions = _transactionService.GetAllTransactions();

        var totalIncome = allTransactions
            .Where(t => t.Type == TransactionType.Income)
            .Sum(t => t.Amount);

        var totalExpenses = allTransactions
            .Where(t => t.Type == TransactionType.Expense)
            .Sum(t => t.Amount);

        AccountBalance = totalIncome - totalExpenses;
    }

    private void PopulateAvailableMonths()
    {
        // Get all transactions
        var allTransactions = _transactionService.GetAllTransactions();

        if (!allTransactions.Any())
        {
            // If no transactions, add current month
            var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            AvailableMonths.Add(new MonthDisplay
            {
                Date = currentMonth,
                DisplayName = FormatMonthYear(currentMonth)
            });
            return;
        }

        // Get distinct months from transactions
        var distinctMonths = allTransactions
            .Select(t => new DateTime(t.Date.Year, t.Date.Month, 1))
            .Distinct()
            .OrderByDescending(d => d)
            .ToList();

        // Add to collection
        AvailableMonths.Clear();
        foreach (var month in distinctMonths)
        {
            AvailableMonths.Add(new MonthDisplay
            {
                Date = month,
                DisplayName = FormatMonthYear(month)
            });
        }
    }

    private string FormatMonthYear(DateTime date)
    {
        // Format as "November 2025" with Swedish culture
        var culture = new CultureInfo("sv-SE");
        var formatted = date.ToString("MMMM yyyy", culture);

        // Capitalize first letter
        if (!string.IsNullOrEmpty(formatted))
        {
            formatted = char.ToUpper(formatted[0]) + formatted.Substring(1);
        }

        return formatted;
    }
}
