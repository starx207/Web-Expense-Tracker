using ExpenseTracker.Data;
using ExpenseTracker.Models;
using ExpenseTracker.Repository.Extensions;
using Microsoft.EntityFrameworkCore;
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

        #region Get Methods
            public IQueryable<BudgetCategory> GetCategories() => _context.BudgetCategories.AsQueryable();
            public IQueryable<Payee> GetPayees(bool includeCategory = false, bool includeAliases = false, bool includeTransactions = false) {
                var payees = _context.Payees.AsQueryable();
                if (includeCategory) {
                    payees = payees.Include(p => p.Category);
                }
                if (includeAliases) {
                    payees = payees.Include(p => p.Aliases);
                }
                if (includeTransactions) {
                    payees = payees.Include(p => p.Transactions);
                }
                return payees;
            }
            public IQueryable<Alias> GetAliases(bool includePayee = false) {
                if (includePayee) {
                    return _context.Aliases.Include(a => a.AliasForPayee).AsQueryable();
                } else {
                    return _context.Aliases.AsQueryable();
                }
            }
            public IQueryable<Transaction> GetTransactions(bool includePayee = false, bool includeCategory = false) {
                var transactions = _context.Transactions.AsQueryable();
                if (includeCategory) {
                    transactions = transactions.Include(t => t.OverrideCategory);
                }
                if (includePayee) {
                    transactions = transactions.Include(t => t.PayableTo);
                }
                return transactions;
            }
        #endregion

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