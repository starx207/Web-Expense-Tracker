using ExpenseTracker.Data.Repository;
using ExpenseTracker.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Tests.Mock
{
    public class MockBudget : IBudget
    {
        private List<BudgetCategory> _categories;
        public MockBudget(List<BudgetCategory> addCategories) {
            _categories = addCategories;
        }
        public IQueryable<BudgetCategory> GetCategories() {
            return _categories.AsQueryable();
        }

        public void AddBudgetCategory(BudgetCategory category) {
            _categories.Add(category);
        }

        public void RemoveBudgetCategory(BudgetCategory category) {
            BudgetCategory categoryToRemove = _categories.Where(c => c.ID == category.ID).First();
            _categories.Remove(categoryToRemove);
        }

        public async Task SaveChangesAsync() {
            await new Task(PretendToDoSomething);
        }

        private void PretendToDoSomething() { }
    }
}