using ExpenseTracker.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Data.Repository
{
    public interface IBudget
    {
        IQueryable<BudgetCategory> GetCategories();
        IQueryable<Payee> GetPayees();
        IQueryable<Alias> GetAliases();
        void AddBudgetCategory(BudgetCategory categoryToAdd);
        void AddPayee(Payee payeeToAdd);
        void AddAlias(Alias aliasToAdd);
        void RemoveBudgetCategory(BudgetCategory categoryToRemove);
        void RemovePayee(Payee payeeToRemove);
        void RemoveAlias(Alias aliasToRemove);
        void UpdatePayee(Payee editedPayee);
        void UpdateAlias(Alias editedAlias);
        Task<int> SaveChangesAsync();
    }
}