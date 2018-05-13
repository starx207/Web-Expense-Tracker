using ExpenseTracker.Exceptions;
using ExpenseTracker.Models;
using ExpenseTracker.Repository;
using ExpenseTracker.Repository.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Services
{
  public class PayeeManagerService : CommonService, IPayeeManagerService
    {
        private readonly IBudgetRepo _context;

        public PayeeManagerService(IBudgetRepo context) : base(context) {
            _context = context;
        }

        public async Task<Payee> GetSinglePayeeAsync(int? id, bool includeAll = false) {
            if (id == null) {
                throw new NullIdException("No id specified");
            }

            var payee = await _context.GetPayees(includeAll, includeAll).Extension().SingleOrDefaultAsync(p => p.ID == id);
                
            if (payee == null) {
                throw new IdNotFoundException($"No payee found for ID = {id}");
            }

            return payee;
        }

        public async Task<int> AddPayeeAsync(string name, string categoryName) {
            // TODO: add model validation
            BudgetCategory category = _context.GetCategories()
                .Where(c => c.Name == categoryName)
                .OrderByDescending(c => c.EffectiveFrom)
                .FirstOrDefault() ?? throw new NullModelException($"There is no Budget Category named '{categoryName}'");

            return await AddPayeeAsync(new Payee {
                Name = name,
                EffectiveFrom = DateTime.Now,
                BudgetCategoryID = category.ID
            });
        }

        public async Task<int> AddPayeeAsync(Payee payee) {
            // TODO: Add model validation
            if (_context.GetPayees().Any(p => p.Name == payee.Name)) {
                throw new ModelValidationException($"There is already a payee named '{payee.Name}'") {
                    PropertyName = nameof(Payee.Name),
                    PropertyValue = payee.Name
                };
            }
            _context.AddPayee(payee);
            return await _context.SaveChangesAsync();
        }

        // TODO: add tests for this method
        public async Task<int> UpdatePayeeAsync(int id, string name, DateTime effectiveFrom, string categoryName) {
            // TODO: Add model validation
            BudgetCategory category = _context.GetCategories()
                .Where(c => c.Name == categoryName)
                .OrderByDescending(c => c.EffectiveFrom)
                .FirstOrDefault() ?? throw new NullModelException($"There is no Budget Category named '{categoryName}'");

            Payee payee = await _context.GetPayees()
                .Extension().SingleOrDefaultAsync(p => p.ID == id) ?? throw new NullModelException($"Could not find Payee with Id = {id}");
            
            try {
                payee.Name = name;
                payee.EffectiveFrom = effectiveFrom;
                payee.BudgetCategoryID = category.ID;
                _context.EditPayee(payee);
                return await _context.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException) {
                throw new ConcurrencyException();
            }
        }

        public async Task<int> UpdatePayeeAsync(int id, Payee payee) {
            // TODO: Add model validation
            if (id != payee.ID) {
                throw new IdMismatchException($"Id = {id} does not match payee Id of {payee.ID}");
            }
            try {
                _context.EditPayee(payee);
                return await _context.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException) {
                throw new ConcurrencyException();
            }
        }

        public async Task<int> RemovePayeeAsync(int id) {
            var payee = _context.GetPayees().SingleOrDefault(p => p.ID == id);

            if (payee != null) {
                _context.DeletePayee(payee);
            }

            return await _context.SaveChangesAsync();
        }

        public bool PayeeExists(int id) {
            return _context.GetPayees().Any(p => p.ID == id);
        }
    }
}