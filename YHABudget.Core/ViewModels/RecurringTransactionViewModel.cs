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

        // Update existing collection efficiently
        for (int i = RecurringTransactions.Count - 1; i >= 0; i--)
        {
            if (!recurringTransactions.Any(rt => rt.Id == RecurringTransactions[i].Id))
            {
                RecurringTransactions.RemoveAt(i);
            }
        }

        foreach (var recurringTransaction in recurringTransactions)
        {
            var existing = RecurringTransactions.FirstOrDefault(rt => rt.Id == recurringTransaction.Id);
            if (existing == null)
            {
                RecurringTransactions.Add(recurringTransaction);
            }
            else if (!existing.Equals(recurringTransaction))
            {
                int index = RecurringTransactions.IndexOf(existing);
                RecurringTransactions[index] = recurringTransaction;
            }
        }
    }

    private void AddRecurringTransaction()
    {
        var result = _dialogService.ShowRecurringTransactionDialog();
        if (result == true)
        {
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
            LoadData();
        }
    }
}

