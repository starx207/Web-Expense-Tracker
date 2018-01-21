using ExpenseTracker.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Repository
{
    public interface IBudgetRepo
    {
        IQueryable<Transaction> Transactions { get; }
        IQueryable<Payee> Payees { get; }
        IQueryable<BudgetCategory> Categories { get; }
        IQueryable<Alias> Aliases { get; }
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