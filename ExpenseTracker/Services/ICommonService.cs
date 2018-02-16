using ExpenseTracker.Models;
using System.Linq;

namespace ExpenseTracker.Services
{
    public interface ICommonService
    {
        IQueryable<Payee> GetPayees(bool includeCategory = false, bool includeAliases = false, bool includeTransaction = false);
        IQueryable<BudgetCategory> GetCategories();
    }
}