/*
    This example code taken from http://blogs.clariusconsulting.net/kzu/making-extension-methods-amenable-to-mocking/
 */
 
using ExpenseTracker.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Repository.Extensions
{
    public interface ICategoryExtMask
    {
        Task<List<BudgetCategory>> ToListAsync();
    }

    public interface IPayeeExtMask
    {
        Task<List<Payee>> ToListAsync();
    }

    public interface IAliasExtMask
    {
        Task<List<Alias>> ToListAsync();
    }

    public interface ITransactionExtMask
    {
        Task<List<Transaction>> ToListAsync();
    }
}