using ExpenseTracker.Repository;
using ExpenseTracker.Models;
using Microsoft.EntityFrameworkCore;
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

        public async Task<BudgetCategory> GetCategoryAsync(int? id) {
            if (id == null) {throw new NullIdException();}
            BudgetCategory category = await _categories.AsQueryable().SingleOrDefaultAsync(c => c.ID == id);
            if (category == null) {throw new IdNotFoundException();}
            return category;
        }

        public async Task AddBudgetCategoryAsync(BudgetCategory category) {
            IEnumerable<BudgetCategory> newCategories = _categories.AsEnumerable().Append(category);
            await Task.Factory.StartNew(() =>
                _categories = new TestAsyncEnumerable<BudgetCategory>(newCategories)
            );
        }

        public async Task RemoveBudgetCategoryAsync(int id) {
            try {
                BudgetCategory categoryToRemove = _categories.AsQueryable().Where(c => c.ID == id).FirstOrDefault();
                List<BudgetCategory> newCategories = _categories.AsEnumerable().ToList();
                newCategories.Remove(categoryToRemove);
                await Task.Factory.StartNew(() => 
                    _categories = new TestAsyncEnumerable<BudgetCategory>(newCategories)
                );
            } catch {}
        }

        public async Task UpdateBudgetCategoryAsync(int id, BudgetCategory category) {
            if (id != category.ID) {
                throw new IdMismatchException();
            }
            BudgetCategory categoryToEdit = _categories.AsQueryable().Where(c => c.ID == category.ID).First();
            List<BudgetCategory> newCategories = _categories.AsEnumerable().ToList();
            newCategories.Remove(categoryToEdit);
            newCategories.Add(category);
            await Task.Factory.StartNew(() =>
                _categories = new TestAsyncEnumerable<BudgetCategory>(newCategories)
            );
        }

        public IQueryable<Payee> GetPayees() {
            return _payees.AsQueryable();
        }

        public async Task<Payee> GetPayeeAsync(int? id) {
            if (id == null) { throw new NullIdException(); }
            Payee payee = await _payees.AsQueryable().SingleOrDefaultAsync(p => p.ID == id);
            if (payee == null) { throw new IdNotFoundException(); }
            return payee;
        }

        public async Task AddPayeeAsync(Payee payeeToAdd) {
            IEnumerable<Payee> newPayees = _payees.AsEnumerable().Append(payeeToAdd);
            await Task.Factory.StartNew(() =>
                _payees = new TestAsyncEnumerable<Payee>(newPayees)
            );
        }

        public async Task RemovePayeeAsync(int id) {
            try {
                Payee removePayee = _payees.AsQueryable().Where(p => p.ID == id).First();
                List<Payee> newPayees = _payees.AsEnumerable().ToList();
                newPayees.Remove(removePayee);
                await Task.Factory.StartNew(() =>
                    _payees = new TestAsyncEnumerable<Payee>(newPayees)
                );
            } catch {}
        }

        public async Task UpdatePayeeAsync(int id, Payee payee) {
            if (id != payee.ID) {
                throw new IdMismatchException();
            }
            Payee payeeToEdit = _payees.AsQueryable().Where(p => p.ID == payee.ID).First();
            List<Payee> newPayees = _payees.AsEnumerable().ToList();
            newPayees.Remove(payeeToEdit);
            newPayees.Add(payee);
            await Task.Factory.StartNew(() =>
                _payees = new TestAsyncEnumerable<Payee>(newPayees)
            );
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