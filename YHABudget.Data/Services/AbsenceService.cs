using YHABudget.Data.Context;
using YHABudget.Data.Enums;
using YHABudget.Data.Models;

namespace YHABudget.Data.Services;

public class AbsenceService : IAbsenceService
{
    private readonly BudgetDbContext _context;
    private const decimal VAB_CAP_ANNUAL_INCOME = 410_000m; // 7.5 PBB (Prisbasbelopp)
    private const decimal COMPENSATION_RATE = 0.80m; // 80% compensation

    public AbsenceService(BudgetDbContext context)
    {
        _context = context;
    }

    public Absence AddAbsence(Absence absence)
    {
        absence.CreatedAt = DateTime.Now;
        _context.Absences.Add(absence);
        _context.SaveChanges();
        return absence;
    }

    public IEnumerable<Absence> GetAllAbsences()
    {
        return _context.Absences
            .OrderByDescending(a => a.Date)
            .ToList();
    }

    public IEnumerable<Absence> GetAbsencesForMonth(DateTime month)
    {
        var monthStart = new DateTime(month.Year, month.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

        return _context.Absences
            .Where(a => a.Date >= monthStart && a.Date <= monthEnd)
            .OrderByDescending(a => a.Date)
            .ToList();
    }

    public Absence? GetAbsenceById(int id)
    {
        return _context.Absences.Find(id);
    }

    public void UpdateAbsence(Absence absence)
    {
        var existingAbsence = _context.Absences.Find(absence.Id);
        if (existingAbsence != null)
        {
            existingAbsence.Date = absence.Date;
            existingAbsence.Type = absence.Type;
            existingAbsence.DailyIncome = absence.DailyIncome;
            existingAbsence.Deduction = absence.Deduction;
            existingAbsence.Compensation = absence.Compensation;
            existingAbsence.Note = absence.Note;
            
            _context.SaveChanges();
        }
    }

    public void DeleteAbsence(int id)
    {
        var absence = _context.Absences.Find(id);
        if (absence != null)
        {
            _context.Absences.Remove(absence);
            _context.SaveChanges();
        }
    }

    public (decimal Deduction, decimal Compensation) CalculateAbsenceImpact(
        DateTime date, 
        AbsenceType type, 
        decimal annualIncome, 
        decimal annualHours)
    {
        if (annualHours <= 0)
        {
            return (0, 0);
        }

        // Calculate daily income based on 160 hours per month
        var monthlyIncome = (annualIncome / annualHours) * 160m;
        var dailyIncome = monthlyIncome / 22m; // Approximate 22 working days per month

        decimal deduction = dailyIncome;
        decimal effectiveIncome = annualIncome;

        // Apply VAB cap if applicable
        if (type == AbsenceType.VAB && annualIncome > VAB_CAP_ANNUAL_INCOME)
        {
            effectiveIncome = VAB_CAP_ANNUAL_INCOME;
            var cappedMonthlyIncome = (effectiveIncome / annualHours) * 160m;
            var cappedDailyIncome = cappedMonthlyIncome / 22m;
            deduction = cappedDailyIncome;
        }

        // Calculate 80% compensation
        decimal compensation = deduction * COMPENSATION_RATE;

        return (deduction, compensation);
    }
}
