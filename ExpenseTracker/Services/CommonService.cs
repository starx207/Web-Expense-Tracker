using ExpenseTracker.Models;
using ExpenseTracker.Repository;
using System.Linq;

namespace ExpenseTracker.Services
{
    public class CommonService : SharedServiceFunctions, ICommonService
    {
        private readonly IBudgetRepo _context;

        public CommonService(IBudgetRepo context) {
            _context = context;
        }

        public IQueryable<BudgetCategory> GetOrderedCategories(string orderBy, bool orderByDescending = false) {
            return SortQueryableByProperty(_context.GetCategories(), orderBy, orderByDescending);
        }
        
        public IQueryable<Payee> GetOrderedPayees(string orderBy, bool orderByDescending = false, bool includeAll = false) {
            var retVals = _context.GetPayees(includeAll).AsQueryable();
            return SortQueryableByProperty(retVals, orderBy, orderByDescending);
        }
    }
}