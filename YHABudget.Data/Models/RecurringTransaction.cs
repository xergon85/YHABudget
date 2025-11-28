using YHABudget.Data.Enums;

namespace YHABudget.Data.Models;

public class RecurringTransaction
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int CategoryId { get; set; }
    public TransactionType Type { get; set; }
    public RecurrenceType RecurrenceType { get; set; }
    public int? RecurrenceMonth { get; set; } // Note: 1-12, only for Yearly recurrence
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }

    // Navigation property
    public Category? Category { get; set; }
}
