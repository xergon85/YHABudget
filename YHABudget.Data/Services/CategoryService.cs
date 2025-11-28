using YHABudget.Data.Context;
using YHABudget.Data.Enums;
using YHABudget.Data.Models;

namespace YHABudget.Data.Services;

public class CategoryService : ICategoryService
{
    private readonly BudgetDbContext _context;
    
    public CategoryService(BudgetDbContext context)
    {
        _context = context;
    }
    
    public IEnumerable<Category> GetAllCategories()
    {
        return _context.Categories
            .OrderBy(c => c.Type)
            .ThenBy(c => c.Name)
            .ToList();
    }
    
    public Category? GetCategoryById(int id)
    {
        return _context.Categories.Find(id);
    }
    
    public IEnumerable<Category> GetCategoriesByType(TransactionType type)
    {
        return _context.Categories
            .Where(c => c.Type == type)
            .OrderBy(c => c.Name)
            .ToList();
    }
    
    public Category AddCategory(Category category)
    {
        _context.Categories.Add(category);
        _context.SaveChanges();
        return category;
    }
    
    public void UpdateCategory(Category category)
    {
        _context.Categories.Update(category);
        _context.SaveChanges();
    }
    
    public void DeleteCategory(int id)
    {
        var category = _context.Categories.Find(id);
        if (category != null)
        {
            _context.Categories.Remove(category);
            _context.SaveChanges();
        }
    }
}
