using ExpenseTracker.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExpenseTracker.Services
{
    public interface ICategoryManagerService : ICommonService
    {
        Task<BudgetCategory> GetSingleCategoryAsync(int? id);
        Task<int> AddCategoryAsync(BudgetCategory category);
        Task<int> RemoveCategoryAsync(int id);
        Task<int> UpdateCategoryAsync(int id, BudgetCategory category, DateTime? effectiveFromDate);
        bool HasCategories();
        bool CategoryExists(int id);
    }
}