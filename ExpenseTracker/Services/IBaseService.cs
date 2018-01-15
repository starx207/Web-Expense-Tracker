using ExpenseTracker.Models;
using System.Linq;

namespace ExpenseTracker.Services
{
    public interface IBaseService
    {
        IQueryable<Payee> GetOrderedPayeeQueryable(string orderBy, bool orderByDescending = false, bool includeAll = false);
        IQueryable<BudgetCategory> GetOrderedCategoryQueryable(string orderBy, bool orderByDescending = false);
    }
}