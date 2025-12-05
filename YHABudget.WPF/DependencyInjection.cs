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

        // Map concrete services to interfaces (Transient to match Data layer)
        services.AddTransient<ITransactionService>(sp => sp.GetRequiredService<TransactionService>());
        services.AddTransient<ICategoryService>(sp => sp.GetRequiredService<CategoryService>());
        services.AddTransient<IRecurringTransactionService>(sp => sp.GetRequiredService<RecurringTransactionService>());
        services.AddTransient<ICalculationService>(sp => sp.GetRequiredService<CalculationService>());
        services.AddTransient<ISalarySettingsService>(sp => sp.GetRequiredService<SalarySettingsService>());

        // Register WPF Services
        services.AddTransient<IDialogService, DialogService>();

        // Register Main ViewModels (Singleton - one instance per app lifetime)
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<OverviewViewModel>();
        services.AddSingleton<TransactionViewModel>();
        services.AddSingleton<RecurringTransactionViewModel>();
        services.AddSingleton<SalaryViewModel>();

        // Register Dialog ViewModels (Transient - new instance per dialog)
        services.AddTransient<TransactionDialogViewModel>();
        services.AddTransient<RecurringTransactionDialogViewModel>();
        services.AddTransient<SalaryDialogViewModel>();

        // Register MainWindow
        services.AddSingleton<MainWindow>();

        return services;
    }
}
