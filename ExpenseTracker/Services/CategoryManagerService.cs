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

        public async Task<int> UpdateCategoryAsync(int id, string name, double amount, DateTime effectiveFrom, BudgetType type) {
            BudgetCategory originalCategory = await _context.GetCategories()
                .SingleOrDefaultAsync(c => c.ID == id) ?? throw new IdNotFoundException($"No BudgetCategory found for Id = {id}");

            if (effectiveFrom > DateTime.Now) {
                throw new InvalidOperationException("EffectiveFrom Date cannot be a future date");
            }

            try {
                if (effectiveFrom > originalCategory.EffectiveFrom) {
                    // Add new category
                    var categoryToAdd = new BudgetCategory {
                        Name = name,
                        EffectiveFrom = effectiveFrom,
                        Amount = amount,
                        Type = type
                    };
                    _context.AddBudgetCategory(categoryToAdd);

                    // Reassign payees and transactions accordingly
                    ReassignPayeesByDate(originalCategory, categoryToAdd);
                    ReassingTransactionsByDate(originalCategory, categoryToAdd);
                } else {
                    // Update the original category's values
                    originalCategory.Name = name;
                    originalCategory.EffectiveFrom = effectiveFrom;
                    originalCategory.Amount = amount;
                    originalCategory.Type = type;

                    if (effectiveFrom < originalCategory.EffectiveFrom) {
                        // Check if there is an older category whose transactions/payees need to be split up
                        var categoryToStealFrom = _context.GetCategories()
                            .Where(c => c.Name == name && c.EffectiveFrom < effectiveFrom)
                            .OrderByDescending(c => c.EffectiveFrom)
                            .FirstOrDefault();
                        
                        if (categoryToStealFrom != null) {
                            ReassignPayeesByDate(categoryToStealFrom, originalCategory);
                            ReassingTransactionsByDate(categoryToStealFrom, originalCategory);
                        }

                        // Reassign payees/transactions from categories with newer EffectiveFrom dates and then delete the categories
                        foreach (BudgetCategory categoryToDelete in _context.GetCategories()
                            .Where(c => c.Name == name
                                && c.EffectiveFrom >= effectiveFrom
                                && c.ID != id)) {

                            ReassignPayeesByDate(categoryToDelete, originalCategory);
                            ReassingTransactionsByDate(categoryToDelete, originalCategory);
                            _context.DeleteBudgetCategory(categoryToDelete);
                        }
                    }

                    _context.EditBudgetCategory(originalCategory);
                }
                return await _context.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException) {
                throw new ConcurrencyException();
            }
        }

        // TODO: remove this version of UpdateCategoryAsync and convert all tests to use new version
        public async Task<int> UpdateCategoryAsync(int id, BudgetCategory category) {
            if (id != category.ID) {
                throw new IdMismatchException($"Id = {id} does not match category Id of {category.ID}");
            }

            if (category.EffectiveFrom > DateTime.Now) {
                throw new InvalidDateExpection("Effective From Date cannot be a future date");
            }

            try {
                var uneditedCategory = _context.GetCategories().AsNoTracking().Where(c => c.ID == category.ID).First();
                if (category.EffectiveFrom > uneditedCategory.EffectiveFrom) {
                    // Add new category
                    var categoryToAdd = new BudgetCategory {
                        ID = _context.GetCategories().OrderByDescending(c => c.ID).First().ID + 1,
                        Name = category.Name,
                        EffectiveFrom = category.EffectiveFrom,
                        Amount = category.Amount,
                        Type = category.Type
                    };
                    _context.AddBudgetCategory(categoryToAdd);
                    
                    // Reassign payees and transactinos accordingly
                    ReassignPayeesByDate(category, categoryToAdd);
                    ReassingTransactionsByDate(category, categoryToAdd);
                } else {
                    if (category.EffectiveFrom < uneditedCategory.EffectiveFrom) {
                        // Check if there is an older category whose transactions/payees need to be split up
                        var categoryToStealFrom = _context.GetCategories()
                            .Where(c => c.Name == category.Name
                                && c.EffectiveFrom < category.EffectiveFrom)
                            .OrderByDescending(c => c.EffectiveFrom)
                            .FirstOrDefault();
                        if (categoryToStealFrom != null) {
                            ReassignPayeesByDate(categoryToStealFrom, category);
                            ReassingTransactionsByDate(categoryToStealFrom, category);
                        }

                        // Reassign payees/transactions from categories with newer EffectiveFrom dates and then delete the categories
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
            if (fromCategory.Payees == null) { return; }

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