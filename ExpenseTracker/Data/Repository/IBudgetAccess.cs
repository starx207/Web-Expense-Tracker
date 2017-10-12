using ExpenseTracker.Models;
using System.Linq;

namespace ExpenseTracker.Data.Repository
{
    public interface IBudgetAccess
    {
        IQueryable<Transaction> Transactions();
        IQueryable<Payee> Payees();
        IQueryable<BudgetCategory> BudgetCategories();
        void DeleteTransaction(Transaction transactionToDelete);
        void DeletePayee(Payee payeeToDelete);
        void DeleteBudgetCategory(BudgetCategory categoryToDelete);
        void AddTransactionAsync(Transaction transactionToAdd);
        void AddPayeeAsync(Payee payeeToAdd);
        void AddBudgetCategoryAsync(BudgetCategory categoryToAdd);
        void EditTransaction(Transaction transactionToEdit);
        void EditPayee(Payee payeeToEdit);
        void EditBudgetCategory(BudgetCategory categoryToEdit);
    }
}