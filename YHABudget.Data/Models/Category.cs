using YHABudget.Data.Enums;

namespace YHABudget.Data.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    
    // Navigation property
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
