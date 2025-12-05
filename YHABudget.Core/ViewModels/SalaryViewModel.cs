using System.Collections.ObjectModel;
using System.Windows.Input;
using YHABudget.Core.Commands;
using YHABudget.Core.MVVM;
using YHABudget.Core.Services;
using YHABudget.Data.Enums;
using YHABudget.Data.Models;
using YHABudget.Data.Services;

namespace YHABudget.Core.ViewModels;

public class SalaryViewModel : ViewModelBase
{
    private readonly ISalarySettingsService _salarySettingsService;
    private readonly IDialogService _dialogService;
    private readonly IRecurringTransactionService _recurringTransactionService;
    private readonly ICategoryService _categoryService;

    private ObservableCollection<SalarySettings> _salaries;
    private decimal _totalMonthlyIncome;

    public SalaryViewModel(
        ISalarySettingsService salarySettingsService,
        IDialogService dialogService,
        IRecurringTransactionService recurringTransactionService,
        ICategoryService categoryService)
    {
        _salarySettingsService = salarySettingsService;
        _dialogService = dialogService;
        _recurringTransactionService = recurringTransactionService;
        _categoryService = categoryService;

        _salaries = new ObservableCollection<SalarySettings>();

        LoadDataCommand = new RelayCommand(() => LoadData());
        AddSalaryCommand = new RelayCommand(() => AddSalary());
        EditSalaryCommand = new RelayCommand<SalarySettings>((salary) => EditSalary(salary));
        DeleteSalaryCommand = new RelayCommand<SalarySettings>((salary) => DeleteSalary(salary));
        CreateRecurringTransactionCommand = new RelayCommand<SalarySettings>((salary) => CreateRecurringTransaction(salary));

        LoadData();
    }

    public ObservableCollection<SalarySettings> Salaries
    {
        get => _salaries;
        private set => SetProperty(ref _salaries, value);
    }

    public decimal TotalMonthlyIncome
    {
        get => _totalMonthlyIncome;
        private set => SetProperty(ref _totalMonthlyIncome, value);
    }

    public ICommand LoadDataCommand { get; }
    public ICommand AddSalaryCommand { get; }
    public ICommand EditSalaryCommand { get; }
    public ICommand DeleteSalaryCommand { get; }
    public ICommand CreateRecurringTransactionCommand { get; }

    private void LoadData()
    {
        var salaries = _salarySettingsService.GetAllSettings();
        Salaries.Clear();
        foreach (var salary in salaries)
        {
            Salaries.Add(salary);
        }

        CalculateTotalMonthlyIncome();
    }

    private void AddSalary()
    {
        var result = _dialogService.ShowSalaryDialog(null);
        if (result != null)
        {
            var added = _salarySettingsService.AddSettings(result);
            Salaries.Add(added);
            CalculateTotalMonthlyIncome();
        }
    }

    private void EditSalary(SalarySettings? salary)
    {
        if (salary == null) return;

        var result = _dialogService.ShowSalaryDialog(salary);
        if (result != null)
        {
            _salarySettingsService.UpdateSettings(result);

            // Refresh the item in the collection
            var index = Salaries.IndexOf(salary);
            if (index >= 0)
            {
                var updated = _salarySettingsService.GetSettingsById(result.Id);
                if (updated != null)
                {
                    Salaries[index] = updated;
                }
            }

            CalculateTotalMonthlyIncome();
        }
    }

    private void DeleteSalary(SalarySettings? salary)
    {
        if (salary == null || salary.Id == 0) return;

        _salarySettingsService.DeleteSettings(salary.Id);
        Salaries.Remove(salary);
        CalculateTotalMonthlyIncome();
    }

    private void CalculateTotalMonthlyIncome()
    {
        decimal total = 0;
        foreach (var salary in Salaries)
        {
            total += salary.AnnualHours > 0
                ? (salary.AnnualIncome / salary.AnnualHours) * 160m
                : 0;
        }
        TotalMonthlyIncome = total;
    }

    private void CreateRecurringTransaction(SalarySettings? salary)
    {
        if (salary == null) return;

        // Calculate monthly salary amount
        var monthlyAmount = salary.AnnualHours > 0
            ? (salary.AnnualIncome / salary.AnnualHours) * 160m
            : 0;

        if (monthlyAmount <= 0)
        {
            // Could show a message to user, but for now just return
            return;
        }

        // Get "Lön" category (id=10 from seed data)
        var lonCategory = _categoryService.GetAllCategories()
            .FirstOrDefault(c => c.Name == "Lön" && c.Type == TransactionType.Income);

        if (lonCategory == null)
        {
            // Shouldn't happen with seed data, but handle gracefully
            return;
        }

        // Check if a recurring transaction already exists for this salary
        var expectedDescription = $"Månadslön - {salary.Note}";
        var existingTransaction = _recurringTransactionService.GetAllRecurringTransactions()
            .FirstOrDefault(rt => rt.Description == expectedDescription && rt.IsActive);

        if (existingTransaction != null)
        {
            // Transaction already exists, don't create duplicate
            return;
        }

        // Create recurring transaction
        var recurringTransaction = new RecurringTransaction
        {
            Description = expectedDescription,
            Amount = monthlyAmount,
            CategoryId = lonCategory.Id,
            Type = TransactionType.Income,
            IsActive = true,
            RecurrenceType = RecurrenceType.Monthly,
            StartDate = DateTime.Now.Date
        };

        _recurringTransactionService.AddRecurringTransaction(recurringTransaction);

        // Process the recurring transaction for the current month so it shows up immediately
        _recurringTransactionService.ProcessRecurringTransactionsForMonth(DateTime.Now);

        // Notify user or refresh UI if needed
        // For now, the user can see it in the RecurringTransactionView
    }
}
