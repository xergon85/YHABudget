using System.Collections.ObjectModel;
using System.Windows.Input;
using YHABudget.Core.Commands;
using YHABudget.Core.MVVM;
using YHABudget.Data.Models;
using YHABudget.Data.Services;

namespace YHABudget.Core.ViewModels;

public class SalaryViewModel : ViewModelBase
{
    private readonly ISalarySettingsService _salarySettingsService;
    private readonly ICalculationService _calculationService;

    private ObservableCollection<SalarySettings> _salaries;
    private SalarySettings? _selectedSalary;
    private decimal _totalMonthlyIncome;

    public SalaryViewModel(ISalarySettingsService salarySettingsService, ICalculationService calculationService)
    {
        _salarySettingsService = salarySettingsService;
        _calculationService = calculationService;

        _salaries = new ObservableCollection<SalarySettings>();

        LoadDataCommand = new RelayCommand(() => LoadData());
        AddSalaryCommand = new RelayCommand(() => AddSalary());
        SaveSalaryCommand = new RelayCommand(() => SaveSalary(), () => SelectedSalary != null);
        DeleteSalaryCommand = new RelayCommand(() => DeleteSalary(), () => SelectedSalary != null);
        CancelEditCommand = new RelayCommand(() => CancelEdit());

        LoadData();
    }

    public ObservableCollection<SalarySettings> Salaries
    {
        get => _salaries;
        private set => SetProperty(ref _salaries, value);
    }

    public SalarySettings? SelectedSalary
    {
        get => _selectedSalary;
        set
        {
            if (SetProperty(ref _selectedSalary, value))
            {
                ((RelayCommand)SaveSalaryCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteSalaryCommand).RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(IsEditing));
            }
        }
    }

    public decimal TotalMonthlyIncome
    {
        get => _totalMonthlyIncome;
        private set => SetProperty(ref _totalMonthlyIncome, value);
    }

    public bool IsEditing => SelectedSalary != null;

    public ICommand LoadDataCommand { get; }
    public ICommand AddSalaryCommand { get; }
    public ICommand SaveSalaryCommand { get; }
    public ICommand DeleteSalaryCommand { get; }
    public ICommand CancelEditCommand { get; }

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
        var newSalary = new SalarySettings
        {
            AnnualIncome = 0,
            AnnualHours = 0,
            Note = string.Empty,
            UpdatedAt = DateTime.Now
        };

        SelectedSalary = newSalary;
    }

    private void SaveSalary()
    {
        if (SelectedSalary == null) return;

        if (SelectedSalary.Id == 0)
        {
            // New salary - add to database
            var added = _salarySettingsService.AddSettings(SelectedSalary);
            Salaries.Add(added);
        }
        else
        {
            // Existing salary - update
            _salarySettingsService.UpdateSettings(SelectedSalary);

            // Refresh the item in the collection
            var index = Salaries.IndexOf(SelectedSalary);
            if (index >= 0)
            {
                var updated = _salarySettingsService.GetSettingsById(SelectedSalary.Id);
                if (updated != null)
                {
                    Salaries[index] = updated;
                }
            }
        }

        CalculateTotalMonthlyIncome();
        SelectedSalary = null;
    }

    private void DeleteSalary()
    {
        if (SelectedSalary == null) return;

        if (SelectedSalary.Id > 0)
        {
            _salarySettingsService.DeleteSettings(SelectedSalary.Id);
            Salaries.Remove(SelectedSalary);
            CalculateTotalMonthlyIncome();
        }

        SelectedSalary = null;
    }

    private void CancelEdit()
    {
        SelectedSalary = null;
    }

    private void CalculateTotalMonthlyIncome()
    {
        decimal total = 0;
        foreach (var salary in Salaries)
        {
            if (salary.AnnualHours > 0)
            {
                total += _calculationService.CalculateMonthlyIncome(salary.AnnualIncome, salary.AnnualHours);
            }
        }
        TotalMonthlyIncome = total;
    }
}
