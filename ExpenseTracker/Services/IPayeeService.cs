using ExpenseTracker.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Services
{
    public interface IPayeeService
    {
        Task<List<Payee>> GetOrderedPayeeListAsync(bool includeAll = false);
        IQueryable<Payee> GetOrderedPayeeQueryable();
        Task<Payee> GetSinglePayeeAsync(int? id, bool includeAll = false);
        Task<int> AddPayeeAsync(Payee payee);
        Task<int> UpdatePayeeAsync(int id, Payee payee);
        Task<int> RemovePayeeAsync(int id);
        bool PayeeExists(int id);
    }
}