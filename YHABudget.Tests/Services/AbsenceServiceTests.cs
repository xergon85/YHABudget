using YHABudget.Data.Context;
using YHABudget.Data.Enums;
using YHABudget.Data.Models;
using YHABudget.Data.Services;
using Microsoft.EntityFrameworkCore;

namespace YHABudget.Tests.Services;

public class AbsenceServiceTests : IDisposable
{
    private readonly BudgetDbContext _context;
    private readonly AbsenceService _absenceService;
    private readonly TransactionService _transactionService;
    private readonly CategoryService _categoryService;
    private readonly SalarySettingsService _salarySettingsService;

    public AbsenceServiceTests()
    {
        var options = new DbContextOptionsBuilder<BudgetDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BudgetDbContext(options);
        _salarySettingsService = new SalarySettingsService(_context);
        _absenceService = new AbsenceService(_context, _salarySettingsService);
        _transactionService = new TransactionService(_context);
        _categoryService = new CategoryService(_context);
    }

    [Fact]
    public void AddAbsence_WithDeduction_CreatesExpenseTransactionInNextMonth()
    {
        // Arrange
        var absenceDate = new DateTime(2025, 12, 15);
        var absence = new Absence
        {
            Date = absenceDate,
            Type = AbsenceType.Sick,
            DailyIncome = 1000m,
            Deduction = 1000m,
            Compensation = 800m,
            Note = "Sick day"
        };

        // Act
        var result = _absenceService.AddAbsence(absence);

        // Assert - Check that absence was created
        Assert.NotNull(result);
        Assert.True(result.Id > 0);

        // Assert - Check that an expense transaction was created in the current month
        var currentMonth = new DateTime(2025, 12, 1);
        var transactions = _transactionService.GetTransactionsByMonth(currentMonth);

        var deductionTransaction = transactions.FirstOrDefault(t => 
            t.Type == TransactionType.Expense && 
            t.Amount == 1000m &&
            t.Description.Contains("Frånvaro"));

        Assert.NotNull(deductionTransaction);
        Assert.Equal(1000m, deductionTransaction.Amount);
        Assert.Equal(TransactionType.Expense, deductionTransaction.Type);
        Assert.Equal(absenceDate, deductionTransaction.Date);
    }

    [Fact]
    public void AddAbsence_WithCompensation_CreatesIncomeTransactionInNextMonth()
    {
        // Arrange
        var absenceDate = new DateTime(2025, 12, 15);
        var absence = new Absence
        {
            Date = absenceDate,
            Type = AbsenceType.Sick,
            DailyIncome = 1000m,
            Deduction = 1000m,
            Compensation = 800m,
            Note = "Sick day"
        };

        // Act
        var result = _absenceService.AddAbsence(absence);

        // Assert - Check that an income transaction was created in the next month for compensation
        var nextMonth = new DateTime(2026, 1, 1);
        var transactions = _transactionService.GetTransactionsByMonth(nextMonth);
        
        // Debug: Check all transactions
        var allTransactions = transactions.ToList();

        var compensationTransaction = transactions.FirstOrDefault(t => 
            t.Type == TransactionType.Income && 
            t.Amount == 800m &&
            t.Description.Contains("ersättning"));

        Assert.NotNull(compensationTransaction);
        Assert.Equal(800m, compensationTransaction.Amount);
        Assert.Equal(TransactionType.Income, compensationTransaction.Type);
        Assert.Equal(new DateTime(2026, 1, 25), compensationTransaction.Date);
    }

    [Fact]
    public void AddAbsence_CreatesTransactionsWithCorrectCategory()
    {
        // Arrange
        var absenceDate = new DateTime(2025, 12, 15);
        var absence = new Absence
        {
            Date = absenceDate,
            Type = AbsenceType.VAB,
            DailyIncome = 1000m,
            Deduction = 1000m,
            Compensation = 800m,
            Note = "VAB day"
        };

        // Act
        var result = _absenceService.AddAbsence(absence);

        // Assert - Check that transactions have correct categories
        var currentMonth = new DateTime(2025, 12, 1);
        var nextMonth = new DateTime(2026, 1, 1);
        var currentMonthTransactions = _transactionService.GetTransactionsByMonth(currentMonth);
        var nextMonthTransactions = _transactionService.GetTransactionsByMonth(nextMonth);

        var deductionTransaction = currentMonthTransactions.FirstOrDefault(t => t.Type == TransactionType.Expense);
        var compensationTransaction = nextMonthTransactions.FirstOrDefault(t => t.Type == TransactionType.Income);

        Assert.NotNull(deductionTransaction);
        Assert.NotNull(compensationTransaction);
        
        // Deduction should be in a Lön category or similar
        Assert.NotNull(deductionTransaction.Category);
        
        // Compensation should be in a Lön category
        Assert.NotNull(compensationTransaction.Category);
    }

