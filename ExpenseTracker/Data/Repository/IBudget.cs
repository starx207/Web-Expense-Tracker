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
        IQueryable<Transaction> GetTransactions();
        void AddBudgetCategory(BudgetCategory categoryToAdd);
        void AddPayee(Payee payeeToAdd);
        void AddAlias(Alias aliasToAdd);
        void AddTransaction(Transaction transactionToAdd);
        void RemoveBudgetCategory(BudgetCategory categoryToRemove);
        void RemovePayee(Payee payeeToRemove);
        void RemoveAlias(Alias aliasToRemove);
        void RemoveTransaction(Transaction transactionToRemove);
        void UpdateBudgetCategory(BudgetCategory editedCategory);
        void UpdatePayee(Payee editedPayee);
        void UpdateAlias(Alias editedAlias);
        void UpdateTransaction(Transaction editedTransaction);
        Task<int> SaveChangesAsync();
    }
}