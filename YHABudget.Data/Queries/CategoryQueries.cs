using Microsoft.EntityFrameworkCore;
using YHABudget.Data.Context;
using YHABudget.Data.Enums;
using YHABudget.Data.Models;

namespace YHABudget.Data.Queries;

public class CategoryQueries
{
    private readonly BudgetDbContext _context;

    public CategoryQueries(BudgetDbContext context)
    {
        _context = context;
    }

    public List<Category> GetAllCategories()
    {
        return _context.Categories
            .OrderBy(c => c.Name)
            .ToList();
    }

    public Category? GetCategoryById(int id)
    {
        return _context.Categories.Find(id);
    }

    public List<Category> GetCategoriesOfType(TransactionType type)
    {
        return _context.Categories
            .Where(c => c.Type == type)
            .OrderBy(c => c.Name)
            .ToList();
    }
}
