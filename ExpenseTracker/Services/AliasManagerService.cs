using ExpenseTracker.Data;
using ExpenseTracker.Exceptions;
using ExpenseTracker.Models;
using ExpenseTracker.Repository;
using ExpenseTracker.Repository.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Services
{
    public class AliasManagerService : CommonService, IAliasManagerService
    {
        private readonly IBudgetRepo _context;

        public AliasManagerService(IBudgetRepo context) : base(context) {
            _context = context;
        }
        
        public async Task<Alias> GetSingleAliasAsync(int? id, bool includeAll = false) {
            if (id == null) {
                throw new NullIdException("No id specified");
            }

            var alias = await _context.GetAliases(includeAll).Extension().SingleOrDefaultAsync(a => a.ID == id);

            if (alias == null) {
                throw new IdNotFoundException($"No alias found for ID = {id}");
            }

            return alias;
        }

        public async Task<int> UpdateAliasAsync(int id, string name, int payeeId) {
            Alias originalAlias = _context.GetAliases()
                .FirstOrDefault(a => a.ID == id) ?? throw new IdNotFoundException($"No Alias found for Id = {id}");

            if (_context.GetAliases().Any(a => a.ID != id && a.Name == originalAlias.Name)) {
                throw new UniqueConstraintViolationException($"There is already and alias named '{name}'") {
                    PropertyName = nameof(Alias.Name),
                    PropertyValue = name
                };
            }

            try {
                originalAlias.Name = name;
                originalAlias.PayeeID = payeeId;
                _context.EditAlias(originalAlias);
                return await _context.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException) {
                throw new ConcurrencyException();
            }
        }

        // TODO: Remove this implementation
        public async Task<int> UpdateAliasAsync(int id, Alias alias) {
            if (id != alias.ID) {
                throw new IdMismatchException($"Id = {id} does not match alias Id of {alias.ID}");
            }
            if (_context.GetAliases().Any(a => a.ID != id && a.Name == alias.Name)) {
                throw new UniqueConstraintViolationException($"There is already and alias named '{alias.Name}'") {
                    PropertyName = nameof(Alias.Name),
                    PropertyValue = alias.Name
                };
            }
            try {
                _context.EditAlias(alias);
                return await _context.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException) {
                throw new ConcurrencyException();
            }
        }

        // TODO: Add tests for this method
        public async Task<int> AddAliasAsync(string name, int payeeId) {
            return await AddAliasAsync(new Alias {
                Name = name,
                PayeeID = payeeId
            });
        }

        public async Task<int> AddAliasAsync(Alias alias) {
            if (_context.GetAliases().Any(a => a.Name == alias.Name)) {
                throw new UniqueConstraintViolationException($"There is already and alias named '{alias.Name}'") {
                    PropertyName = nameof(Alias.Name),
                    PropertyValue = alias.Name
                };
            }
            _context.AddAlias(alias);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> RemoveAliasAsync(int id) {
            var alias = _context.GetAliases().SingleOrDefault(c => c.ID == id);
            if (alias != null) {
                _context.DeleteAlias(alias);
            }
            return await _context.SaveChangesAsync();
        }

        public bool AliasExists(int id) {
            return _context.GetAliases().Any(a => a.ID == id);
        }
    }
}