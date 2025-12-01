using YHABudget.Data.Enums;

namespace YHABudget.Data.Models;

public class Transaction : IEquatable<Transaction>
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

    public bool Equals(Transaction? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        
        return Id == other.Id &&
               Amount == other.Amount &&
               Description == other.Description &&
               Date == other.Date &&
               Type == other.Type &&
               CategoryId == other.CategoryId &&
               IsRecurring == other.IsRecurring;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Transaction);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Amount, Description, Date, Type, CategoryId, IsRecurring);
    }
}
