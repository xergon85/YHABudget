using YHABudget.Data.Enums;

namespace YHABudget.Data.Models;

public class Absence
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public AbsenceType Type { get; set; }
    public decimal DailyIncome { get; set; }
    public decimal Deduction { get; set; }
    public decimal Compensation { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}
