using System.Collections.ObjectModel;
using System.Windows.Input;
using YHABudget.Core.Commands;
using YHABudget.Core.MVVM;
using YHABudget.Core.Services;
using YHABudget.Data.Models;
using YHABudget.Data.Services;

namespace YHABudget.Core.ViewModels;

public class RecurringTransactionViewModel : ViewModelBase
{
    private readonly IRecurringTransactionService _recurringTransactionService;
    private readonly IDialogService _dialogService;

    private ObservableCollection<RecurringTransaction> _recurringTransactions;
    private RecurringTransaction? _selectedRecurringTransaction;

    public RecurringTransactionViewModel(IRecurringTransactionService recurringTransactionService, IDialogService dialogService)
    {
        _recurringTransactionService = recurringTransactionService;
        _dialogService = dialogService;

        _recurringTransactions = new ObservableCollection<RecurringTransaction>();

        LoadDataCommand = new RelayCommand(() => LoadData());
        AddRecurringTransactionCommand = new RelayCommand(() => AddRecurringTransaction());
        EditRecurringTransactionCommand = new RelayCommand(() => EditRecurringTransaction(), () => SelectedRecurringTransaction != null);
        DeleteRecurringTransactionCommand = new RelayCommand<int>((id) => DeleteRecurringTransaction(id));
        ToggleActiveCommand = new RelayCommand<int>((id) => ToggleActive(id));

        LoadData();
    }

    public ObservableCollection<RecurringTransaction> RecurringTransactions
    {
        get => _recurringTransactions;
        private set => SetProperty(ref _recurringTransactions, value);
    }

    public RecurringTransaction? SelectedRecurringTransaction
    {
        get => _selectedRecurringTransaction;
        set
        {
            if (SetProperty(ref _selectedRecurringTransaction, value))
            {
                OnPropertyChanged(nameof(EditRecurringTransactionCommand));
            }
        }
    }

    public ICommand LoadDataCommand { get; }
    public ICommand AddRecurringTransactionCommand { get; }
    public ICommand EditRecurringTransactionCommand { get; }
    public ICommand DeleteRecurringTransactionCommand { get; }
    public ICommand ToggleActiveCommand { get; }

    private void LoadData()
    {
        var recurringTransactions = _recurringTransactionService.GetAllRecurringTransactions().ToList();

        // Replace entire collection with single assignment (one notification instead of N+1)
        RecurringTransactions = new ObservableCollection<RecurringTransaction>(recurringTransactions);
    }

    private void AddRecurringTransaction()
    {
        var result = _dialogService.ShowRecurringTransactionDialog();
        if (result == true)
        {
            // Process recurring transactions for current month after adding
            _recurringTransactionService.ProcessRecurringTransactionsForMonth(DateTime.Now);
            LoadData();
        }
    }

    private void EditRecurringTransaction()
    {
        if (SelectedRecurringTransaction == null)
            return;

        var result = _dialogService.ShowRecurringTransactionDialog(SelectedRecurringTransaction);
        if (result == true)
        {
            // Process recurring transactions for current month after editing
            _recurringTransactionService.ProcessRecurringTransactionsForMonth(DateTime.Now);
            LoadData();
        }
    }

    private void DeleteRecurringTransaction(int id)
    {
        _recurringTransactionService.DeleteRecurringTransaction(id);
        LoadData();
    }

    private void ToggleActive(int id)
    {
        var recurringTransaction = _recurringTransactionService.GetRecurringTransactionById(id);
        if (recurringTransaction != null)
        {
            recurringTransaction.IsActive = !recurringTransaction.IsActive;
            _recurringTransactionService.UpdateRecurringTransaction(recurringTransaction);
            // Process recurring transactions for current month after toggling
            _recurringTransactionService.ProcessRecurringTransactionsForMonth(DateTime.Now);
            LoadData();
        }
    }
}

