using ExpenseTracker.Models;
using System.Linq;

namespace ExpenseTracker.Repository
{
    public interface ISharedRepo
    {
        IQueryable<Payee> GetOrderedPayeeQueryable(string orderBy, bool orderByDescending = false, bool includeAll = false);
        IQueryable<BudgetCategory> GetOrderedCategoryQueryable(string orderBy, bool orderByDescending = false);
    }
}