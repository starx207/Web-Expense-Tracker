using ExpenseTracker.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Data.Repository
{
    public interface IBudget
    {
        IQueryable<BudgetCategory> GetCategories();
        IQueryable<Payee> GetPayees();
        void AddBudgetCategory(BudgetCategory categoryToAdd);
        void AddPayee(Payee payeeToAdd);
        void RemoveBudgetCategory(BudgetCategory categoryToRemove);
        void RemovePayee(Payee payeeToRemove);
        void UpdatePayee(Payee editedPayee);
        Task<int> SaveChangesAsync();
    }
}