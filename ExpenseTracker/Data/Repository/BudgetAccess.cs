using ExpenseTracker.Models;
using System.Linq;

namespace ExpenseTracker.Data.Repository
{
    public class BudgetAccess : IBudgetAccess
    {
        private readonly BudgetContext _context;

        public BudgetAccess(BudgetContext context) {
            _context = context;
        }

        #region "GET methods"
        public IQueryable<Transaction> Transactions() {
            IQueryable<Transaction> transactions = _context.Transactions.AsQueryable();
            return transactions;
        }

        public IQueryable<Payee> Payees() {
            IQueryable<Payee> payees = _context.Payees.AsQueryable();
            return payees;
        }

        public IQueryable<BudgetCategory> BudgetCategories() {
            IQueryable<BudgetCategory> categories = _context.BudgetCategories.AsQueryable();
            return categories;
        }
        #endregion

        #region "DELETE methods"
        public void DeleteTransaction(Transaction transactionToDelete) {
            _context.Transactions.Remove(transactionToDelete);
        }

        public void DeletePayee(Payee payeeToDelete) {
            _context.Payees.Remove(payeeToDelete);
        }

        public void DeleteBudgetCategory(BudgetCategory categoryToDelete) {
            _context.BudgetCategories.Remove(categoryToDelete);
        }
        #endregion

        #region "ADD methods"
        public async void AddTransactionAsync(Transaction transactionToAdd) {
            await _context.Transactions.AddAsync(transactionToAdd);
        }

        public async void AddPayeeAsync(Payee payeeToAdd) {
            await _context.Payees.AddAsync(payeeToAdd);
        }

        public async void AddBudgetCategoryAsync(BudgetCategory categoryToAdd) {
            await _context.BudgetCategories.AddAsync(categoryToAdd);
        }
        #endregion

        #region "EDIT methods"
        public void EditTransaction(Transaction transactionToEdit) {
            _context.Transactions.Update(transactionToEdit);
        }

        public void EditPayee(Payee payeeToEdit) {
            _context.Payees.Update(payeeToEdit);
        }

        public void EditBudgetCategory(BudgetCategory categoryToEdit) {
            _context.BudgetCategories.Update(categoryToEdit);
        }
        #endregion
    }
}