using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using YHABudget.Data.Context;
using YHABudget.Data.Services;

namespace YHABudget.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddDataServices(this IServiceCollection services, string connectionString)
    {
        // Register DbContext - Data layer controls the provider
        services.AddDbContext<BudgetDbContext>(options =>
            options.UseSqlite(connectionString));

        // Register Data Services
        services.AddScoped<TransactionService>();
        services.AddScoped<CategoryService>();
        services.AddScoped<RecurringTransactionService>();
        services.AddScoped<CalculationService>();

        return services;
    }
}
