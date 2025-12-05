using YHABudget.Data.Context;
using YHABudget.Data.Enums;
using YHABudget.Data.Models;

namespace YHABudget.Data.Services;

public class AbsenceService : IAbsenceService
{
    private readonly BudgetDbContext _context;
    private readonly ISalarySettingsService _salarySettingsService;
    private const decimal VAB_CAP_ANNUAL_INCOME = 410_000m; // 7.5 PBB (Prisbasbelopp)
    private const decimal COMPENSATION_RATE = 0.80m; // 80% compensation

    public AbsenceService(BudgetDbContext context, ISalarySettingsService salarySettingsService)
    {
        _context = context;
        _salarySettingsService = salarySettingsService;
    }

    public Absence AddAbsence(Absence absence)
    {
        absence.CreatedAt = DateTime.Now;
        _context.Absences.Add(absence);
        _context.SaveChanges();
        
        // Create deduction transaction in the same month as the absence
        CreateDeductionTransaction(absence);
        
        // Create compensation transaction in the next month
        CreateCompensationTransaction(absence);
        
        return absence;
    }

    private void CreateDeductionTransaction(Absence absence)
    {
        // Only create deduction transaction if there is a deduction
        if (absence.Deduction <= 0)
            return;

        // Get or create "Lön" category for income/deductions
        var lonCategory = _context.Categories.FirstOrDefault(c => c.Name == "Lön" && c.Type == TransactionType.Income);
        if (lonCategory == null)
        {
            lonCategory = new Category { Name = "Lön", Type = TransactionType.Income };
            _context.Categories.Add(lonCategory);
            _context.SaveChanges();
        }

        var deductionTransaction = new Transaction
        {
            Description = $"Frånvaro avdrag - {absence.Type} ({absence.Date:yyyy-MM-dd})",
            Amount = absence.Deduction,
            Date = absence.Date, // Same month as absence
            Type = TransactionType.Expense,
            CategoryId = lonCategory.Id
        };

        _context.Transactions.Add(deductionTransaction);
        _context.SaveChanges();
    }

    private void CreateCompensationTransaction(Absence absence)
    {
        // Only create compensation transaction if there is compensation
        if (absence.Compensation <= 0)
            return;

        // Get or create "Lön" category
        var lonCategory = _context.Categories.FirstOrDefault(c => c.Name == "Lön" && c.Type == TransactionType.Income);
        if (lonCategory == null)
        {
            lonCategory = new Category { Name = "Lön", Type = TransactionType.Income };
            _context.Categories.Add(lonCategory);
            _context.SaveChanges();
        }

        // Compensation is paid in the next month
        var nextMonth = absence.Date.AddMonths(1);
        var compensationDate = new DateTime(nextMonth.Year, nextMonth.Month, 25); // Typical salary payment date

        var compensationTransaction = new Transaction
        {
            Description = $"Frånvaro ersättning - {absence.Type} ({absence.Date:yyyy-MM-dd})",
            Amount = absence.Compensation,
            Date = compensationDate,
            Type = TransactionType.Income,
            CategoryId = lonCategory.Id
        };

        _context.Transactions.Add(compensationTransaction);
        _context.SaveChanges();
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
            // Find and delete associated transactions
            // Deduction transaction is in the same month as absence
            var deductionTransactions = _context.Transactions
                .Where(t => t.Type == TransactionType.Expense &&
                           t.Description.Contains($"Frånvaro avdrag - {absence.Type} ({absence.Date:yyyy-MM-dd})"))
                .ToList();
            
            // Compensation transaction is in the next month
            var compensationTransactions = _context.Transactions
                .Where(t => t.Type == TransactionType.Income &&
                           t.Description.Contains($"Frånvaro ersättning - {absence.Type} ({absence.Date:yyyy-MM-dd})"))
                .ToList();

            _context.Transactions.RemoveRange(deductionTransactions);
            _context.Transactions.RemoveRange(compensationTransactions);
            _context.Absences.Remove(absence);
            _context.SaveChanges();
        }
    }

    public (decimal DailyIncome, decimal Deduction, decimal Compensation) CalculateAbsenceImpact(
        DateTime date,
        AbsenceType type)
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

        return CalculateAbsenceImpact(date, type, totalAnnualIncome, totalAnnualHours);
    }

    public (decimal DailyIncome, decimal Deduction, decimal Compensation) CalculateAbsenceImpact(
        DateTime date,
        AbsenceType type,
        decimal annualIncome,
        decimal annualHours)
    {
        if (annualHours <= 0)
        {
            return (0, 0, 0);
        }

        // Calculate daily income based on 160 hours per month
        var monthlyIncome = (annualIncome / annualHours) * 160m;
        var dailyIncome = monthlyIncome / 22m; // Approximate 22 working days per month

        // Deduction is always 80% of actual salary (employee loses this from their pay)
        decimal deduction = dailyIncome * COMPENSATION_RATE;

        // Compensation is capped at 410,000 kr annual income
        decimal compensationDailyIncome = dailyIncome;
        if (annualIncome > VAB_CAP_ANNUAL_INCOME)
        {
            var cappedMonthlyIncome = (VAB_CAP_ANNUAL_INCOME / annualHours) * 160m;
            compensationDailyIncome = cappedMonthlyIncome / 22m;
        }

        // Calculate 80% compensation based on capped income
        decimal compensation = compensationDailyIncome * COMPENSATION_RATE;

        return (dailyIncome, deduction, compensation);
    }
}
