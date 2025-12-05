using YHABudget.Data.Context;
using YHABudget.Data.Enums;

namespace YHABudget.Data.Services.AbsenceCalculationStrategies;

public interface IAbsenceCalculationStrategy
{
    AbsenceType AbsenceType { get; }
    
    /// <summary>
    /// Calculates the daily income, deduction, and compensation for an absence
    /// </summary>
    /// <param name="context">Database context to check for previous absences</param>
    /// <param name="date">The date of the absence</param>
    /// <param name="annualIncome">Total annual income</param>
    /// <param name="annualHours">Total annual working hours</param>
    /// <returns>Tuple with (dailyIncome, deduction, compensation)</returns>
    (decimal DailyIncome, decimal Deduction, decimal Compensation) Calculate(
        BudgetDbContext context,
        DateTime date,
        decimal annualIncome, 
        decimal annualHours);
}