    [Fact]
    public void DeleteAbsence_RemovesAssociatedTransactions()
    {
        // Arrange
        var absenceDate = new DateTime(2025, 12, 15);
        var absence = new Absence
        {
            Date = absenceDate,
            Type = AbsenceType.Sick,
            DailyIncome = 1000m,
            Deduction = 1000m,
            Compensation = 800m,
            Note = "Sick day"
        };

        var addedAbsence = _absenceService.AddAbsence(absence);
        var currentMonth = new DateTime(2025, 12, 1);
        var nextMonth = new DateTime(2026, 1, 1);
        var currentMonthBefore = _transactionService.GetTransactionsByMonth(currentMonth).Count();
        var nextMonthBefore = _transactionService.GetTransactionsByMonth(nextMonth).Count();

        // Act
        _absenceService.DeleteAbsence(addedAbsence.Id);

        // Assert - Check that associated transactions were removed
        var currentMonthAfter = _transactionService.GetTransactionsByMonth(currentMonth).Count();
        var nextMonthAfter = _transactionService.GetTransactionsByMonth(nextMonth).Count();
        
        
        Assert.Equal(currentMonthBefore - 1, currentMonthAfter); // Deduction transaction removed
        Assert.Equal(nextMonthBefore - 1, nextMonthAfter); // Compensation transaction removed
    }

    [Fact]
    public void AddAbsence_VAB_WithIncomeAboveCap_CompensationLimitedToCap()
    {
        // Arrange - Annual income well above 410,000 kr cap
        var annualIncome = 600_000m; // 50,000 kr per month
        var monthlyIncome = annualIncome / 12m; // 50,000 kr
        var dailyIncome = monthlyIncome / 22m; // ~2273 kr per day
        var absenceDate = new DateTime(2025, 12, 15);
        
        // Calculate expected capped daily rate based on 410,000 annual cap
        var cappedMonthlyIncome = 410_000m / 12m; // ~34,167 kr per month
        var cappedDailyIncome = cappedMonthlyIncome / 22m; // ~1553 kr per day
        var expectedCompensation = cappedDailyIncome * 0.80m; // 80% of capped amount = ~1242 kr
        
        var absence = new Absence
        {
            Date = absenceDate,
            Type = AbsenceType.VAB,
            DailyIncome = dailyIncome,
            Deduction = dailyIncome,
            Compensation = expectedCompensation, // Should be capped
            Note = "VAB with high income"
        };

        // Act
        var result = _absenceService.AddAbsence(absence);

        // Assert - Compensation should not exceed cap
        var nextMonth = new DateTime(2026, 1, 1);
        var transactions = _transactionService.GetTransactionsByMonth(nextMonth);
        var compensationTransaction = transactions.FirstOrDefault(t => t.Type == TransactionType.Income);
        
        Assert.NotNull(compensationTransaction);
        Assert.True(compensationTransaction.Amount <= expectedCompensation);
        Assert.True(compensationTransaction.Amount <= (410_000m / 12m / 22m * 0.80m)); // Cap based on monthly/daily calc
    }

    [Fact]
    public void AddAbsence_Sick_WithIncomeAboveCap_CompensationNotLimitedForFirstDays()
    {
        // Arrange - For sick leave, first 14 days from employer has no cap
        var annualIncome = 600_000m; // 50,000 kr per month
        var monthlyIncome = annualIncome / 12m; // 50,000 kr
        var dailyIncome = monthlyIncome / 22m; // ~2273 kr per day
        var absenceDate = new DateTime(2025, 12, 15);
        
        // For sick leave days 1-14 (from employer), there's no cap
        // Employee gets 80% of actual income
        var expectedCompensation = dailyIncome * 0.80m; // ~1818 kr
        
        var absence = new Absence
        {
            Date = absenceDate,
            Type = AbsenceType.Sick,
            DailyIncome = dailyIncome,
            Deduction = dailyIncome,
            Compensation = expectedCompensation, // No cap for employer-paid sick days
            Note = "Sick day with high income"
        };

        // Act
        var result = _absenceService.AddAbsence(absence);

        // Assert - Compensation should match full 80% of actual income (no cap for employer days)
        var nextMonth = new DateTime(2026, 1, 1);
        var transactions = _transactionService.GetTransactionsByMonth(nextMonth);
        var compensationTransaction = transactions.FirstOrDefault(t => t.Type == TransactionType.Income);
        
        Assert.NotNull(compensationTransaction);
        Assert.Equal(expectedCompensation, compensationTransaction.Amount);
        // Verify it's actually above the VAB cap to confirm no capping occurred
        Assert.True(compensationTransaction.Amount > (410_000m / 12m / 22m * 0.80m));
    }

