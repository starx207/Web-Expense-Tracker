/*
    This example code taken from http://blogs.clariusconsulting.net/kzu/making-extension-methods-amenable-to-mocking/
 */
 
using ExpenseTracker.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Data
{
    internal class CategoryExt : ICategoryExtMask
    {
        IQueryable<BudgetCategory> _collection;

        public CategoryExt(IQueryable<BudgetCategory> collection) {
            _collection = collection;
        }

        public async Task<List<BudgetCategory>> ToListAsync() {
            return await _collection.ToListAsync();
        }

        public async Task<BudgetCategory> SingleOrDefaultAsync(int id) {
            return await _collection.SingleOrDefaultAsync(c => c.ID == id);
        }
    }

    internal class PayeeExt: IPayeeExtMask
    {
        IQueryable<Payee> _collection;

        public PayeeExt(IQueryable<Payee> collection) {
            _collection = collection;
        }

        public async Task<List<Payee>> ToListAsync() {
            return await _collection.ToListAsync();
        }

        public async Task<Payee> SingleOrDefaultAsync(int id) {
            return await _collection.SingleOrDefaultAsync(p => p.ID == id);
        }

        public IQueryable<Payee> IncludeAll() {
            return _collection.Include(p => p.Category).Include(p => p.Aliases);
        }
    }

    internal class AliasExt: IAliasExtMask
    {
        IQueryable<Alias> _collection;

        public AliasExt(IQueryable<Alias> collection) {
            _collection = collection;
        }

        public async Task<List<Alias>> ToListAsync() {
            return await _collection.ToListAsync();
        }

        public async Task<Alias> SingleOrDefaultAsync(int id) {
            return await _collection.SingleOrDefaultAsync(a => a.ID == id);
        }

        public IQueryable<Alias> IncludeAll() {
            return _collection.Include(p => p.AliasForPayee);
        }
    }

    internal class TransactionExt: ITransactionExtMask
    {
        IQueryable<Transaction> _collection;

        public TransactionExt(IQueryable<Transaction> collection) {
            _collection = collection;
        }

        public async Task<List<Transaction>> ToListAsync() {
            return await _collection.ToListAsync();
        }

        public async Task<Transaction> SingleOrDefaultAsync(int id) {
            return await _collection.SingleOrDefaultAsync(t => t.ID == id);
        }

        public IQueryable<Transaction> IncludeAll() {
            return _collection.Include(p => p.OverrideCategory).Include(p => p.PayableTo);
        }
    }
}