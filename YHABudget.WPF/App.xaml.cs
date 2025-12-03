using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using YHABudget.Core.Services;
using YHABudget.Core.ViewModels;
using YHABudget.Data.Context;
using YHABudget.Data.Services;
using YHABudget.WPF.Services;

namespace YHABudget.WPF;

public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Register DbContext with SQLite
                services.AddDbContext<BudgetDbContext>(options =>
                    options.UseSqlite("Data Source=budget.db"));

                // Register Services
                services.AddScoped<ITransactionService, TransactionService>();
                services.AddScoped<ICategoryService, CategoryService>();
                services.AddScoped<IRecurringTransactionService, RecurringTransactionService>();
                services.AddScoped<ICalculationService, CalculationService>();
                services.AddScoped<IDialogService, DialogService>();

                // Register ViewModels
                services.AddSingleton<MainViewModel>();
                services.AddSingleton<OverviewViewModel>();
                services.AddSingleton<TransactionViewModel>();
                services.AddSingleton<RecurringTransactionViewModel>();
                services.AddSingleton<SettingsViewModel>();

                // Register MainWindow
                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        // Ensure database is created and seeded
        using (var scope = _host.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<BudgetDbContext>();
            dbContext.Database.EnsureCreated();
            DatabaseSeeder.SeedDatabase(dbContext);
        }

        // Show main window
        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();

        base.OnExit(e);
    }
}
