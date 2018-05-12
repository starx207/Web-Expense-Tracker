using ExpenseTracker.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Services
{
    public interface IAliasManagerService : ICommonService
    {
        Task<Alias> GetSingleAliasAsync(int? id, bool includeAll = false);
        Task<int> UpdateAliasAsync(int id, Alias alias);
        Task<int> UpdateAliasAsync(int id, string name, string payeeName);
        Task<int> UpdateAliasAsync(int id, string name, int payeeId);
        Task<int> AddAliasAsync(Alias alias);
        Task<int> AddAliasAsync(string name, string payeeName);
        Task<int> AddAliasAsync(string name, int payeeId);
        Task<int> RemoveAliasAsync(int id);
        bool AliasExists(int id);
    }
}