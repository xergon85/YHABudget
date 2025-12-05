using System.Collections.ObjectModel;
using System.Windows.Input;
using YHABudget.Core.Commands;
using YHABudget.Core.Helpers;
using YHABudget.Core.MVVM;
using YHABudget.Core.Services;
using YHABudget.Data.Enums;
using YHABudget.Data.Models;
using YHABudget.Data.Services;

namespace YHABudget.Core.ViewModels;

public class TransactionViewModel : ViewModelBase
{
    private readonly ITransactionService _transactionService;
    private readonly ICategoryService _categoryService;
    private readonly IRecurringTransactionService _recurringTransactionService;
    private readonly IDialogService _dialogService;

    private ObservableCollection<Transaction> _transactions;
    private ObservableCollection<CategoryOption> _categories;
    private ObservableCollection<MonthOption> _availableMonths;
    private TransactionType? _selectedTypeFilter;
    private int? _selectedCategoryFilter;
    private DateTime? _selectedMonthFilter;
    private decimal _totalAmount;
    private Transaction? _selectedTransaction;

    public TransactionViewModel(ITransactionService transactionService, ICategoryService categoryService, IRecurringTransactionService recurringTransactionService, IDialogService dialogService)
    {
        _transactionService = transactionService;
        _categoryService = categoryService;
        _recurringTransactionService = recurringTransactionService;
        _dialogService = dialogService;

        _transactions = new ObservableCollection<Transaction>();
        _categories = new ObservableCollection<CategoryOption>();
        _availableMonths = new ObservableCollection<MonthOption>();

        LoadDataCommand = new RelayCommand(() => LoadData());
        ClearFiltersCommand = new RelayCommand(() => ClearFilters());
        DeleteTransactionCommand = new RelayCommand<int>((id) => DeleteTransaction(id));
        AddTransactionCommand = new RelayCommand(() => AddTransaction());
        EditTransactionCommand = new RelayCommand(() => EditTransaction(), () => SelectedTransaction != null);

        LoadData();
        InitializeMonths();
    }

    public ObservableCollection<Transaction> Transactions
    {
        get => _transactions;
        private set => SetProperty(ref _transactions, value);
    }

    public ObservableCollection<CategoryOption> Categories
    {
        get => _categories;
        private set => SetProperty(ref _categories, value);
    }

    public ObservableCollection<MonthOption> AvailableMonths
    {
        get => _availableMonths;
        private set => SetProperty(ref _availableMonths, value);
    }

    public TransactionType? SelectedTypeFilter
    {
        get => _selectedTypeFilter;
        set
        {
            if (SetProperty(ref _selectedTypeFilter, value))
            {
                ApplyFilters();
            }
        }
    }

    public int? SelectedCategoryFilter
    {
        get => _selectedCategoryFilter;
        set
        {
            if (SetProperty(ref _selectedCategoryFilter, value))
            {
                ApplyFilters();
            }
        }
    }

    public DateTime? SelectedMonthFilter
    {
        get => _selectedMonthFilter;
        set
        {
            if (SetProperty(ref _selectedMonthFilter, value))
            {
                ApplyFilters();
            }
        }
    }

    public decimal TotalAmount
    {
        get => _totalAmount;
        private set => SetProperty(ref _totalAmount, value);
    }

    public Transaction? SelectedTransaction
    {
        get => _selectedTransaction;
        set
        {
            if (SetProperty(ref _selectedTransaction, value))
            {
                OnPropertyChanged(nameof(EditTransactionCommand));
            }
        }
    }

    public ICommand LoadDataCommand { get; }
    public ICommand ClearFiltersCommand { get; }
    public ICommand DeleteTransactionCommand { get; }
    public ICommand AddTransactionCommand { get; }
    public ICommand EditTransactionCommand { get; }

    private void ApplyFilters()
    {
        // Use service to filter transactions
        var filtered = _transactionService.GetTransactionsByFilter(
            SelectedTypeFilter, 
            SelectedCategoryFilter, 
            SelectedMonthFilter);

        var filteredList = filtered.OrderByDescending(t => t.Date).ToList();

        // Replace collection (single notification)
        Transactions = new ObservableCollection<Transaction>(filteredList);

        // Calculate total
        TotalAmount = Transactions.Sum(t =>
            t.Type == TransactionType.Income ? t.Amount : -t.Amount
        );
    }

    private void InitializeMonths()
    {
        // Get unique months from service
        var monthsWithTransactions = _transactionService.GetMonthsWithTransactions().ToList();

        var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        if (!monthsWithTransactions.Contains(currentMonth))
        {
            monthsWithTransactions.Add(currentMonth);
            monthsWithTransactions = monthsWithTransactions.OrderByDescending(d => d).ToList();
        }

        AvailableMonths.Clear();
        
        // Add "Show All" option
        AvailableMonths.Add(new MonthOption
        {
            Date = null,
            DisplayText = "Visa alla"
        });
        
        foreach (var month in monthsWithTransactions)
        {
            var isCurrentMonth = month == currentMonth;
            var displayText = DateFormatHelper.FormatMonthYear(month);
            AvailableMonths.Add(new MonthOption
            {
                Date = month,
                DisplayText = isCurrentMonth ? $"★ {displayText}" : displayText
            });
        }
    }

    private void LoadData()
    {
        // Load categories (only once)
        if (Categories.Count == 0)
        {
            // Add "Show All" option
            Categories.Add(new CategoryOption
            {
                Id = null,
                Name = "Visa alla"
            });
            
            var categories = _categoryService.GetAllCategories();
            foreach (var category in categories)
            {
                Categories.Add(new CategoryOption
                {
                    Id = category.Id,
                    Name = category.Name
                });
            }
        }

        // Refresh available months
        InitializeMonths();

        // Apply current filters to display
        ApplyFilters();
    }

    private void ClearFilters()
    {
        SelectedTypeFilter = null;
        SelectedCategoryFilter = null;
        SelectedMonthFilter = null;
    }

    private void DeleteTransaction(int transactionId)
    {
        _transactionService.DeleteTransaction(transactionId);
        
        // Reload data to refresh the list and months
        LoadData();
    }

    private void AddTransaction()
    {
        var result = _dialogService.ShowTransactionDialog();
        if (result == true)
        {
            // Reload data to refresh the list and months
            LoadData();
        }
    }

    private void EditTransaction()
    {
        if (SelectedTransaction == null)
            return;

        _dialogService.ShowTransactionDialog(SelectedTransaction);
    }

    public class MonthOption
    {
        public DateTime? Date { get; set; }
        public string DisplayText { get; set; } = string.Empty;
    }

    public class CategoryOption
    {
        public int? Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
