namespace YHABudget.Data.DTOs;

public class ScheduledTransactionSummary
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}
