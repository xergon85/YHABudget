using YHABudget.Data.Context;
using YHABudget.Data.Enums;
using YHABudget.Data.Models;
using YHABudget.Data.Queries;

namespace YHABudget.Data.Services;

public class CategoryService : ICategoryService
{
    private readonly BudgetDbContext _context;
    private readonly CategoryQueries _queries;
    
    public CategoryService(BudgetDbContext context)
    {
        _context = context;
        _queries = new CategoryQueries(context);
    }
    
    public IEnumerable<Category> GetAllCategories()
    {
        return _queries.GetAllCategories();
    }
    
    public Category? GetCategoryById(int id)
    {
        return _queries.GetCategoryById(id);
    }
    
    public IEnumerable<Category> GetCategoriesByType(TransactionType type)
    {
        return _queries.GetCategoriesOfType(type);
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
