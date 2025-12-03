using System.Collections.ObjectModel;
using System.Windows.Input;
using YHABudget.Core.Commands;
using YHABudget.Core.MVVM;
using YHABudget.Data.Enums;
using YHABudget.Data.Models;
using YHABudget.Data.Services;

namespace YHABudget.Core.ViewModels;

public class TransactionDialogViewModel : ViewModelBase
{
    private readonly ICategoryService _categoryService;
    private readonly ITransactionService _transactionService;

    private Transaction? _transactionReference; // Keep reference to update original object
    private int? _transactionId;
    private decimal? _amount;
    private string _description = string.Empty;
    private DateTime _date = DateTime.Now;
    private int? _selectedCategoryId;
    private TransactionType _transactionType = TransactionType.Expense;
    private ObservableCollection<Category> _availableCategories;
    private string _errorMessage = string.Empty;
    private bool _isEditMode;
    private bool _saveSuccessful;

    public TransactionDialogViewModel(ICategoryService categoryService, ITransactionService transactionService)
    {
        _categoryService = categoryService;
        _transactionService = transactionService;
        _availableCategories = new ObservableCollection<Category>();
        
        SaveCommand = new RelayCommand(() => Save(), CanSave);
        CancelCommand = new RelayCommand(() => Cancel());
        
        LoadCategories();
    }    public int? TransactionId
    {
        get => _transactionId;
        set => SetProperty(ref _transactionId, value);
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

    public DateTime Date
    {
        get => _date;
        set => SetProperty(ref _date, value);
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

    public void LoadTransaction(Transaction transaction)
    {
        _transactionReference = transaction; // Store reference
        IsEditMode = true;
        TransactionId = transaction.Id;
        Amount = transaction.Amount;
        Description = transaction.Description;
        Date = transaction.Date;
        TransactionType = transaction.Type;
        SelectedCategoryId = transaction.CategoryId;
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

    private bool CanSave()
    {
        return Amount.HasValue
            && Amount.Value > 0
            && !string.IsNullOrWhiteSpace(Description)
            && SelectedCategoryId.HasValue
            && string.IsNullOrEmpty(ErrorMessage);
    }

    private void Save()
    {
        if (!CanSave())
        {
            return;
        }

        try
        {
            if (IsEditMode && _transactionReference != null)
            {
                // Update the original transaction object directly
                _transactionReference.Amount = Amount!.Value;
                _transactionReference.Description = Description;
                _transactionReference.Date = Date;
                _transactionReference.CategoryId = SelectedCategoryId!.Value;
                _transactionReference.Type = TransactionType;

                _transactionService.UpdateTransaction(_transactionReference);
            }
            else
            {
                var transaction = new Transaction
                {
                    Amount = Amount!.Value,
                    Description = Description,
                    Date = Date,
                    CategoryId = SelectedCategoryId!.Value,
                    Type = TransactionType,
                    IsRecurring = false
                };

                _transactionService.AddTransaction(transaction);
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
