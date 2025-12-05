using Microsoft.EntityFrameworkCore;
using YHABudget.Data.Enums;
using YHABudget.Data.Models;

namespace YHABudget.Data.Context;

public class BudgetDbContext : DbContext
{
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<RecurringTransaction> RecurringTransactions { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<SalarySettings> SalarySettings { get; set; }

    public BudgetDbContext(DbContextOptions<BudgetDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Transaction entity
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Date).IsRequired();
            entity.Property(e => e.Type).IsRequired();

            entity.HasOne(e => e.Category)
                .WithMany(c => c.Transactions)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Category entity
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Type).IsRequired();
        });

        // Configure RecurringTransaction entity
        modelBuilder.Entity<RecurringTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.RecurrenceType).IsRequired();
            entity.Property(e => e.StartDate).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();

            entity.HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure SalarySettings entity
        modelBuilder.Entity<SalarySettings>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AnnualIncome).HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.AnnualHours).HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.Note).HasMaxLength(200).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
        });

        // Seed initial categories
        SeedCategories(modelBuilder);

        // Seed default salary settings
        SeedSalarySettings(modelBuilder);
    }

    private void SeedCategories(ModelBuilder modelBuilder)
    {
        // Expense categories
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Mat", Type = TransactionType.Expense },
            new Category { Id = 2, Name = "Hus & drift", Type = TransactionType.Expense },
            new Category { Id = 3, Name = "Transport", Type = TransactionType.Expense },
            new Category { Id = 4, Name = "Fritid", Type = TransactionType.Expense },
            new Category { Id = 5, Name = "Barn", Type = TransactionType.Expense },
            new Category { Id = 6, Name = "Streaming-tjänster", Type = TransactionType.Expense },
            new Category { Id = 7, Name = "SaaS-produkter", Type = TransactionType.Expense },
            new Category { Id = 8, Name = "Försäkring", Type = TransactionType.Expense },
            new Category { Id = 9, Name = "Teknik", Type = TransactionType.Expense },

            // Income categories
            new Category { Id = 10, Name = "Lön", Type = TransactionType.Income },
            new Category { Id = 11, Name = "Bidrag", Type = TransactionType.Income },
            new Category { Id = 12, Name = "Hobbyverksamhet", Type = TransactionType.Income }
        );
    }

    private void SeedSalarySettings(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SalarySettings>().HasData(
            new SalarySettings
            {
                Id = 1,
                AnnualIncome = 12 * 25000m,
                AnnualHours = 1920m,
                Note = "Lön",
                UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
