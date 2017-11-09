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
        public MockBudget(TestAsyncEnumerable<BudgetCategory> addCategories,
                          TestAsyncEnumerable<Payee> addPayees) {
            _categories = addCategories;
            _payees = addPayees;
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

        public async Task<int> SaveChangesAsync() {
            return await Task.FromResult(1);
        }
    }
}