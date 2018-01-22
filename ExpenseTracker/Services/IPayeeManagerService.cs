using ExpenseTracker.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExpenseTracker.Services
{
    public interface IPayeeManagerService : ICommonService
    {
        Task<Payee> GetSinglePayeeAsync(int? id, bool includeAll = false);
        Task<int> AddPayeeAsync(Payee payee);
        Task<int> UpdatePayeeAsync(int id, Payee payee);
        Task<int> RemovePayeeAsync(int id);
        bool PayeeExists(int id);
    }
}