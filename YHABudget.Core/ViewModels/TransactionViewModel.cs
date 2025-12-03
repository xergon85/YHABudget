using System.Collections.ObjectModel;
using System.Windows.Input;
using YHABudget.Core.Commands;
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
    private ObservableCollection<Category> _categories;
    private List<Transaction> _allTransactions = new(); // Store all transactions
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
        _categories = new ObservableCollection<Category>();

        LoadDataCommand = new RelayCommand(() => LoadData());
        ClearFiltersCommand = new RelayCommand(() => ClearFilters());
        DeleteTransactionCommand = new RelayCommand<int>((id) => DeleteTransaction(id));
        AddTransactionCommand = new RelayCommand(() => AddTransaction());
        EditTransactionCommand = new RelayCommand(() => EditTransaction(), () => SelectedTransaction != null);

        LoadData();
    }

    public ObservableCollection<Transaction> Transactions
    {
        get => _transactions;
        private set => SetProperty(ref _transactions, value);
    }

    public ObservableCollection<Category> Categories
    {
        get => _categories;
        private set => SetProperty(ref _categories, value);
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
        // Filter transactions in memory (no database call)
        var filtered = _allTransactions.AsEnumerable();

        if (SelectedTypeFilter.HasValue)
        {
            filtered = filtered.Where(t => t.Type == SelectedTypeFilter.Value);
        }

        if (SelectedCategoryFilter.HasValue)
        {
            filtered = filtered.Where(t => t.CategoryId == SelectedCategoryFilter.Value);
        }

        if (SelectedMonthFilter.HasValue)
        {
            var month = SelectedMonthFilter.Value;
            var monthStart = new DateTime(month.Year, month.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            filtered = filtered.Where(t => t.Date >= monthStart && t.Date <= monthEnd);
        }

        var filteredList = filtered.OrderByDescending(t => t.Date).ToList();

        // Replace collection (single notification)
        Transactions = new ObservableCollection<Transaction>(filteredList);

        // Calculate total
        TotalAmount = Transactions.Sum(t =>
            t.Type == TransactionType.Income ? t.Amount : -t.Amount
        );
    }

    private void LoadData()
    {
        // Load categories (only once)
        if (Categories.Count == 0)
        {
            var categories = _categoryService.GetAllCategories();
            foreach (var category in categories)
            {
                Categories.Add(category);
            }
        }

        // Load ALL transactions from database once
        _allTransactions = _transactionService.GetAllTransactions().ToList();

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
        
        // Remove from in-memory list
        var transaction = _allTransactions.FirstOrDefault(t => t.Id == transactionId);
        if (transaction != null)
        {
            _allTransactions.Remove(transaction);
            ApplyFilters();
        }
    }

    private void AddTransaction()
    {
        var result = _dialogService.ShowTransactionDialog();
        if (result == true)
        {
            // Reload just the new transaction from database
            var allTransactions = _transactionService.GetAllTransactions().ToList();
            var newTransaction = allTransactions.FirstOrDefault(t => !_allTransactions.Any(existing => existing.Id == t.Id));
            if (newTransaction != null)
            {
                _allTransactions.Add(newTransaction);
                ApplyFilters();
            }
        }
    }

    private void EditTransaction()
    {
        if (SelectedTransaction == null)
            return;

        _dialogService.ShowTransactionDialog(SelectedTransaction);
    }
}
