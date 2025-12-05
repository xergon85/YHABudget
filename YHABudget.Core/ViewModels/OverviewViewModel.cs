using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using YHABudget.Core.Commands;
using YHABudget.Core.MVVM;
using YHABudget.Data.DTOs;
using YHABudget.Data.Enums;
using YHABudget.Data.Services;

namespace YHABudget.Core.ViewModels;

public class OverviewViewModel : ViewModelBase
{
    private readonly IRecurringTransactionService _recurringTransactionService;
    private readonly ICalculationService _calculationService;

    private DateTime _selectedMonth;
    private decimal _totalIncome;
    private decimal _totalExpenses;
    private decimal _netBalance;
    private decimal _accountBalance;
    private decimal _expectedAccountBalance;
    private decimal _expectedMonthResult;
    private decimal _scheduledIncome;
    private decimal _scheduledExpenses;
    private bool _isCurrentMonth;
    private bool _isPastMonth;
    private ObservableCollection<ScheduledTransactionSummary> _scheduledIncomeTransactions;
    private ObservableCollection<ScheduledTransactionSummary> _scheduledExpenseTransactions;
    private ObservableCollection<CategorySummary> _incomeByCategory;
    private ObservableCollection<CategorySummary> _expensesByCategory;
    private ObservableCollection<MonthDisplay> _availableMonths;

    public OverviewViewModel(IRecurringTransactionService recurringTransactionService, ICalculationService calculationService)
    {
        _recurringTransactionService = recurringTransactionService;
        _calculationService = calculationService;

        _incomeByCategory = new ObservableCollection<CategorySummary>();
        _expensesByCategory = new ObservableCollection<CategorySummary>();
        _availableMonths = new ObservableCollection<MonthDisplay>();
        _scheduledIncomeTransactions = new ObservableCollection<ScheduledTransactionSummary>();
        _scheduledExpenseTransactions = new ObservableCollection<ScheduledTransactionSummary>();

        LoadDataCommand = new RelayCommand(() => RefreshData());

        // Set selected month to current month initially
        _selectedMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

        // Load data (this populates AvailableMonths)
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
                RefreshData();
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

    public decimal ExpectedAccountBalance
    {
        get => _expectedAccountBalance;
        private set => SetProperty(ref _expectedAccountBalance, value);
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

    public bool IsPastMonth
    {
        get => _isPastMonth;
        private set => SetProperty(ref _isPastMonth, value);
    }

    public ObservableCollection<ScheduledTransactionSummary> ScheduledIncomeTransactions
    {
        get => _scheduledIncomeTransactions;
        private set => SetProperty(ref _scheduledIncomeTransactions, value);
    }

    public ObservableCollection<ScheduledTransactionSummary> ScheduledExpenseTransactions
    {
        get => _scheduledExpenseTransactions;
        private set => SetProperty(ref _scheduledExpenseTransactions, value);
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
        var overview = _calculationService.GetMonthOverview(SelectedMonth);

        TotalIncome = overview.TotalIncome;
        TotalExpenses = overview.TotalExpenses;
        NetBalance = overview.NetBalance;
        AccountBalance = overview.AccountBalance;
        IncomeByCategory = new ObservableCollection<CategorySummary>(overview.IncomeByCategory);
        ExpensesByCategory = new ObservableCollection<CategorySummary>(overview.ExpensesByCategory);

        // Update available months only if they've changed
        var newMonths = overview.AvailableMonths;
        var currentMonths = AvailableMonths.Select(m => m.Date).ToList();

        if (!newMonths.SequenceEqual(currentMonths))
        {
            AvailableMonths.Clear();
            foreach (var date in newMonths)
            {
                AvailableMonths.Add(new MonthDisplay
                {
                    Date = date,
                    DisplayName = FormatMonthYear(date)
                });
            }
        }
    }

    private void RefreshData()
    {
        // Process recurring transactions for the current month, then reload data
        _recurringTransactionService.ProcessRecurringTransactionsForMonth(SelectedMonth);
        LoadData();
        CalculateExpectedMonthResult();
    }

    private void CalculateExpectedMonthResult()
    {
        var result = _calculationService.CalculateExpectedMonthResult(SelectedMonth);

        IsCurrentMonth = result.IsCurrentMonth;
        IsPastMonth = result.IsPastMonth;
        ScheduledIncome = result.ScheduledIncome;
        ScheduledExpenses = result.ScheduledExpenses;
        ExpectedMonthResult = result.ProjectedNetBalance;
        ExpectedAccountBalance = result.ExpectedAccountBalance;
        ScheduledIncomeTransactions = new ObservableCollection<ScheduledTransactionSummary>(result.ScheduledIncomeTransactions);
        ScheduledExpenseTransactions = new ObservableCollection<ScheduledTransactionSummary>(result.ScheduledExpenseTransactions);
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
