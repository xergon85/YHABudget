using YHABudget.Data.Models;

namespace YHABudget.Data.Services;

public interface ISalarySettingsService
{
    List<SalarySettings> GetAllSettings();
    SalarySettings? GetSettingsById(int id);
    SalarySettings AddSettings(SalarySettings settings);
    void UpdateSettings(SalarySettings settings);
    void DeleteSettings(int id);
}
