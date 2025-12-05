using YHABudget.Data.Context;
using YHABudget.Data.Enums;

namespace YHABudget.Data.Services.AbsenceCalculationStrategies;

/// <summary>
/// Calculation strategy for sick leave (Sjukfrånvaro)
/// - Karensdag (first sick day): Full deduction (100%), no compensation
/// - Subsequent days: 80% deduction, 80% compensation (capped at 410,000 kr/year)
/// - High earners take a bigger financial hit
/// </summary>
public class SickLeaveCalculationStrategy : IAbsenceCalculationStrategy
{
    private const decimal INCOME_CAP = 410_000m; // 7.5 PBB (Prisbasbelopp)
    private const decimal COMPENSATION_RATE = 0.80m; // 80%

    public AbsenceType AbsenceType => AbsenceType.Sick;

    public (decimal DailyIncome, decimal Deduction, decimal Compensation) Calculate(
        BudgetDbContext context,
        DateTime date,
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

        // Check if this is the first sick day (karensdag)
        // Look for any sick leave absence in the 7 days before this date
        var sevenDaysAgo = date.AddDays(-7);
        var hasPreviousSickLeave = context.Absences
            .Any(a => a.Type == AbsenceType.Sick && 
                     a.Date >= sevenDaysAgo && 
                     a.Date < date);

        decimal deduction;
        decimal compensation;
        
        if (!hasPreviousSickLeave)
        {
            // This is the first sick day - karensdag (waiting day)
            // Full deduction (100% of daily salary), no compensation
            deduction = dailyIncome;
            compensation = 0m;
        }
        else
        {
            // Not the first day - normal sick leave
            // Deduction is 80% of actual salary (employee loses this from their pay)
            deduction = dailyIncome * COMPENSATION_RATE;
            
            // Compensation is capped at 410,000 kr annual income
            var compensationDailyIncome = dailyIncome;
            if (annualIncome > INCOME_CAP)
            {
                var cappedMonthlyIncome = (INCOME_CAP / annualHours) * 160m;
                compensationDailyIncome = cappedMonthlyIncome / 22m;
            }

            // Calculate 80% compensation based on capped income
            compensation = compensationDailyIncome * COMPENSATION_RATE;
        }

        return (dailyIncome, deduction, compensation);
    }
}
