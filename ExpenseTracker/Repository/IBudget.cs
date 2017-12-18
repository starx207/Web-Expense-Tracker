using ExpenseTracker.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Repository
{
    public interface IBudget
    {
        IQueryable<BudgetCategory> GetCategories();
        IQueryable<Payee> GetPayees();
        IQueryable<Alias> GetAliases();
        IQueryable<Transaction> GetTransactions();
        Task<BudgetCategory> GetCategoryAsync(int? id);
        Task<Payee> GetPayeeAsync(int? id);
        Task AddBudgetCategoryAsync(BudgetCategory categoryToAdd);
        Task AddPayeeAsync(Payee payeeToAdd);
        void AddAlias(Alias aliasToAdd);
        void AddTransaction(Transaction transactionToAdd);
        Task RemoveBudgetCategoryAsync(int id);
        Task RemovePayeeAsync(int id);
        void RemoveAlias(Alias aliasToRemove);
        void RemoveTransaction(Transaction transactionToRemove);
        Task UpdateBudgetCategoryAsync(int id, BudgetCategory category);
        Task UpdatePayeeAsync(int id, Payee payee);
        void UpdateAlias(Alias editedAlias);
        void UpdateTransaction(Transaction editedTransaction);
        Task<int> SaveChangesAsync();
    }
}