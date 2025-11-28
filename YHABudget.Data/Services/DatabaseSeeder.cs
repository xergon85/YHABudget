using YHABudget.Data.Context;
using YHABudget.Data.Enums;
using YHABudget.Data.Models;

namespace YHABudget.Data.Services;

public static class DatabaseSeeder
{
    public static void SeedDatabase(BudgetDbContext context)
    {
        // Check if database already has transactions
        if (context.Transactions.Any())
        {
            return; // Database already seeded
        }

        // Categories are already seeded via OnModelCreating
        // Use the predefined IDs:
        // 1=Mat, 2=Hus & drift, 3=Transport, 4=Fritid, 5=Barn, 
        // 6=Streaming-tjänster, 7=SaaS-produkter, 8=Försäkring, 9=Teknik
        // 10=Lön, 11=Bidrag, 12=Hobbyverksamhet

        // Seed Transactions
        var today = DateTime.Now;
        var transactions = new[]
        {
            // Income
            new Transaction
            {
                Description = "Månadslön",
                Amount = 35000,
                Date = new DateTime(today.Year, today.Month, 25),
                Type = TransactionType.Income,
                CategoryId = 10 // Lön
            },
            new Transaction
            {
                Description = "Månadslön",
                Amount = 35000,
                Date = new DateTime(today.Year, today.Month - 1, 25),
                Type = TransactionType.Income,
                CategoryId = 10 // Lön
            },
            
            // Expenses - Current month
            new Transaction
            {
                Description = "ICA Maxi",
                Amount = 1250,
                Date = new DateTime(today.Year, today.Month, 5),
                Type = TransactionType.Expense,
                CategoryId = 1 // Mat
            },
            new Transaction
            {
                Description = "Coop",
                Amount = 780,
                Date = new DateTime(today.Year, today.Month, 12),
                Type = TransactionType.Expense,
                CategoryId = 1 // Mat
            },
            new Transaction
            {
                Description = "Willys",
                Amount = 650,
                Date = new DateTime(today.Year, today.Month, 18),
                Type = TransactionType.Expense,
                CategoryId = 1 // Mat
            },
            new Transaction
            {
                Description = "SL-kort",
                Amount = 970,
                Date = new DateTime(today.Year, today.Month, 1),
                Type = TransactionType.Expense,
                CategoryId = 3 // Transport
            },
            new Transaction
            {
                Description = "Bensin",
                Amount = 550,
                Date = new DateTime(today.Year, today.Month, 10),
                Type = TransactionType.Expense,
                CategoryId = 3 // Transport
            },
            new Transaction
            {
                Description = "Hyra",
                Amount = 12000,
                Date = new DateTime(today.Year, today.Month, 1),
                Type = TransactionType.Expense,
                CategoryId = 2 // Hus & drift
            },
            new Transaction
            {
                Description = "El",
                Amount = 850,
                Date = new DateTime(today.Year, today.Month, 15),
                Type = TransactionType.Expense,
                CategoryId = 2 // Hus & drift
            },
            new Transaction
            {
                Description = "Bio",
                Amount = 280,
                Date = new DateTime(today.Year, today.Month, 8),
                Type = TransactionType.Expense,
                CategoryId = 4 // Fritid
            },
            new Transaction
            {
                Description = "Restaurang",
                Amount = 450,
                Date = new DateTime(today.Year, today.Month, 14),
                Type = TransactionType.Expense,
                CategoryId = 4 // Fritid
            },
            new Transaction
            {
                Description = "Netflix",
                Amount = 179,
                Date = new DateTime(today.Year, today.Month, 3),
                Type = TransactionType.Expense,
                CategoryId = 6 // Streaming-tjänster
            },
            new Transaction
            {
                Description = "Spotify",
                Amount = 119,
                Date = new DateTime(today.Year, today.Month, 3),
                Type = TransactionType.Expense,
                CategoryId = 6 // Streaming-tjänster
            },
            new Transaction
            {
                Description = "Hemförsäkring",
                Amount = 250,
                Date = new DateTime(today.Year, today.Month, 1),
                Type = TransactionType.Expense,
                CategoryId = 8 // Försäkring
            },
            
            // Expenses - Last month
            new Transaction
            {
                Description = "ICA Maxi",
                Amount = 1100,
                Date = new DateTime(today.Year, today.Month - 1, 8),
                Type = TransactionType.Expense,
                CategoryId = 1 // Mat
            },
            new Transaction
            {
                Description = "Hyra",
                Amount = 12000,
                Date = new DateTime(today.Year, today.Month - 1, 1),
                Type = TransactionType.Expense,
                CategoryId = 2 // Hus & drift
            },
            new Transaction
            {
                Description = "SL-kort",
                Amount = 970,
                Date = new DateTime(today.Year, today.Month - 1, 1),
                Type = TransactionType.Expense,
                CategoryId = 3 // Transport
            }
        };

        context.Transactions.AddRange(transactions);
        context.SaveChanges();
    }
}
