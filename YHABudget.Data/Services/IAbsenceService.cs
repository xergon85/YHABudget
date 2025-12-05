using YHABudget.Data.Enums;
using YHABudget.Data.Models;

namespace YHABudget.Data.Services;

public interface IAbsenceService
{
    Absence AddAbsence(Absence absence);
    IEnumerable<Absence> GetAllAbsences();
    IEnumerable<Absence> GetAbsencesForMonth(DateTime month);
    Absence? GetAbsenceById(int id);
    void UpdateAbsence(Absence absence);
    void DeleteAbsence(int id);
    
    /// <summary>
    /// Calculates all absence impact values based on salary settings
    /// </summary>
    /// <param name="date">Date of absence</param>
    /// <param name="type">Type of absence (VAB or Sick)</param>
    /// <param name="annualIncome">Annual income from salary settings</param>
    /// <param name="annualHours">Annual hours from salary settings</param>
    /// <returns>Tuple with (dailyIncome, deduction, compensation)</returns>
    (decimal DailyIncome, decimal Deduction, decimal Compensation) CalculateAbsenceImpact(
        DateTime date, 
        AbsenceType type);
    
    (decimal DailyIncome, decimal Deduction, decimal Compensation) CalculateAbsenceImpact(
        DateTime date, 
        AbsenceType type, 
        decimal annualIncome, 
        decimal annualHours);
}
