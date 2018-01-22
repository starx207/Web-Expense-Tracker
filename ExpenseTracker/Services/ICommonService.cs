using ExpenseTracker.Models;
using System.Linq;

namespace ExpenseTracker.Services
{
    public interface ICommonService
    {
        IQueryable<Payee> GetOrderedPayees(string orderBy, bool orderByDescending = false, bool includeAll = false);
        IQueryable<BudgetCategory> GetOrderedCategories(string orderBy, bool orderByDescending = false);
    }
}