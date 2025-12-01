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
        // Load categories
        var categories = _categoryService.GetAllCategories();
        Categories = new ObservableCollection<Category>(categories);

        // Load transactions based on filters
        var transactions = _transactionService.GetTransactionsByFilter(
            SelectedTypeFilter,
            SelectedCategoryFilter,
            SelectedMonthFilter
        );

        Transactions = new ObservableCollection<Transaction>(transactions);

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
