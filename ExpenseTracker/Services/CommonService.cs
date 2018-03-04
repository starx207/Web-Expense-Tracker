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

        public IQueryable<BudgetCategory> GetCategories(bool currentOnly = true) {
            if (currentOnly) {
                var query = from category in _context.GetCategories()
                            where category.EffectiveFrom == _context.GetCategories().Where(c => c.Name == category.Name).OrderByDescending(c => c.EffectiveFrom).First().EffectiveFrom
                            select category;

                return query;
            }
            return _context.GetCategories();
        }
        
        public IQueryable<Payee> GetPayees(bool includeCategory = false, bool includeAliases = false, bool includeTransactions = false, bool currentOnly = true) {
            if (currentOnly) {
                var query = from payee in _context.GetPayees(includeCategory, includeAliases, includeTransactions)
                            where payee.EffectiveFrom == _context.GetPayees(false, false, false).Where(p => p.Name == payee.Name).OrderByDescending(p => p.EffectiveFrom).First().EffectiveFrom
                            select payee;

                return query;
            }
            return _context.GetPayees(includeCategory, includeAliases, includeTransactions);
        }
    }
}