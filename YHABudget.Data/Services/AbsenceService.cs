using YHABudget.Data.Context;
using YHABudget.Data.Enums;
using YHABudget.Data.Models;
using YHABudget.Data.Services.AbsenceCalculationStrategies;

namespace YHABudget.Data.Services;

public class AbsenceService : IAbsenceService
{
    private readonly BudgetDbContext _context;
    private readonly ISalarySettingsService _salarySettingsService;
    private readonly AbsenceCalculationStrategyFactory _strategyFactory;

    public AbsenceService(BudgetDbContext context, ISalarySettingsService salarySettingsService)
    {
        _context = context;
        _salarySettingsService = salarySettingsService;
        _strategyFactory = new AbsenceCalculationStrategyFactory();
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

        // Get or create absence-specific category for deductions
        var categoryName = absence.Type == AbsenceType.Sick ? "Sjukfrånvaro" : "VAB-frånvaro";
        var absenceCategory = _context.Categories.FirstOrDefault(c => c.Name == categoryName && c.Type == TransactionType.Expense);
        if (absenceCategory == null)
        {
            absenceCategory = new Category { Name = categoryName, Type = TransactionType.Expense };
            _context.Categories.Add(absenceCategory);
            _context.SaveChanges();
        }

        var deductionTransaction = new Transaction
        {
            Description = $"Frånvaro avdrag ({absence.Date:yyyy-MM-dd})",
            Amount = absence.Deduction,
            Date = absence.Date, // Same month as absence
            Type = TransactionType.Expense,
            CategoryId = absenceCategory.Id
        };

        _context.Transactions.Add(deductionTransaction);
        _context.SaveChanges();
    }

    private void CreateCompensationTransaction(Absence absence)
    {
        // Only create compensation transaction if there is compensation
        if (absence.Compensation <= 0)
            return;

        // Get or create absence-specific income category for compensation
        var categoryName = absence.Type == AbsenceType.Sick ? "Sjuklön" : "VAB";
        var compensationCategory = _context.Categories.FirstOrDefault(c => c.Name == categoryName && c.Type == TransactionType.Income);
        if (compensationCategory == null)
        {
            compensationCategory = new Category { Name = categoryName, Type = TransactionType.Income };
            _context.Categories.Add(compensationCategory);
            _context.SaveChanges();
        }

        // Compensation is paid in the next month
        var nextMonth = absence.Date.AddMonths(1);
        var compensationDate = new DateTime(nextMonth.Year, nextMonth.Month, 25); // Typical salary payment date

        var compensationTransaction = new Transaction
        {
            Description = $"Frånvaro ersättning ({absence.Date:yyyy-MM-dd})",
            Amount = absence.Compensation,
            Date = compensationDate,
            Type = TransactionType.Income,
            CategoryId = compensationCategory.Id
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
                           t.Description.Contains($"Frånvaro avdrag ({absence.Date:yyyy-MM-dd})"))
                .ToList();
            
            // Compensation transaction is in the next month
            var compensationTransactions = _context.Transactions
                .Where(t => t.Type == TransactionType.Income &&
                           t.Description.Contains($"Frånvaro ersättning ({absence.Date:yyyy-MM-dd})"))
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
        // Use strategy pattern to get the appropriate calculation logic
        var strategy = _strategyFactory.GetStrategy(type);
        return strategy.Calculate(_context, date, annualIncome, annualHours);
    }
}
