using ExpenseTracker.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Services
{
    public interface IAliasService
    {
        Task<Alias> GetSingleAliasAsync(int? id, bool includeAll = false);
        Task<int> UpdateAliasAsync(int id, Alias alias);
        Task<int> AddAliasAsync(Alias alias);
        Task<int> RemoveAliasAsync(int id);
        bool AliasExists(int id);
    }
}