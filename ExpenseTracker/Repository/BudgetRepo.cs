using ExpenseTracker.Data;
using ExpenseTracker.Models;
using ExpenseTracker.Repository.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Repository
{
    public class BudgetRepo : IBudgetRepo
    {
        private readonly BudgetContext _context;

        public BudgetRepo(BudgetContext context) {
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

        public async Task<List<BudgetCategory>> BudgetCategoriesAsync(string orderBy = "", bool descendingOrder = false) {
            var returnCategories = _context.BudgetCategories.AsQueryable();
            
            returnCategories = SortQueryableCollectionByProperty(orderBy, returnCategories, descendingOrder);

            return await returnCategories.Extension().ToListAsync();
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

        private IQueryable<T> SortQueryableCollectionByProperty<T>(string propertyName, IQueryable<T> queryable, bool descending) where T : class {

            if (typeof(T).GetProperty(propertyName) == null) {
                throw new ArgumentException($"{typeof(T).Name}.{propertyName} does not exist");
            }

            if (descending) {
                queryable = queryable.OrderByDescending(q => GetPropetyValue(propertyName, q));
            } else {
                queryable = queryable.OrderBy(q => GetPropetyValue(propertyName, q));
            }
            return queryable;
        }

        private object GetPropetyValue<T>(string propertyName, T obj) {
            return obj.GetType().GetProperty(propertyName).GetAccessors()[0].Invoke(obj, null);
        }
    }
}