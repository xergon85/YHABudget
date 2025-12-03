namespace YHABudget.Data.DTOs;

public class ExpectedMonthResult
{
    public decimal ScheduledIncome { get; set; }
    public decimal ScheduledExpenses { get; set; }
    public decimal CurrentNetBalance { get; set; }
    public decimal ProjectedNetBalance { get; set; }
    public bool IsCurrentMonth { get; set; }
}
