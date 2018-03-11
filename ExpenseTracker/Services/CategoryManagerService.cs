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

        public async Task<int> UpdateCategoryAsync(int id, BudgetCategory category) {
            if (id != category.ID) {
                throw new IdMismatchException($"Id = {id} does not match category Id of {category.ID}");
            }

            if (category.EffectiveFrom > DateTime.Now) {
                throw new InvalidDateExpection("Effective From Date cannot be a future date");
            }

            try {
                var uneditedCategory = _context.GetCategories().Where(c => c.ID == category.ID).First();
                if (category.EffectiveFrom > uneditedCategory.EffectiveFrom) {
                    // Add new category
                    _context.AddBudgetCategory( new BudgetCategory {
                        Name = category.Name,
                        EffectiveFrom = category.EffectiveFrom,
                        Amount = category.Amount,
                        Type = category.Type
                    });
                    // Retrieve newly added category and reassign payees and transactinos accordingly
                    var addedCategory = _context.GetCategories().Where(c => c.Name == category.Name).OrderByDescending(c => c.EffectiveFrom).First();
                    ReassignPayeesByDate(category, addedCategory);
                    ReassingTransactionsByDate(category, addedCategory);
                } else {
                    if (category.EffectiveFrom < uneditedCategory.EffectiveFrom) {
                        // Check if there is an older category whose transactions/payees need to be split up
                        var categoryToStealFrom = _context.GetCategories()
                            .Where(c => c.Name == category.Name
                                && c.EffectiveFrom < category.EffectiveFrom)
                            .FirstOrDefault();
                        if (categoryToStealFrom != null) {
                            ReassignPayeesByDate(categoryToStealFrom, category);
                            ReassingTransactionsByDate(categoryToStealFrom, category);
                        }

                        // Reassign payees/transactions from categories with newer EffectiveFrom dates and then delete the categoreis
                        foreach (BudgetCategory categoryToDelete in _context.GetCategories()
                            .Where(c => c.Name == category.Name
                                && c.EffectiveFrom >= category.EffectiveFrom
                                && c.ID != category.ID)) {

                            ReassignPayeesByDate(categoryToDelete, category);
                            ReassingTransactionsByDate(categoryToDelete, category);
                            _context.DeleteBudgetCategory(categoryToDelete);
                        }
                    }
                    _context.EditBudgetCategory(category);
                }                

                return await _context.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException) {
                throw new ConcurrencyException();
            }
        }

        public async Task<int> AddCategoryAsync(BudgetCategory category) {
            if (_context.GetCategories().Any(c => c.Name == category.Name)) {
                throw new UniqueConstraintViolationException($"There is already a Budget Category with Name = {category.Name}");
            }
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

        private void ReassignPayeesByDate(BudgetCategory fromCategory, BudgetCategory toCategory) {
            foreach (Payee payee in fromCategory.Payees.Where(p => p.EffectiveFrom >= toCategory.EffectiveFrom)) {
                payee.BudgetCategoryID = toCategory.ID;
                _context.EditPayee(payee);
            }
        }

        private void ReassingTransactionsByDate(BudgetCategory fromCategory, BudgetCategory toCategory) {
            foreach (Transaction tran in _context.GetTransactions()
                .Where(t => t.OverrideCategoryID == fromCategory.ID && t.Date >= toCategory.EffectiveFrom)) {

                tran.OverrideCategoryID = toCategory.ID;
                _context.EditTransaction(tran);
            }
        }
    }
}