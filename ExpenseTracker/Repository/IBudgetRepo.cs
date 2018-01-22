using ExpenseTracker.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Repository
{
    public interface IBudgetRepo
    {
        IQueryable<Transaction> GetTransactions(bool includePayee = false, bool includeCategory = false);
        IQueryable<Payee> GetPayees(bool includeCategory = false);
        IQueryable<BudgetCategory> GetCategories();
        IQueryable<Alias> GetAliases(bool includePayee = false);
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