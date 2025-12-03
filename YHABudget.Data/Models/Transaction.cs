using System.ComponentModel;
using System.Runtime.CompilerServices;
using YHABudget.Data.Enums;

namespace YHABudget.Data.Models;

public class Transaction : IEquatable<Transaction>, INotifyPropertyChanged
{
    private decimal _amount;
    private string _description = string.Empty;
    private DateTime _date;
    private int _categoryId;
    private TransactionType _type;

    public event PropertyChangedEventHandler? PropertyChanged;

    public int Id { get; set; }
    
    public decimal Amount
    {
        get => _amount;
        set
        {
            if (_amount != value)
            {
                _amount = value;
                OnPropertyChanged();
            }
        }
    }
    
    public string Description
    {
        get => _description;
        set
        {
            if (_description != value)
            {
                _description = value;
                OnPropertyChanged();
            }
        }
    }
    
    public DateTime Date
    {
        get => _date;
        set
        {
            if (_date != value)
            {
                _date = value;
                OnPropertyChanged();
            }
        }
    }
    
    public int CategoryId
    {
        get => _categoryId;
        set
        {
            if (_categoryId != value)
            {
                _categoryId = value;
                OnPropertyChanged();
            }
        }
    }
    
    public TransactionType Type
    {
        get => _type;
        set
        {
            if (_type != value)
            {
                _type = value;
                OnPropertyChanged();
            }
        }
    }
    
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

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}