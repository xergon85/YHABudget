using YHABudget.Data.Enums;

namespace YHABudget.Data.Models;

public class RecurringTransaction : IEquatable<RecurringTransaction>
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

    public bool Equals(RecurringTransaction? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        
        return Id == other.Id &&
               Description == other.Description &&
               Amount == other.Amount &&
               CategoryId == other.CategoryId &&
               Type == other.Type &&
               RecurrenceType == other.RecurrenceType &&
               RecurrenceMonth == other.RecurrenceMonth &&
               StartDate == other.StartDate &&
               EndDate == other.EndDate &&
               IsActive == other.IsActive;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as RecurringTransaction);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Id);
        hash.Add(Description);
        hash.Add(Amount);
        hash.Add(CategoryId);
        hash.Add(Type);
        hash.Add(RecurrenceType);
        hash.Add(RecurrenceMonth);
        hash.Add(StartDate);
        hash.Add(EndDate);
        hash.Add(IsActive);
        return hash.ToHashCode();
    }
}
