using System.Collections.ObjectModel;
using System.Windows.Input;
using YHABudget.Core.Commands;
using YHABudget.Core.MVVM;
using YHABudget.Data.Enums;
using YHABudget.Data.Models;
using YHABudget.Data.Services;

namespace YHABudget.Core.ViewModels;

public class RecurringTransactionDialogViewModel : ViewModelBase
{
    private readonly ICategoryService _categoryService;
    private readonly IRecurringTransactionService _recurringTransactionService;

    // Private fields for all properties
    private int? _recurringTransactionId;
    private decimal? _amount;
    private string _description = string.Empty;
    private int? _selectedCategoryId;
    private TransactionType _transactionType = TransactionType.Expense;
    private RecurrenceType _recurrenceType = RecurrenceType.Monthly;
    private int? _recurrenceMonth;
    private DateTime _startDate = DateTime.Now;
    private DateTime? _endDate;
    private bool _isActive = true;
    private ObservableCollection<Category> _availableCategories;
    private string _errorMessage = string.Empty;
    private bool _isEditMode;
    private bool _saveSuccessful;

    public RecurringTransactionDialogViewModel(
        ICategoryService categoryService, 
        IRecurringTransactionService recurringTransactionService)
    {
        _categoryService = categoryService;
        _recurringTransactionService = recurringTransactionService;
        _availableCategories = new ObservableCollection<Category>();
        
        SaveCommand = new RelayCommand(() => Save(), CanSave);
        CancelCommand = new RelayCommand(() => Cancel());
        
        LoadCategories();
    }

    // Basic properties
    public int? RecurringTransactionId
    {
        get => _recurringTransactionId;
        set => SetProperty(ref _recurringTransactionId, value);
    }

