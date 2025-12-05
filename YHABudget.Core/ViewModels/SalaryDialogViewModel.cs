using System.ComponentModel.DataAnnotations;
using YHABudget.Core.MVVM;
using YHABudget.Data.Models;
using YHABudget.Data.Services;

namespace YHABudget.Core.ViewModels;

public class SalaryDialogViewModel : ViewModelBase
{
    private readonly ICalculationService _calculationService;
    
    private int _id;
    private decimal _annualIncome;
    private decimal _annualHours;
    private string _note = string.Empty;
    private bool _isEditMode;

    public SalaryDialogViewModel(ICalculationService calculationService)
    {
        _calculationService = calculationService;
    }

    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    [Range(0, 10_000_000, ErrorMessage = "Årsinkomst måste vara mellan 0 och 10 000 000 kr")]
    public decimal AnnualIncome
    {
        get => _annualIncome;
        set
        {
            if (SetProperty(ref _annualIncome, value))
            {
                OnPropertyChanged(nameof(MonthlyIncome));
            }
        }
    }

    [Range(0, 8760, ErrorMessage = "Årsarbetstid måste vara mellan 0 och 8760 timmar")]
    public decimal AnnualHours
    {
        get => _annualHours;
        set
        {
            if (SetProperty(ref _annualHours, value))
            {
                OnPropertyChanged(nameof(MonthlyIncome));
            }
        }
    }

    [Required(ErrorMessage = "Beskrivning krävs")]
    [MaxLength(200, ErrorMessage = "Beskrivning får vara max 200 tecken")]
    public string Note
    {
        get => _note;
        set => SetProperty(ref _note, value);
    }

    public bool IsEditMode
    {
        get => _isEditMode;
        set => SetProperty(ref _isEditMode, value);
    }

    public decimal MonthlyIncome => _calculationService.CalculateMonthlyIncome(_annualIncome, _annualHours);

    public void LoadSalary(SalarySettings? salary)
    {
        if (salary != null)
        {
            Id = salary.Id;
            AnnualIncome = salary.AnnualIncome;
            AnnualHours = salary.AnnualHours;
            Note = salary.Note;
            IsEditMode = true;
        }
        else
        {
            Id = 0;
            AnnualIncome = 0;
            AnnualHours = 0;
            Note = string.Empty;
            IsEditMode = false;
        }
    }

    public SalarySettings ToSalarySettings()
    {
        return new SalarySettings
        {
            Id = Id,
            AnnualIncome = AnnualIncome,
            AnnualHours = AnnualHours,
            Note = Note,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
