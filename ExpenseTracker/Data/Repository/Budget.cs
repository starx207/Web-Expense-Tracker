using ExpenseTracker.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Data.Repository
{
    public class Budget : IBudget
    {
        private readonly IBudgetAccess repo;
        public Budget(IBudgetAccess repository) { 
            repo = repository;
        }

        public IQueryable<BudgetCategory> GetCategories() {
            return repo.BudgetCategories();
        }

        public void AddBudgetCategory(BudgetCategory categoryToAdd) {
            repo.AddBudgetCategory(categoryToAdd);
        }
        public void RemoveBudgetCategory(BudgetCategory categoryToRemove) { 
            repo.DeleteBudgetCategory(categoryToRemove);
        }

        public async Task<int> SaveChangesAsync() {
            return await repo.SaveChangesAsync();
        }
    }
}