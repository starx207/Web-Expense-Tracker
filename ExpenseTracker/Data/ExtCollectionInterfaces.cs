/*
    This example code taken from http://blogs.clariusconsulting.net/kzu/making-extension-methods-amenable-to-mocking/
 */
 
using ExpenseTracker.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Data
{
    public interface ICategoryExtMask
    {
        Task<List<BudgetCategory>> ToListAsync();
        Task<BudgetCategory> SingleOrDefaultAsync(int id);
    }

    public interface IPayeeExtMask
    {
        Task<List<Payee>> ToListAsync();
        Task<Payee> SingleOrDefaultAsync(int id);
        IQueryable<Payee> IncludeAll();
    }

    public interface IAliasExtMask
    {
        Task<List<Alias>> ToListAsync();
        Task<Alias> SingleOrDefaultAsync(int id);
        IQueryable<Alias> IncludeAll();
    }

    public interface ITransactionExtMask
    {
        Task<List<Transaction>> ToListAsync();
        Task<Transaction> SingleOrDefaultAsync(int id);
        IQueryable<Transaction> IncludeAll();
    }
}