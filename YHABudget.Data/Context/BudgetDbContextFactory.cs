using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace YHABudget.Data.Context;

public class BudgetDbContextFactory : IDesignTimeDbContextFactory<BudgetDbContext>
{
    public BudgetDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BudgetDbContext>();
        
        // Use a default connection string for migrations
        optionsBuilder.UseSqlite("Data Source=budget.db");
        
        return new BudgetDbContext(optionsBuilder.Options);
    }
}
