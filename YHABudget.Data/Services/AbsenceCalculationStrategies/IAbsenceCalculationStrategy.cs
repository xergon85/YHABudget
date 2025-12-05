using YHABudget.Data.Enums;

namespace YHABudget.Data.Services.AbsenceCalculationStrategies;

public interface IAbsenceCalculationStrategy
{
    AbsenceType AbsenceType { get; }
    
    /// <summary>
    /// Calculates the daily income, deduction, and compensation for an absence
    /// </summary>
    /// <param name="annualIncome">Total annual income</param>
    /// <param name="annualHours">Total annual working hours</param>
    /// <returns>Tuple with (dailyIncome, deduction, compensation)</returns>
    (decimal DailyIncome, decimal Deduction, decimal Compensation) Calculate(
        decimal annualIncome, 
        decimal annualHours);
}
