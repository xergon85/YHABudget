using System.ComponentModel.DataAnnotations;
using YHABudget.Core.MVVM;
using YHABudget.Data.Enums;
using YHABudget.Data.Models;
using YHABudget.Data.Services;

namespace YHABudget.Core.ViewModels;

public class AbsenceDialogViewModel : ViewModelBase
{
    private readonly ISalarySettingsService _salarySettingsService;
    
    private int _id;
    private DateTime _date;
    private AbsenceType _type;
    private decimal _dailyIncome;
    private decimal _deduction;
    private decimal _compensation;
    private string? _note;
    private bool _isEditMode;

    public AbsenceDialogViewModel(ISalarySettingsService salarySettingsService)
    {
        _salarySettingsService = salarySettingsService;
        _date = DateTime.Now.Date;
        _type = AbsenceType.Sick;
        
        CalculateImpact();
    }

    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public DateTime Date
    {
        get => _date;
        set
        {
            if (SetProperty(ref _date, value))
            {
                CalculateImpact();
            }
        }
    }

    public AbsenceType Type
    {
        get => _type;
        set
        {
            if (SetProperty(ref _type, value))
            {
                CalculateImpact();
            }
        }
    }

    public decimal DailyIncome
    {
        get => _dailyIncome;
        set => SetProperty(ref _dailyIncome, value);
    }

    public decimal Deduction
    {
        get => _deduction;
        set => SetProperty(ref _deduction, value);
    }

    public decimal Compensation
    {
        get => _compensation;
        set => SetProperty(ref _compensation, value);
    }

    public string? Note
    {
        get => _note;
        set => SetProperty(ref _note, value);
    }

    public bool IsEditMode
    {
        get => _isEditMode;
        private set => SetProperty(ref _isEditMode, value);
    }

    public void LoadAbsence(Absence? absence)
    {
        if (absence != null)
        {
            IsEditMode = true;
            Id = absence.Id;
            Date = absence.Date;
            Type = absence.Type;
            DailyIncome = absence.DailyIncome;
            Deduction = absence.Deduction;
            Compensation = absence.Compensation;
            Note = absence.Note;
        }
        else
        {
            IsEditMode = false;
            Id = 0;
            Date = DateTime.Now.Date;
            Type = AbsenceType.Sick;
            Note = null;
            CalculateImpact();
        }
    }

    public Absence ToAbsence()
    {
        return new Absence
        {
            Id = Id,
            Date = Date,
            Type = Type,
            DailyIncome = DailyIncome,
            Deduction = Deduction,
            Compensation = Compensation,
            Note = Note
        };
    }

    private void CalculateImpact()
    {
        // Get total salary from all settings
        var salaries = _salarySettingsService.GetAllSettings();
        decimal totalAnnualIncome = 0;
        decimal totalAnnualHours = 0;

        foreach (var salary in salaries)
        {
            totalAnnualIncome += salary.AnnualIncome;
            totalAnnualHours += salary.AnnualHours;
        }

        if (totalAnnualHours <= 0)
        {
            DailyIncome = 0;
            Deduction = 0;
            Compensation = 0;
            return;
        }

        // Calculate monthly and daily income
        var monthlyIncome = (totalAnnualIncome / totalAnnualHours) * 160m;
        var dailyIncome = monthlyIncome / 22m; // Approximate 22 working days per month
        DailyIncome = dailyIncome;

        // Apply VAB cap if applicable
        decimal effectiveAnnualIncome = totalAnnualIncome;
        const decimal VAB_CAP = 410_000m; // 7.5 PBB

        if (Type == AbsenceType.VAB && totalAnnualIncome > VAB_CAP)
        {
            effectiveAnnualIncome = VAB_CAP;
            var cappedMonthlyIncome = (effectiveAnnualIncome / totalAnnualHours) * 160m;
            dailyIncome = cappedMonthlyIncome / 22m;
        }

        Deduction = dailyIncome;
        Compensation = dailyIncome * 0.80m; // 80% compensation
    }
}
