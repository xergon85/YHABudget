using YHABudget.Data.Models;

namespace YHABudget.Core.Services;

public interface ISalarySettingsService
{
    SalarySettings GetSettings();
    void UpdateSettings(SalarySettings settings);
}
