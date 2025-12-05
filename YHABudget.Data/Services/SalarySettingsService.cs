using YHABudget.Data.Context;
using YHABudget.Data.Models;

namespace YHABudget.Data.Services;

public class SalarySettingsService : ISalarySettingsService
{
    private readonly BudgetDbContext _context;

    public SalarySettingsService(BudgetDbContext context)
    {
        _context = context;
    }

    public List<SalarySettings> GetAllSettings()
    {
        return _context.SalarySettings.OrderBy(s => s.Id).ToList();
    }

    public SalarySettings? GetSettingsById(int id)
    {
        return _context.SalarySettings.FirstOrDefault(s => s.Id == id);
    }

    public SalarySettings AddSettings(SalarySettings settings)
    {
        settings.UpdatedAt = DateTime.Now;
        _context.SalarySettings.Add(settings);
        _context.SaveChanges();
        return settings;
    }

    public void UpdateSettings(SalarySettings settings)
    {
        settings.UpdatedAt = DateTime.Now;

        var existingSettings = _context.SalarySettings.FirstOrDefault(s => s.Id == settings.Id);

        if (existingSettings != null)
        {
            existingSettings.AnnualIncome = settings.AnnualIncome;
            existingSettings.AnnualHours = settings.AnnualHours;
            existingSettings.Note = settings.Note;
            existingSettings.UpdatedAt = settings.UpdatedAt;
            _context.SaveChanges();
        }
    }

    public void DeleteSettings(int id)
    {
        var settings = _context.SalarySettings.FirstOrDefault(s => s.Id == id);
        if (settings != null)
        {
            _context.SalarySettings.Remove(settings);
            _context.SaveChanges();
        }
    }
}
