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
        Task<int> AddCategoryAsync(string name, double amount, BudgetType type);
        Task<int> RemoveCategoryAsync(int id);
        Task<int> UpdateCategoryAsync(int id, double amount, DateTime effectiveFrom, BudgetType type);
        bool HasCategories();
        bool CategoryExists(int id);
    }
}