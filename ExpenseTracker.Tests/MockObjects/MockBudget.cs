using ExpenseTracker.Data.Repository;
using ExpenseTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Tests.Mock
{
    internal class MockBudget : IBudget
    {
        private TestAsyncEnumerable<BudgetCategory> _categories;
        private TestAsyncEnumerable<Payee> _payees;
        private TestAsyncEnumerable<Alias> _aliases;
        private TestAsyncEnumerable<Transaction> _transactions;
        public MockBudget(TestAsyncEnumerable<BudgetCategory> addCategories,
                          TestAsyncEnumerable<Payee> addPayees,
                          TestAsyncEnumerable<Alias> addAliases,
                          TestAsyncEnumerable<Transaction> addTransactions) {
            _categories = addCategories;
            _payees = addPayees;
            _aliases = addAliases;
            _transactions = addTransactions;
        }
        public IQueryable<BudgetCategory> GetCategories() {
            return _categories.AsQueryable();
        }

        public void AddBudgetCategory(BudgetCategory category) {
            IEnumerable<BudgetCategory> newCategories = _categories.AsEnumerable().Append(category);
            _categories = new TestAsyncEnumerable<BudgetCategory>(newCategories);
        }

        public void RemoveBudgetCategory(BudgetCategory category) {
            BudgetCategory categoryToRemove = _categories.AsQueryable().Where(c => c.ID == category.ID).First();
            List<BudgetCategory> newCategories = _categories.AsEnumerable().ToList();
            newCategories.Remove(categoryToRemove);
            _categories = new TestAsyncEnumerable<BudgetCategory>(newCategories);
        }

        public void UpdateBudgetCategory(BudgetCategory category) {
            BudgetCategory categoryToEdit = _categories.AsQueryable().Where(c => c.ID == category.ID).First();
            List<BudgetCategory> newCategories = _categories.AsEnumerable().ToList();
            newCategories.Remove(categoryToEdit);
            newCategories.Add(category);
            _categories = new TestAsyncEnumerable<BudgetCategory>(newCategories);
        }

        public IQueryable<Payee> GetPayees() {
            return _payees.AsQueryable();
        }

        public void AddPayee(Payee payeeToAdd) {
            IEnumerable<Payee> newPayees = _payees.AsEnumerable().Append(payeeToAdd);
            _payees = new TestAsyncEnumerable<Payee>(newPayees);
        }

        public void RemovePayee(Payee payeeToRemove) {
            Payee removePayee = _payees.AsQueryable().Where(p => p.ID == payeeToRemove.ID).First();
            List<Payee> newPayees = _payees.AsEnumerable().ToList();
            newPayees.Remove(removePayee);
            _payees = new TestAsyncEnumerable<Payee>(newPayees);
        }

        public void UpdatePayee(Payee editedPayee) {
            Payee payeeToEdit = _payees.AsQueryable().Where(p => p.ID == editedPayee.ID).First();
            List<Payee> newPayees = _payees.AsEnumerable().ToList();
            newPayees.Remove(payeeToEdit);
            newPayees.Add(editedPayee);

            _payees = new TestAsyncEnumerable<Payee>(newPayees);
        }

        public IQueryable<Alias> GetAliases() {
            return _aliases.AsQueryable();
        }

        public void AddAlias(Alias aliasToAdd) {
            IEnumerable<Alias> newAliases = _aliases.AsEnumerable().Append(aliasToAdd);
            _aliases = new TestAsyncEnumerable<Alias>(newAliases);
        }

        public void RemoveAlias(Alias aliasToRemove) {
            Alias removeAlias = _aliases.AsQueryable().Where(a => a.ID == aliasToRemove.ID).First();
            List<Alias> newAliases = _aliases.AsEnumerable().ToList();
            newAliases.Remove(removeAlias);
            _aliases = new TestAsyncEnumerable<Alias>(newAliases);
        }

        public void UpdateAlias(Alias editedAlias) {
            Alias aliasToEdit = _aliases.AsQueryable().Where(a => a.ID == editedAlias.ID).First();
            List<Alias> newAliases = _aliases.AsEnumerable().ToList();
            newAliases.Remove(aliasToEdit);
            newAliases.Add(editedAlias);

            _aliases = new TestAsyncEnumerable<Alias>(newAliases);
        }

        public IQueryable<Transaction> GetTransactions() {
            return _transactions.AsQueryable();
        }

        public void AddTransaction(Transaction transactionToAdd) {
            IEnumerable<Transaction> newTransactions = _transactions.AsEnumerable().Append(transactionToAdd);
            _transactions = new TestAsyncEnumerable<Transaction>(newTransactions);
        }

        public void RemoveTransaction(Transaction transactionToRemove) {
            Transaction removeTransaction = _transactions.AsQueryable().Where(t => t.ID == transactionToRemove.ID).First();
            List<Transaction> newTransactions = _transactions.AsEnumerable().ToList();
            newTransactions.Remove(removeTransaction);
            _transactions = new TestAsyncEnumerable<Transaction>(newTransactions);
        }

        public void UpdateTransaction(Transaction editedTransaction) {
            Transaction transactionToEdit = _transactions.AsQueryable().Where(t => t.ID == editedTransaction.ID).First();
            List<Transaction> newTransactions = _transactions.AsEnumerable().ToList();
            newTransactions.Remove(transactionToEdit);
            newTransactions.Add(editedTransaction);

            _transactions = new TestAsyncEnumerable<Transaction>(newTransactions);
        }

        public async Task<int> SaveChangesAsync() {
            return await Task.FromResult(1);
        }
    }
}