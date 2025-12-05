using YHABudget.Data.Enums;

namespace YHABudget.Data.Services.AbsenceCalculationStrategies;

/// <summary>
/// Calculation strategy for sick leave (Sjukfrånvaro)
/// - Deduction: 80% of actual salary (employee loses this from their pay)
/// - Compensation: 80% of capped salary at 410,000 kr/year (7.5 PBB)
/// - High earners take a bigger financial hit
/// </summary>
public class SickLeaveCalculationStrategy : IAbsenceCalculationStrategy
{
    private const decimal INCOME_CAP = 410_000m; // 7.5 PBB (Prisbasbelopp)
    private const decimal COMPENSATION_RATE = 0.80m; // 80%

    public AbsenceType AbsenceType => AbsenceType.Sick;

    public (decimal DailyIncome, decimal Deduction, decimal Compensation) Calculate(
        decimal annualIncome, 
        decimal annualHours)
    {
        if (annualHours <= 0)
        {
            return (0, 0, 0);
        }

        // Calculate daily income based on actual salary
        var monthlyIncome = (annualIncome / annualHours) * 160m; // 160 hours per month
        var dailyIncome = monthlyIncome / 22m; // Approximate 22 working days per month

        // Deduction is always 80% of actual salary (employee loses this from their pay)
        var deduction = dailyIncome * COMPENSATION_RATE;

        // Compensation is capped at 410,000 kr annual income
        var compensationDailyIncome = dailyIncome;
        if (annualIncome > INCOME_CAP)
        {
            var cappedMonthlyIncome = (INCOME_CAP / annualHours) * 160m;
            compensationDailyIncome = cappedMonthlyIncome / 22m;
        }

        // Calculate 80% compensation based on capped income
        var compensation = compensationDailyIncome * COMPENSATION_RATE;

        return (dailyIncome, deduction, compensation);
    }
}
