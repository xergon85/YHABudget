using System.ComponentModel.DataAnnotations;

namespace YHABudget.Data.Models;

public class SalarySettings
{
    [Key]
    public int Id { get; set; }

    [Required]
    [Range(0, 10_000_000, ErrorMessage = "Årsinkomst måste vara mellan 0 och 10 000 000 kr")]
    public decimal AnnualIncome { get; set; }

    [Required]
    [Range(0, 8760, ErrorMessage = "Årsarbetstid måste vara mellan 0 och 8760 timmar")]
    public decimal AnnualHours { get; set; }

    [Required]
    [MaxLength(200)]
    public string Note { get; set; } = string.Empty;

    [Required]
    public DateTime UpdatedAt { get; set; }
}
