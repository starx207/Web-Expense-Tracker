/*
    This example code taken from http://blogs.clariusconsulting.net/kzu/making-extension-methods-amenable-to-mocking/
 */
 
using ExpenseTracker.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Repository.Extensions
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
    }
}