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
}