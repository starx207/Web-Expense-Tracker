using ExpenseTracker.Models;
using ExpenseTracker.Repository;
using System.Linq;

namespace ExpenseTracker.Services
{
    public class CommonService : ICommonService
    {
        private readonly IBudgetRepo _context;

        public CommonService(IBudgetRepo context) {
            _context = context;
        }

        public IQueryable<BudgetCategory> GetCategories() {
            return _context.GetCategories();
        }
        
        public IQueryable<Payee> GetPayees(bool includeCategory = false, bool includeAliases = false, bool includeTransactions = false) {
            return _context.GetPayees(includeCategory, includeAliases, includeTransactions);
        }
    }
}