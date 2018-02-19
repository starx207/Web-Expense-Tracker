using ExpenseTracker.Exceptions;
using ExpenseTracker.Models;
using ExpenseTracker.Repository;
using ExpenseTracker.Repository.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Services
{
  public class CategoryManagerService : CommonService, ICategoryManagerService
    {
        private readonly IBudgetRepo _context;

        public CategoryManagerService(IBudgetRepo context) : base(context) {
            _context = context;
        }
        public bool HasCategories() {
            return _context.GetCategories().Any();
        }

        public async Task<BudgetCategory> GetSingleCategoryAsync(int? id) {
            if (id == null) {
                throw new NullIdException("No id specified");
            }

            var category = await _context.GetCategories().Extension().SingleOrDefaultAsync((int)id);
                
            if (category == null) {
                throw new IdNotFoundException($"No category found for ID = {id}");
            }

            return category;
        }

        public async Task<int> UpdateCategoryAsync(int id, BudgetCategory category, DateTime? effectiveFromDate) {
            if (id != category.ID) {
                throw new IdMismatchException($"Id = {id} does not match category Id of {category.ID}");
            }
            var effectiveFrom = effectiveFromDate ?? DateTime.Today;
            if (effectiveFrom > DateTime.Now) {
                throw new InvalidDateExpection("Effective From Date cannot be a future date");
            }

            try {
                // Check for a budget category that needs to be split based on the effectiveFrom date
                var categoryToSplit = _context.GetCategories()
                    .Where(c => c.Name == category.Name 
                        && c.BeginEffectiveDate < category.BeginEffectiveDate 
                        && c.EndEffectiveDate >= effectiveFrom
                        && c.ID != category.ID)
                    .FirstOrDefault();
                if (categoryToSplit != null) {
                    // If any found, reassign payees and transactions with Date >= effectiveFrom to the category being edited
                    ReassignPayeesAndTransactions(categoryToSplit, category, effectiveFrom);
                    categoryToSplit.EndEffectiveDate = effectiveFrom.AddDays(-1);
                    _context.EditBudgetCategory(categoryToSplit);
                }

                // Find all like-named categories with effective date after effectiveFrom and reassign their payees and transactions
                // then delete the category
                var categoriesToDelete = _context.GetCategories()
                    .Where(c => c.Name == category.Name && c.BeginEffectiveDate >= effectiveFrom && c.ID != category.ID);
                foreach (var deleteCategory in categoriesToDelete) {
                    ReassignPayeesAndTransactions(deleteCategory, category);
                    _context.DeleteBudgetCategory(deleteCategory);
                }

                // Set the edited category's BeginEffectiveDate and nullify its EndEffectiveDate
                category.BeginEffectiveDate = effectiveFrom;
                category.EndEffectiveDate = null;
                _context.EditBudgetCategory(category);

                return await _context.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException) {
                throw new ConcurrencyException();
            }
        }

        public async Task<int> AddCategoryAsync(BudgetCategory category) {
            _context.AddBudgetCategory(category);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> RemoveCategoryAsync(int id) {
            var category = _context.GetCategories().SingleOrDefault(c => c.ID == id);
            if (category != null) {
                _context.DeleteBudgetCategory(category);
            }
            return await _context.SaveChangesAsync();
        }

        public bool CategoryExists(int id) {
            return _context.GetCategories().Any(c => c.ID == id);
        }

        private void ReassignPayeesAndTransactions(BudgetCategory originalCategory, BudgetCategory newCategory, DateTime? reassignCutoff = null) {
            // If no date specified, use day before original BeginEffectiveDate. This will cause everything to be reassigned
            var dateToUse = reassignCutoff ?? originalCategory.BeginEffectiveDate.AddDays(-1);
            var payeesToReassign = _context.GetPayees().Where(p => p.BudgetCategoryID == originalCategory.ID && p.BeginEffectiveDate >= dateToUse);
            var transactionsToReassign = _context.GetTransactions().Where(t => t.OverrideCategoryID == originalCategory.ID && t.Date >= dateToUse);

            foreach (var payee in payeesToReassign) {
                payee.BudgetCategoryID = newCategory.ID;
                _context.EditPayee(payee);
            }
            foreach (var transaction in transactionsToReassign) {
                transaction.OverrideCategoryID = newCategory.ID;
                _context.EditTransaction(transaction);
            }
        }
    }
}