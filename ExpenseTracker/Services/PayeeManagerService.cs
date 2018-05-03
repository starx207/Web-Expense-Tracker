using ExpenseTracker.Exceptions;
using ExpenseTracker.Models;
using ExpenseTracker.Repository;
using ExpenseTracker.Repository.Extensions;
using Microsoft.EntityFrameworkCore;
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

        public async Task<int> AddPayeeAsync(Payee payee) {
            if (_context.GetPayees().Any(p => p.Name == payee.Name)) {
                throw new UniqueConstraintViolationException($"There is already a payee with Name = {payee.Name}");
            }
            _context.AddPayee(payee);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdatePayeeAsync(int id, Payee payee) {
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