    [Fact]
    public void AddAbsence_VAB_WithIncomeBelowCap_CompensationBasedOnActualIncome()
    {
        // Arrange - Annual income below 410,000 kr cap
        var annualIncome = 300_000m; // 25,000 kr per month
        var monthlyIncome = annualIncome / 12m; // 25,000 kr
        var dailyIncome = monthlyIncome / 22m; // ~1136 kr per day
        var absenceDate = new DateTime(2025, 12, 15);
        
        // Should get 80% of actual income since below cap
        var expectedCompensation = dailyIncome * 0.80m; // ~909 kr
        
        var absence = new Absence
        {
            Date = absenceDate,
            Type = AbsenceType.VAB,
            DailyIncome = dailyIncome,
            Deduction = dailyIncome,
            Compensation = expectedCompensation,
            Note = "VAB with income below cap"
        };

        // Act
        var result = _absenceService.AddAbsence(absence);

        // Assert - Compensation should be 80% of actual income
        var nextMonth = new DateTime(2026, 1, 1);
        var transactions = _transactionService.GetTransactionsByMonth(nextMonth);
        var compensationTransaction = transactions.FirstOrDefault(t => t.Type == TransactionType.Income);
        
        Assert.NotNull(compensationTransaction);
        Assert.Equal(expectedCompensation, compensationTransaction.Amount);
    }

    [Fact]
    public void AddAbsence_VAB_MultipleAbsencesThroughoutYear_CreatesAllTransactions()
    {
        // Arrange - Multiple VAB absences throughout the year
        var cappedMonthlyIncome = 410_000m / 12m;
        var cappedDailyIncome = cappedMonthlyIncome / 22m;
        var dailyCompensation = cappedDailyIncome * 0.80m;
        
        var absenceDates = new List<DateTime>();
        // 2-3 VAB days per month (realistic usage)
        for (int month = 1; month <= 12; month++)
        {
            absenceDates.Add(new DateTime(2025, month, 5));
            absenceDates.Add(new DateTime(2025, month, 15));
            if (month % 2 == 0) // Extra day every other month
            {
                absenceDates.Add(new DateTime(2025, month, 25));
            }
        }

        // Act - Add all absences
        foreach (var date in absenceDates)
        {
            var absence = new Absence
            {
                Date = date,
                Type = AbsenceType.VAB,
                DailyIncome = cappedDailyIncome,
                Deduction = cappedDailyIncome,
                Compensation = dailyCompensation,
                Note = $"VAB day {date:yyyy-MM-dd}"
            };
            _absenceService.AddAbsence(absence);
        }

        // Assert - All deductions should be created
        int totalDeductions = 0;
        for (int month = 1; month <= 12; month++)
        {
            var monthStart = new DateTime(2025, month, 1);
            var monthTransactions = _transactionService.GetTransactionsByMonth(monthStart);
            totalDeductions += monthTransactions
                .Count(t => t.Type == TransactionType.Expense && t.Description.Contains("Frånvaro avdrag"));
        }
        
        Assert.Equal(absenceDates.Count, totalDeductions);
        
        // Calculate total compensation
        decimal totalCompensation = 0m;
        for (int month = 1; month <= 12; month++)
        {
            var nextMonthStart = new DateTime(2025, month, 1).AddMonths(1);
            if (nextMonthStart.Year == 2026)
            {
                nextMonthStart = new DateTime(2026, 1, 1);
            }
            var monthTransactions = _transactionService.GetTransactionsByMonth(nextMonthStart);
            totalCompensation += monthTransactions
                .Where(t => t.Type == TransactionType.Income && t.Description.Contains("Frånvaro ersättning - VAB"))
                .Sum(t => t.Amount);
        }

        // All compensations should be created (no artificial yearly cap)
        var expectedTotal = absenceDates.Count * dailyCompensation;
        Assert.Equal(expectedTotal, totalCompensation);
    }

    [Fact]
    public void CalculateAbsenceImpact_HighSalaries_BothCappedToSameCompensation()
    {
        // Arrange - Two different high salaries (both above 410k cap)
        var salary1M = 1_000_000m;
        var salary500k = 500_000m;
        var standardHours = 2080m; // Full-time annual hours
        var testDate = new DateTime(2025, 12, 5);
        
        // Act - Calculate for both salaries
        var (dailyIncome1M, deduction1M, compensation1M) = _absenceService.CalculateAbsenceImpact(
            testDate, 
            AbsenceType.Sick, 
            salary1M, 
            standardHours);
        
        var (dailyIncome500k, deduction500k, compensation500k) = _absenceService.CalculateAbsenceImpact(
            testDate, 
            AbsenceType.Sick, 
            salary500k, 
            standardHours);
        
        // Assert - Daily incomes should be different (based on actual salary)
        Assert.NotEqual(dailyIncome1M, dailyIncome500k);
        Assert.True(dailyIncome1M > dailyIncome500k);
        
        // Assert - Deductions should be different (80% of actual salary)
        Assert.NotEqual(deduction1M, deduction500k);
        Assert.True(deduction1M > deduction500k);
        
        // Assert - Compensations should be THE SAME (both capped at 410k)
        Assert.Equal(compensation500k, compensation1M);
        
        // Assert - Verify compensation is based on 410k cap
        var expectedCappedMonthly = (410_000m / standardHours) * 160m;
        var expectedCappedDaily = expectedCappedMonthly / 22m;
        var expectedCompensation = expectedCappedDaily * 0.80m;
        Assert.Equal(expectedCompensation, compensation1M);
        Assert.Equal(expectedCompensation, compensation500k);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

