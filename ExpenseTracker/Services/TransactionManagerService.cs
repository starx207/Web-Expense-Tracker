using ExpenseTracker.Exceptions;
using ExpenseTracker.Models;
using ExpenseTracker.Repository;
using ExpenseTracker.Repository.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Services
{
  public class TransactionManagerService : CommonService, ITransactionManagerService
    {
        private readonly IBudgetRepo _context;

        public TransactionManagerService(IBudgetRepo context) : base(context) {
            _context = context;
        }

        public bool TransactionExists(int id) {
            return _context.GetTransactions().Any(t => t.ID == id);
        }

        public IQueryable<Transaction> GetTransactions(bool includeOverride = false, bool includePayee = false) {
            return _context.GetTransactions(includePayee, includeOverride);
        }

        public async Task<Transaction> GetSingleTransactionAsync(int? id, bool includeAll = false) {
            if (id == null) {
                throw new NullIdException("No id specified");
            }

            var transaction = await _context.GetTransactions(includeAll, includeAll).Extension().SingleOrDefaultAsync(t => t.ID == id);

            if (transaction == null) {
                throw new IdNotFoundException($"No transaction found for ID = {id}");
            }

            return transaction;
        }

        public async Task<int> AddTransactionAsync(Transaction transaction) {
            _context.AddTransaction(transaction);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateTransactionAsync(int id, Transaction transaction) {
            if (id != transaction.ID) {
                throw new IdMismatchException($"Id = {id} does not equal transaction id of {transaction.ID}");
            }

            try {
                _context.EditTransaction(transaction);
                return await _context.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException) {
                throw new ConcurrencyException();
            }                
        }

        public async Task<int> RemoveTransactionAsync(int id) {
            var transaction = _context.GetTransactions().SingleOrDefault(t => t.ID == id);
            if (transaction != null) {
                _context.DeleteTransaction(transaction);
            }
            return await _context.SaveChangesAsync();
        }
    }
}