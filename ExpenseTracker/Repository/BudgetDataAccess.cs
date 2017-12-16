using ExpenseTracker.Data;
using ExpenseTracker.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Repository
{
    public class BudgetDataAccess : IDataAccess
    {
        private readonly BudgetContext _context;

        public BudgetDataAccess(BudgetContext context) {
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

        public IQueryable<Alias> Aliases() {
            IQueryable<Alias> aliases = _context.Aliases.AsQueryable();
            return aliases;
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

        public void DeleteAlias(Alias aliasToDelete) {
            _context.Aliases.Remove(aliasToDelete);
        }
        #endregion

        #region "ADD methods"
        public void AddTransaction(Transaction transactionToAdd) {
            _context.Transactions.Add(transactionToAdd);
        }

        public void AddPayee(Payee payeeToAdd) {
            _context.Payees.Add(payeeToAdd);
        }

        public void AddBudgetCategory(BudgetCategory categoryToAdd) {
            _context.BudgetCategories.Add(categoryToAdd);
        }

        public void AddAlias(Alias aliasToAdd) {
            _context.Aliases.Add(aliasToAdd);
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

        public void EditAlias(Alias aliasToEdit) {
            _context.Aliases.Update(aliasToEdit);
        }
        #endregion

        public async Task<int> SaveChangesAsync() {
            return await _context.SaveChangesAsync();
        }
    }
}