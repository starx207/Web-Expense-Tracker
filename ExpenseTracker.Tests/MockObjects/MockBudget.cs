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
        public MockBudget(TestAsyncEnumerable<BudgetCategory> addCategories) {
            _categories = addCategories;
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
            throw new NotImplementedException("GetPayees() is not implemented for the MockBudget");
        }

        public void AddPayee(Payee payeeToAdd) {
            throw new NotImplementedException("AddPayee() is not implemented for the MockBudget");
        }

        public async Task<int> SaveChangesAsync() {
            return await Task.FromResult(1);
        }
    }
}