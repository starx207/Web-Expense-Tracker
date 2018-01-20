using ExpenseTracker.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Repository
{
    public interface ITransactionRepo : ISharedRepo
    {
        IQueryable<Transaction> GetOrderedTransactionQueryable(string orderBy, bool orderByDescending = false, bool includeAll = false);
        Task<Transaction> GetSingleTransactionAsync(int? id, bool includeAll = false);
        Task<int> AddTransactionAsync(Transaction transaction);
        Task<int> UpdateTransactionAsync(int id, Transaction transaction);
        Task<int> RemoveTransactionAsync(int id);
        bool TransactionExists(int id);
    }
}