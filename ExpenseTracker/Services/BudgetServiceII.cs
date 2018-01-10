using ExpenseTracker.Models;
using ExpenseTracker.Repository;
using ExpenseTracker.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Services
{
    public class BudgetServiceII : IBudgetService
    {
        private readonly IBudgetRepo repo;
        public BudgetServiceII(IBudgetRepo repository) { 
            repo = repository;
        }

        public IQueryable<BudgetCategory> GetCategories() {
            return repo.BudgetCategories();
        }

        public async Task<List<BudgetCategory>> GetCategoriesAsync(string orderBy = "", bool descending = false) {
            if (typeof(BudgetCategory).GetProperty(orderBy) != null) {
                return await repo.BudgetCategoriesAsync(orderBy, descending);
            }
            return await repo.BudgetCategoriesAsync();
        }

        public async Task<BudgetCategory> GetCategoryAsync(int? id) {
            if (id == null) {
                throw new NullIdException("No id specified");
            }

            BudgetCategory category = await Task.Factory.StartNew(() => GetCategories().SingleOrDefault(c => c.ID == id));
            if (category == null) {
                throw new IdNotFoundException($"No category found for ID = {id}");
            }
            return category;
        }

        public async Task AddBudgetCategoryAsync(BudgetCategory categoryToAdd) {
            repo.AddBudgetCategory(categoryToAdd);
            await repo.SaveChangesAsync();
        }

        public async Task RemoveBudgetCategoryAsync(int id) {
            BudgetCategory category;
            try {
                category = await GetCategoryAsync(id);
                repo.DeleteBudgetCategory(category);
                await repo.SaveChangesAsync();
            } catch (Exception ex) {
                if (!(ex is NullIdException || ex is IdNotFoundException)) {
                    throw;
                }
            }
        }

        public async Task UpdateBudgetCategoryAsync(int id, BudgetCategory category) {
            if (id != category.ID) {
                throw new IdMismatchException($"Id = {id} does not match category.ID = {category.ID}");
            }
            repo.EditBudgetCategory(category);
            await repo.SaveChangesAsync();
        }

        public IQueryable<Payee> GetPayees() {
            return repo.Payees();
        }

        public async Task<Payee> GetPayeeAsync(int? id) {
            if (id == null) {
                throw new NullIdException("No id specified");
            }

            Payee payee = await Task.Factory.StartNew(() => GetPayees().SingleOrDefault(p => p.ID == id));
            if (payee == null) {
                throw new IdNotFoundException($"No payee found for ID = {id}");
            }
            return payee;
        }

        public async Task AddPayeeAsync(Payee payeeToAdd) {
            repo.AddPayee(payeeToAdd);
            await repo.SaveChangesAsync();
        }

        public async Task RemovePayeeAsync(int id) {
            Payee payee;
            try {
                payee = await GetPayeeAsync(id);
                repo.DeletePayee(payee);
                await repo.SaveChangesAsync();
            } catch (Exception ex) {
                if (!(ex is NullIdException || ex is IdNotFoundException)) {
                    throw;
                }
            }
        }

        public async Task UpdatePayeeAsync(int id, Payee payee) {
            if (id != payee.ID) {
                throw new IdMismatchException($"ID = {id} does not match payee.ID = {payee.ID}");
            }
            repo.EditPayee(payee);
            await repo.SaveChangesAsync();
        }

        public IQueryable<Alias> GetAliases() {
            return repo.Aliases();
        }

        public void AddAlias(Alias aliasToAdd) {
            repo.AddAlias(aliasToAdd);
        }

        public void RemoveAlias(Alias aliasToRemove) {
            repo.DeleteAlias(aliasToRemove);
        }

        public void UpdateAlias(Alias editedAlias) {
            repo.EditAlias(editedAlias);
        }

        public IQueryable<Transaction> GetTransactions() {
            return repo.Transactions();
        }

        public void AddTransaction(Transaction transactionToAdd) {
            transactionToAdd.Amount = RoundTransaction(transactionToAdd.Amount);
            repo.AddTransaction(transactionToAdd);
        }

        public void RemoveTransaction(Transaction transactionToRemove) {
            repo.DeleteTransaction(transactionToRemove);
        }

        public void UpdateTransaction(Transaction editedTransaction) {
            editedTransaction.Amount = RoundTransaction(editedTransaction.Amount);
            repo.EditTransaction(editedTransaction);
        }

        public async Task<int> SaveChangesAsync() {
            return await repo.SaveChangesAsync();
        }

        private double RoundTransaction(double originalAmt) {
            return Math.Round(originalAmt, 2, MidpointRounding.AwayFromZero);
        }
    }
}