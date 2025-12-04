using Microsoft.Extensions.DependencyInjection;
using YHABudget.Core.Services;
using YHABudget.Core.ViewModels;
using YHABudget.Data;
using YHABudget.Data.Services;
using YHABudget.WPF.Services;

namespace YHABudget.WPF;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, string connectionString)
    {
        // Register Data Layer (infrastructure concerns)
        services.AddDataServices(connectionString);

        // Map concrete services to interfaces
        services.AddScoped<ITransactionService>(sp => sp.GetRequiredService<TransactionService>());
        services.AddScoped<ICategoryService>(sp => sp.GetRequiredService<CategoryService>());
        services.AddScoped<IRecurringTransactionService>(sp => sp.GetRequiredService<RecurringTransactionService>());
        services.AddScoped<ICalculationService>(sp => sp.GetRequiredService<CalculationService>());

        // Register WPF Services
        services.AddScoped<IDialogService, DialogService>();

        // Register Main ViewModels (Singleton - one instance per app lifetime)
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<OverviewViewModel>();
        services.AddSingleton<TransactionViewModel>();
        services.AddSingleton<RecurringTransactionViewModel>();
        services.AddSingleton<SalaryViewModel>();

        // Register Dialog ViewModels (Transient - new instance per dialog)
        services.AddTransient<TransactionDialogViewModel>();
        services.AddTransient<RecurringTransactionDialogViewModel>();

        // Register MainWindow
        services.AddSingleton<MainWindow>();

        return services;
    }
}
