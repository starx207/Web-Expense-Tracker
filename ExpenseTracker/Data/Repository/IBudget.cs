using ExpenseTracker.Models;
using System.Linq;

namespace ExpenseTracker.Data.Repository
{
    public interface IBudget
    {
        IQueryable<BudgetCategory> GetCategories();
        void AddBudgetCategory(BudgetCategory categoryToAdd);
    }
}