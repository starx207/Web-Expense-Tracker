using ExpenseTracker.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Data
{
    public static class EFMask
    {
        public static async Task<List<T>> ConvertToListAsync<T>(this IQueryable<T> collection) where T : class {
            return await collection.ToListAsync();
        }

        public static async Task<BudgetCategory> SingleOrDefaultCategoryAsync(this IQueryable<BudgetCategory> collection, int id) {
            return await collection.SingleOrDefaultAsync(c => c.ID == id);
        }

        public static IQueryable<Payee> IncludeAllPayeeProperties(this IQueryable<Payee> collection) {
            return collection.Include(p => p.Category).Include(p => p.Aliases);
        }

        public static async Task<Alias> SingleOrDefaultAliasAsync(this IQueryable<Alias> collection, int id) {
            return await collection.SingleOrDefaultAsync(a => a.ID == id);
        }

        public static async Task<Payee> SingleOrDefaultPayeeAsync(this IQueryable<Payee> collection, int id) {
            return await collection.SingleOrDefaultAsync(p => p.ID == id);
        }

        public static IQueryable<Transaction> IncludeAllTransactionProperties(this IQueryable<Transaction> collection) {
            return collection.Include(t => t.OverrideCategory).Include(t => t.PayableTo);
        }

        public static async Task<Transaction> SingleOrDefaultTransactionAsync(this IQueryable<Transaction> collection, int id) {
            return await collection.SingleOrDefaultAsync(t => t.ID == id);
        }

        public static IQueryable<Alias> IncludeAllAliasProperties(this IQueryable<Alias> collection) {
            return collection.Include(a => a.AliasForPayee);
        }
    }
}