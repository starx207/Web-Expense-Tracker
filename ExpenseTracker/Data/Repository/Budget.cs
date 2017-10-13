using ExpenseTracker.Models;
using System;
using System.Linq;

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
    }
}