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

    // public interface ICategoryExtMask
    // {
    //     Task<List<BudgetCategory>> ToListAsync();
    //     Task<BudgetCategory> SingleOrDefaultAsync(int id);
    // }

    // public interface IPayeeExtMask
    // {
    //     Task<List<Payee>> ToListAsync();
    //     Task<Payee> SingleOrDefaultAsync(int id);
    // }

    // public interface IAliasExtMask
    // {
    //     Task<List<Alias>> ToListAsync();
    //     Task<Alias> SingleOrDefaultAsync(int id);
    // }

    // public interface ITransactionExtMask
    // {
    //     Task<List<Transaction>> ToListAsync();
    //     Task<Transaction> SingleOrDefaultAsync(int id);
    // }
}