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
    private readonly IDialogService _dialogService;

    private ObservableCollection<Transaction> _transactions;
    private ObservableCollection<Category> _categories;
    private TransactionType? _selectedTypeFilter;
    private int? _selectedCategoryFilter;
    private DateTime? _selectedMonthFilter;
    private decimal _totalAmount;
    private Transaction? _selectedTransaction;

    public TransactionViewModel(ITransactionService transactionService, ICategoryService categoryService, IDialogService dialogService)
    {
        _transactionService = transactionService;
        _categoryService = categoryService;
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
                LoadData();
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
                LoadData();
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
                LoadData();
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

        // Load transactions based on filters
        var newTransactions = _transactionService.GetTransactionsByFilter(
            SelectedTypeFilter,
            SelectedCategoryFilter,
            SelectedMonthFilter
        ).ToList();

        // Update existing collection efficiently
        // Remove transactions that are no longer in the new list
        for (int i = Transactions.Count - 1; i >= 0; i--)
        {
            if (!newTransactions.Any(t => t.Id == Transactions[i].Id))
            {
                Transactions.RemoveAt(i);
            }
        }

        // Add or update transactions
        foreach (var transaction in newTransactions)
        {
            var existing = Transactions.FirstOrDefault(t => t.Id == transaction.Id);
            if (existing == null)
            {
                // Add new transaction in the right position (by date, descending)
                int insertIndex = 0;
                for (int i = 0; i < Transactions.Count; i++)
                {
                    if (transaction.Date > Transactions[i].Date)
                    {
                        insertIndex = i;
                        break;
                    }
                    insertIndex = i + 1;
                }
                Transactions.Insert(insertIndex, transaction);
            }
            else if (!existing.Equals(transaction))
            {
                // Update existing transaction properties
                int index = Transactions.IndexOf(existing);
                Transactions[index] = transaction;
            }
        }

        // Calculate total (income positive, expense negative for display)
        TotalAmount = Transactions.Sum(t =>
            t.Type == TransactionType.Income ? t.Amount : -t.Amount
        );
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
        LoadData();
    }

    private void AddTransaction()
    {
        var result = _dialogService.ShowTransactionDialog();
        if (result == true)
        {
            LoadData();
        }
    }

    private void EditTransaction()
    {
        if (SelectedTransaction == null)
            return;

        var result = _dialogService.ShowTransactionDialog(SelectedTransaction);
        if (result == true)
        {
            LoadData();
        }
    }
}
