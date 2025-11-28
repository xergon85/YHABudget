using YHABudget.Data.Enums;

namespace YHABudget.Data.Models;

public class Transaction
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int CategoryId { get; set; }
    public TransactionType Type { get; set; }
    public bool IsRecurring { get; set; }
    
    // Navigation property
    public Category? Category { get; set; }
}
