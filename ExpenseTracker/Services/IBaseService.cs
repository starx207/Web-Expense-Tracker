using ExpenseTracker.Models;
using System.Linq;

namespace ExpenseTracker.Services
{
    public interface IBaseService
    {
        IQueryable<Payee> GetOrderedPayeeQueryable();
        IQueryable<BudgetCategory> GetOrderedCategoryQueryable();
    }
}