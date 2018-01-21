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

        public IQueryable<BudgetCategory> Categories {
            get { return _context.BudgetCategories.AsQueryable(); }
        }
        public IQueryable<Payee> Payees {
            get { return _context.Payees.AsQueryable(); }
        }
        public IQueryable<Alias> Aliases {
            get { return _context.Aliases.AsQueryable(); }
        }
        public IQueryable<Transaction> Transactions {
            get { return _context.Transactions.AsQueryable(); }
        }

        #region "DELETE methods"
            public void DeleteTransaction(Transaction transactionToDelete) => _context.Transactions.Remove(transactionToDelete);
            public void DeletePayee(Payee payeeToDelete) => _context.Payees.Remove(payeeToDelete);
            public void DeleteBudgetCategory(BudgetCategory categoryToDelete) => _context.BudgetCategories.Remove(categoryToDelete);
            public void DeleteAlias(Alias aliasToDelete) => _context.Aliases.Remove(aliasToDelete);
        #endregion

        #region "ADD methods"
            public void AddTransaction(Transaction transactionToAdd) => _context.Transactions.Add(transactionToAdd);
            public void AddPayee(Payee payeeToAdd) => _context.Payees.Add(payeeToAdd);
            public void AddBudgetCategory(BudgetCategory categoryToAdd) => _context.BudgetCategories.Add(categoryToAdd);
            public void AddAlias(Alias aliasToAdd) => _context.Aliases.Add(aliasToAdd);
        #endregion

        #region "EDIT methods"
            public void EditTransaction(Transaction transactionToEdit) => _context.Transactions.Update(transactionToEdit);
            public void EditPayee(Payee payeeToEdit) => _context.Payees.Update(payeeToEdit);
            public void EditBudgetCategory(BudgetCategory categoryToEdit) => _context.BudgetCategories.Update(categoryToEdit);
            public void EditAlias(Alias aliasToEdit) => _context.Aliases.Update(aliasToEdit);
        #endregion

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}