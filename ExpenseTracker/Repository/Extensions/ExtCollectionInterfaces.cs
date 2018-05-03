/*
    This example code taken from http://blogs.clariusconsulting.net/kzu/making-extension-methods-amenable-to-mocking/
 */
 
using ExpenseTracker.Models;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ExpenseTracker.Repository.Extensions
{
    public interface IExtensionMask<T>
    {
        Task<List<T>> ToListAsync();
        Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> selector);
    }

    // public interface ICategoryExtMask : IExtensionMask<BudgetCategory> { }

    // public interface IPayeeExtMask : IExtensionMask<Payee> { }

    // public interface IAliasExtMask : IExtensionMask<Alias> { }

    // public interface ITransactionExtMask : IExtensionMask<Transaction> { }
}