using YHABudget.Data.DTOs;
using YHABudget.Data.Models;

namespace YHABudget.Data.Services;

public interface ICalculationService
{
    decimal CalculateMonthlyIncome(decimal annualIncome, decimal annualHours);
    Task<List<Transaction>> GenerateTransactionsFromRecurring(DateTime month);
    ExpectedMonthResult CalculateExpectedMonthResult(DateTime selectedMonth);
    MonthOverview GetMonthOverview(DateTime selectedMonth);
}