    public decimal? Amount
    {
        get => _amount;
        set
        {
            if (SetProperty(ref _amount, value))
            {
                ValidateAmount();
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public string Description
    {
        get => _description;
        set
        {
            if (SetProperty(ref _description, value))
            {
                ValidateDescription();
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public int? SelectedCategoryId
    {
        get => _selectedCategoryId;
        set
        {
            if (SetProperty(ref _selectedCategoryId, value))
            {
                ValidateCategory();
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public TransactionType TransactionType
    {
        get => _transactionType;
        set
        {
            if (SetProperty(ref _transactionType, value))
            {
                LoadCategories();
            }
        }
    }

    // Recurrence properties
    public RecurrenceType RecurrenceType
    {
        get => _recurrenceType;
        set
        {
            if (SetProperty(ref _recurrenceType, value))
            {
                ValidateRecurrence();
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public int? RecurrenceMonth
    {
        get => _recurrenceMonth;
        set
        {
            if (SetProperty(ref _recurrenceMonth, value))
            {
                ValidateRecurrence();
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public DateTime StartDate
    {
        get => _startDate;
        set => SetProperty(ref _startDate, value);
    }

    public DateTime? EndDate
    {
        get => _endDate;
        set => SetProperty(ref _endDate, value);
    }

    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    // UI properties
    public ObservableCollection<Category> AvailableCategories
    {
        get => _availableCategories;
        private set => SetProperty(ref _availableCategories, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool IsEditMode
    {
        get => _isEditMode;
        private set => SetProperty(ref _isEditMode, value);
    }
    
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public bool SaveSuccessful
    {
        get => _saveSuccessful;
        private set => SetProperty(ref _saveSuccessful, value);
    }

    public event EventHandler? RequestClose;

    // Load existing recurring transaction for editing
    public void LoadRecurringTransaction(RecurringTransaction recurringTransaction)
    {
        IsEditMode = true;
        RecurringTransactionId = recurringTransaction.Id;
        Amount = recurringTransaction.Amount;
        Description = recurringTransaction.Description;
        TransactionType = recurringTransaction.Type;
        SelectedCategoryId = recurringTransaction.CategoryId;
        RecurrenceType = recurringTransaction.RecurrenceType;
        RecurrenceMonth = recurringTransaction.RecurrenceMonth;
        StartDate = recurringTransaction.StartDate;
        EndDate = recurringTransaction.EndDate;
        IsActive = recurringTransaction.IsActive;
    }

    private void LoadCategories()
    {
        var categories = _categoryService.GetCategoriesByType(TransactionType);
        AvailableCategories.Clear();
        foreach (var category in categories)
        {
            AvailableCategories.Add(category);
        }

        // Reset category selection if current category doesn't match new type
        if (SelectedCategoryId.HasValue)
        {
            var categoryExists = AvailableCategories.Any(c => c.Id == SelectedCategoryId.Value);
            if (!categoryExists)
            {
                SelectedCategoryId = null;
            }
        }
    }

    // Validation methods
    private void ValidateAmount()
    {
        if (!Amount.HasValue || Amount.Value <= 0)
        {
            ErrorMessage = "Belopp måste vara större än 0";
        }
        else
        {
            ErrorMessage = string.Empty;
        }
    }

    private void ValidateDescription()
    {
        if (string.IsNullOrWhiteSpace(Description))
        {
            ErrorMessage = "Beskrivning måste anges";
        }
        else
        {
            ErrorMessage = string.Empty;
        }
    }

    private void ValidateCategory()
    {
        if (!SelectedCategoryId.HasValue)
        {
            ErrorMessage = "Kategori måste väljas";
        }
        else
        {
            ErrorMessage = string.Empty;
        }
    }

    private void ValidateRecurrence()
    {
        // If RecurrenceType is Yearly, RecurrenceMonth must be set
        if (RecurrenceType == RecurrenceType.Yearly && !RecurrenceMonth.HasValue)
        {
            ErrorMessage = "Månad måste väljas för årlig återkommande transaktion";
        }
        else
        {
            ErrorMessage = string.Empty;
        }
    }

    private bool CanSave()
    {
        bool hasValidAmount = Amount.HasValue && Amount.Value > 0;
        bool hasDescription = !string.IsNullOrWhiteSpace(Description);
        bool hasCategory = SelectedCategoryId.HasValue;
        bool hasValidRecurrence = RecurrenceType == RecurrenceType.Monthly || 
                                  (RecurrenceType == RecurrenceType.Yearly && RecurrenceMonth.HasValue);
        bool noErrors = string.IsNullOrEmpty(ErrorMessage);

        return hasValidAmount && hasDescription && hasCategory && hasValidRecurrence && noErrors;
    }

    private void Save()
    {
        if (!CanSave())
        {
            return;
        }

        try
        {
            if (IsEditMode && RecurringTransactionId.HasValue)
            {
                // Update existing
                var recurringTransaction = _recurringTransactionService
                    .GetRecurringTransactionById(RecurringTransactionId.Value);
                if (recurringTransaction != null)
                {
                    recurringTransaction.Amount = Amount!.Value;
                    recurringTransaction.Description = Description;
                    recurringTransaction.CategoryId = SelectedCategoryId!.Value;
                    recurringTransaction.Type = TransactionType;
                    recurringTransaction.RecurrenceType = RecurrenceType;
                    recurringTransaction.RecurrenceMonth = RecurrenceMonth;
                    recurringTransaction.StartDate = StartDate;
                    recurringTransaction.EndDate = EndDate;
                    recurringTransaction.IsActive = IsActive;

                    _recurringTransactionService.UpdateRecurringTransaction(recurringTransaction);
                }
            }
            else
            {
                // Create new
                var recurringTransaction = new RecurringTransaction
                {
                    Amount = Amount!.Value,
                    Description = Description,
                    CategoryId = SelectedCategoryId!.Value,
                    Type = TransactionType,
                    RecurrenceType = RecurrenceType,
                    RecurrenceMonth = RecurrenceMonth,
                    StartDate = StartDate,
                    EndDate = EndDate,
                    IsActive = IsActive
                };

                _recurringTransactionService.AddRecurringTransaction(recurringTransaction);
            }

            SaveSuccessful = true;
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Fel vid sparande: {ex.Message}";
        }
    }

    private void Cancel()
    {
        SaveSuccessful = false;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }
}
