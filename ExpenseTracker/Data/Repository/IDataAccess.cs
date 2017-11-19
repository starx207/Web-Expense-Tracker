using ExpenseTracker.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Data.Repository
{
    public interface IDataAccess
    {
        IQueryable<Transaction> Transactions();
        IQueryable<Payee> Payees();
        IQueryable<BudgetCategory> BudgetCategories();
        IQueryable<Alias> Aliases();
        void DeleteTransaction(Transaction transactionToDelete);
        void DeletePayee(Payee payeeToDelete);
        void DeleteBudgetCategory(BudgetCategory categoryToDelete);
        void DeleteAlias(Alias aliasToDelete);
        void AddTransaction(Transaction transactionToAdd);
        void AddPayee(Payee payeeToAdd);
        void AddBudgetCategory(BudgetCategory categoryToAdd);
        void AddAlias(Alias aliasToAdd);
        void EditTransaction(Transaction transactionToEdit);
        void EditPayee(Payee payeeToEdit);
        void EditBudgetCategory(BudgetCategory categoryToEdit);
        void EditAlias(Alias aliasToEdit);
        Task<int> SaveChangesAsync();
    }
}