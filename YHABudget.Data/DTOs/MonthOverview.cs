namespace YHABudget.Data.DTOs;

public class MonthOverview
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetBalance { get; set; }
    public decimal AccountBalance { get; set; }
    public List<CategorySummary> IncomeByCategory { get; set; } = new();
    public List<CategorySummary> ExpensesByCategory { get; set; } = new();
    public List<DateTime> AvailableMonths { get; set; } = new();
}
