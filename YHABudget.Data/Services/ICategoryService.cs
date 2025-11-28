using YHABudget.Data.Enums;
using YHABudget.Data.Models;

namespace YHABudget.Data.Services;

public interface ICategoryService
{
    IEnumerable<Category> GetAllCategories();
    Category? GetCategoryById(int id);
    IEnumerable<Category> GetCategoriesByType(TransactionType type);
    Category AddCategory(Category category);
    void UpdateCategory(Category category);
    void DeleteCategory(int id);
}
