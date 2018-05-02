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
        Task<int> UpdateCategoryAsync(int id, string name, double amount, DateTime effectiveFrom, BudgetType type);
        //Task<int> UpdateCategoryAsync(int id, BudgetCategory category);
        bool HasCategories();
        bool CategoryExists(int id);
    }
}