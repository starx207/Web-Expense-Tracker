using ExpenseTracker.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Services
{
    public interface ICategoryService
    {
        Task<List<BudgetCategory>> GetOrderedCategoryListAsync();
        IQueryable<BudgetCategory> GetOrderedCategoryQueryable();
        Task<BudgetCategory> GetSingleCategoryAsync(int? id);
        Task<int> AddCategoryAsync(BudgetCategory category);
        Task<int> RemoveCategoryAsync(int id);
        bool HasCategories();
        bool CategoryExists(int id);
    }
}