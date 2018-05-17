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

        // TODO: Add tests for this method
        public async Task<int> AddPayeeAsync(PayeeCrudVm payeeVm) {
            try {
                return await AddPayeeAsync(payeeVm.Name, payeeVm.CategoryName);
            } catch (ModelValidationException ex) {
                if (ex.PropertyName == nameof(Payee.BudgetCategoryID)) {
                    throw new ModelValidationException(nameof(payeeVm.CategoryName), payeeVm.CategoryName,
                        $"There is no Budget Category named '{payeeVm.CategoryName}'");
                }
                throw;
            }
        }

        public async Task<int> AddPayeeAsync(string name, string categoryName) {
            BudgetCategory category = _context.GetCategories()
                .Where(c => c.Name == categoryName)
                .OrderByDescending(c => c.EffectiveFrom)
                .FirstOrDefault() ?? throw new ModelValidationException(nameof(Payee.BudgetCategoryID), null,
                    $"There is no Budget Category named '{categoryName}'");

            return await AddPayeeAsync(new Payee {
                Name = name,
                EffectiveFrom = DateTime.Now,
                BudgetCategoryID = category.ID
            });
        }

        public async Task<int> AddPayeeAsync(Payee payee) {
            ValidatePayee(payee);
            _context.AddPayee(payee);
            return await _context.SaveChangesAsync();
        }

        // TODO: Add tests for this method
        public async Task<int> UpdatePayeeAsync(PayeeCrudVm payeeVm) {
            try {
                return await UpdatePayeeAsync(payeeVm.NavId, payeeVm.Name, payeeVm.EffectiveFrom, payeeVm.CategoryName);
            } catch (ModelValidationException ex) {
                if (ex.PropertyName == nameof(Payee.BudgetCategoryID)) {
                    throw new ModelValidationException(nameof(payeeVm.CategoryName), payeeVm.CategoryName,
                        $"There is no Budget Category named '{payeeVm.CategoryName}'");
                }
                throw;
            }
        }

        // TODO: add tests for this method
        public async Task<int> UpdatePayeeAsync(int id, string name, DateTime effectiveFrom, string categoryName) {
            BudgetCategory category = _context.GetCategories()
                .Where(c => c.Name == categoryName)
                .OrderByDescending(c => c.EffectiveFrom)
                .FirstOrDefault() ?? throw new ModelValidationException(nameof(Payee.BudgetCategoryID), null,
                        $"There is no Budget Category named '{categoryName}'");

            Payee payee = await _context.GetPayees()
                .Extension().SingleOrDefaultAsync(p => p.ID == id) ?? throw new NullModelException($"Could not find Payee with Id = {id}");
            
            try {
                payee.Name = name;
                payee.EffectiveFrom = effectiveFrom;
                payee.BudgetCategoryID = category.ID;
                ValidatePayee(payee);
                _context.EditPayee(payee);
                return await _context.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException) {
                throw new ConcurrencyException();
            }
        }

        public async Task<int> UpdatePayeeAsync(int id, Payee payee) {
            ValidatePayee(payee);
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

        private void ValidatePayee(Payee payee) {
            ModelValidation<Payee>.ValidateModel(payee);
            if (_context.GetPayees().Any(p => p.ID != payee.ID && p.Name == payee.Name)) {
                throw new ModelValidationException(nameof(Payee.Name), payee.Name, "Name already in use");
            }
        }
    }
}