using ExpenseTracker.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExpenseTracker.Repository
{
    public interface ICategoryRepo : ISharedRepo
    {
        //Task<List<BudgetCategory>> GetOrderedCategoryListAsync(string orderBy, bool orderByDescending = false);
        Task<BudgetCategory> GetSingleCategoryAsync(int? id);
        Task<int> AddCategoryAsync(BudgetCategory category);
        Task<int> RemoveCategoryAsync(int id);
        bool HasCategories();
        bool CategoryExists(int id);
    }
}