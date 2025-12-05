using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using YHABudget.Data.Context;
using YHABudget.Data.Services;

namespace YHABudget.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddDataServices(this IServiceCollection services, string connectionString)
    {
        // Register DbContext as Transient to avoid caching issues in Singleton ViewModels
        // Each service call will get a fresh DbContext instance
        services.AddDbContext<BudgetDbContext>(
            options => options.UseSqlite(connectionString),
            contextLifetime: ServiceLifetime.Transient,
            optionsLifetime: ServiceLifetime.Transient);

        // Register Data Services as Transient to allow use in Singleton ViewModels
        services.AddTransient<TransactionService>();
        services.AddTransient<CategoryService>();
        services.AddTransient<RecurringTransactionService>();
        services.AddTransient<CalculationService>();
        services.AddTransient<SalarySettingsService>();
        services.AddTransient<AbsenceService>();

        return services;
    }
}
