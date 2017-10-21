using ExpenseTracker.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Data.Repository
{
    public interface IBudget
    {
        IQueryable<BudgetCategory> GetCategories();
        void AddBudgetCategory(BudgetCategory categoryToAdd);
        void RemoveBudgetCategory(BudgetCategory categoryToRemove);
        Task SaveChangesAsync();
    }
}