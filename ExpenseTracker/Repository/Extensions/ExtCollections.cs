/*
    This example code taken from http://blogs.clariusconsulting.net/kzu/making-extension-methods-amenable-to-mocking/
 */
 
using ExpenseTracker.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
[assembly: InternalsVisibleTo("ExpenseTracker.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")] // Needed for Moq

namespace ExpenseTracker.Repository.Extensions
{
    internal class GenericExt<T> : IExtensionMask<T>
    {
        IQueryable<T> _collection;
        public GenericExt(IQueryable<T> collection) => _collection = collection;
        public virtual async Task<List<T>> ToListAsync() => await _collection.ToListAsync();
        public virtual async Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> selector) => await _collection.SingleOrDefaultAsync(selector);
    }

    internal class CategoryExt : GenericExt<BudgetCategory> { 
        public CategoryExt() : base(new List<BudgetCategory>().AsQueryable()) { }
        public CategoryExt(IQueryable<BudgetCategory> collection) : base(collection) { }
    }

    internal class PayeeExt: GenericExt<Payee> {
        public PayeeExt() : base(new List<Payee>().AsQueryable()) { }
        public PayeeExt(IQueryable<Payee> collection) : base(collection) { }
    }

    internal class AliasExt: GenericExt<Alias> {
        public AliasExt() : base(new List<Alias>().AsQueryable()) { }
        public AliasExt(IQueryable<Alias> collection) : base(collection) { }
    }

    internal class TransactionExt: GenericExt<Transaction> {
        public TransactionExt() : base(new List<Transaction>().AsQueryable()) { }
        public TransactionExt(IQueryable<Transaction> collection) : base(collection) { }
    }
}