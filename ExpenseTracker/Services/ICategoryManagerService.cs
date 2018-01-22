using ExpenseTracker.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExpenseTracker.Services
{
    public interface ICategoryManagerService : ICommonService
    {
        Task<BudgetCategory> GetSingleCategoryAsync(int? id);
        Task<int> AddCategoryAsync(BudgetCategory category);
        Task<int> RemoveCategoryAsync(int id);
        bool HasCategories();
        bool CategoryExists(int id);
    }
}