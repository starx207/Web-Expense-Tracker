using ExpenseTracker.Data;
using ExpenseTracker.Exceptions;
using ExpenseTracker.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Services
{
    public class BudgetService : IBudgetService
    {
        private readonly BudgetContext _context;

        public BudgetService(BudgetContext context) {
            _context = context;
        }

        #region Budget Category Methods
            public bool HasCategories() {
                return _context.BudgetCategories.Any();
            }
            public async Task<List<BudgetCategory>> GetOrderedCategoryListAsync() {
                return await _context.BudgetCategories.OrderBy(c => c.Name).Extension().ToListAsync();
            }

            public IQueryable<BudgetCategory> GetOrderedCategoryQueryable() {
                return _context.BudgetCategories.OrderBy(c => c.Name).AsQueryable();
            }

            public async Task<BudgetCategory> GetSingleCategoryAsync(int? id) {
                if (id == null) {
                    throw new NullIdException("No id specified");
                }

                var category = await _context.BudgetCategories.Extension().SingleOrDefaultAsync((int)id);
                
                if (category == null) {
                    throw new IdNotFoundException($"No category found for ID = {id}");
                }

                return category;
            }

            public async Task<int> AddCategoryAsync(BudgetCategory category) {
                _context.BudgetCategories.Add(category);
                return await _context.SaveChangesAsync();
            }

            public async Task<int> RemoveCategoryAsync(int id) {
                var category = _context.BudgetCategories.SingleOrDefault(c => c.ID == id);
                if (category != null) {
                    _context.BudgetCategories.Remove(category);
                }
                return await _context.SaveChangesAsync();
            }

            public bool CategoryExists(int id) {
                return _context.BudgetCategories.Any(c => c.ID == id);
            }
        #endregion

        #region Payee Methods
            public async Task<List<Payee>> GetOrderedPayeeListAsync(bool includeAll = false) {
                var retVals = _context.Payees.AsQueryable();
                if (includeAll) {
                    retVals = retVals.Extension().IncludeAll();
                }
                return await retVals.OrderBy(p => p.Name).Extension().ToListAsync();
            }

            public IQueryable<Payee> GetOrderedPayeeQueryable() {
                return _context.Payees.OrderBy(p => p.Name).AsQueryable();
            }

            public async Task<Payee> GetSinglePayeeAsync(int? id, bool includeAll = false) {
                if (id == null) {
                    throw new NullIdException("No id specified");
                }

                var payees = _context.Payees.AsQueryable();

                if (includeAll) {
                    payees = payees.Extension().IncludeAll();
                }

                var payee = await payees.Extension().SingleOrDefaultAsync((int)id);
                
                if (payee == null) {
                    throw new IdNotFoundException($"No payee found for ID = {id}");
                }

                return payee;
            }

            public async Task<int> AddPayeeAsync(Payee payee) {
                _context.Payees.Add(payee);
                return await _context.SaveChangesAsync();
            }

            public async Task<int> UpdatePayeeAsync(int id, Payee payee) {
                if (id != payee.ID) {
                    throw new IdMismatchException($"Id = {id} does not match payee Id of {payee.ID}");
                }
                _context.Payees.Update(payee);
                return await _context.SaveChangesAsync();
            }

            public async Task<int> RemovePayeeAsync(int id) {
                var payee = _context.Payees.SingleOrDefault(p => p.ID == id);

                if (payee != null) {
                    _context.Payees.Remove(payee);
                }

                return await _context.SaveChangesAsync();
            }

            public bool PayeeExists(int id) {
                return _context.Payees.Any(p => p.ID == id);
            }
        #endregion
        
        #region Alias Methods
            public async Task<Alias> GetSingleAliasAsync(int? id, bool includeAll = false) {
                if (id == null) {
                    throw new NullIdException("No id specified");
                }

                var aliases = _context.Aliases.AsQueryable();

                if (includeAll) {
                    aliases = aliases.Extension().IncludeAll();
                }

                var alias = await aliases.Extension().SingleOrDefaultAsync((int)id);

                if (alias == null) {
                    throw new IdNotFoundException($"No alias found for ID = {id}");
                }

                return alias;
            }

            public async Task<int> UpdateAliasAsync(int id, Alias alias) {
                if (id != alias.ID) {
                    throw new IdMismatchException($"Id = {id} does not match alias Id of {alias.ID}");
                }
                _context.Aliases.Update(alias);
                return await _context.SaveChangesAsync();
            }

            public async Task<int> AddAliasAsync(Alias alias) {
                _context.Aliases.Add(alias);
                return await _context.SaveChangesAsync();
            }

            public async Task<int> RemoveAliasAsync(int id) {
                var alias = _context.Aliases.SingleOrDefault(c => c.ID == id);
                if (alias != null) {
                    _context.Aliases.Remove(alias);
                }
                return await _context.SaveChangesAsync();
            }

            public bool AliasExists(int id) {
                return _context.Aliases.Any(a => a.ID == id);
            }
        #endregion

        #region Transaction Methods
            public bool TransactionExists(int id) {
                return _context.Transactions.Any(t => t.ID == id);
            }

            public async Task<List<Transaction>> GetOrderedTransactionListAsync(bool includeAll = false) {
                var transactions = _context.Transactions.AsQueryable();

                if (includeAll) {
                    transactions = transactions.Extension().IncludeAll();
                }

                return await transactions.OrderByDescending(t => t.Date).Extension().ToListAsync();
            }

            public async Task<Transaction> GetSingleTransactionAsync(int? id, bool includeAll = false) {
                if (id == null) {
                    throw new NullIdException("No id specified");
                }

                var transactions = _context.Transactions.AsQueryable();

                if (includeAll) {
                    transactions = transactions.Extension().IncludeAll();
                }

                var transaction = await transactions.Extension().SingleOrDefaultAsync((int)id);

                if (transaction == null) {
                    throw new IdNotFoundException($"No transaction found for ID = {id}");
                }

                return transaction;
            }

            public async Task<int> AddTransactionAsync(Transaction transaction) {
                _context.Transactions.Add(transaction);
                return await _context.SaveChangesAsync();
            }

            public async Task<int> UpdateTransactionAsync(int id, Transaction transaction) {
                if (id != transaction.ID) {
                    throw new IdMismatchException($"Id = {id} does not equal transaction id of {transaction.ID}");
                }

                _context.Transactions.Update(transaction);
                return await _context.SaveChangesAsync();
            }

            public async Task<int> RemoveTransactionAsync(int id) {
                var transaction = _context.Transactions.SingleOrDefault(t => t.ID == id);
                if (transaction != null) {
                    _context.Transactions.Remove(transaction);
                }
                return await _context.SaveChangesAsync();
            }
        #endregion
    }
}