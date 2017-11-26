using ExpenseTracker.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Data.Repository
{
    public class Budget : IBudget
    {
        private readonly IDataAccess repo;
        public Budget(IDataAccess repository) { 
            repo = repository;
        }

        public IQueryable<BudgetCategory> GetCategories() {
            return repo.BudgetCategories();
        }

        public void AddBudgetCategory(BudgetCategory categoryToAdd) {
            repo.AddBudgetCategory(categoryToAdd);
        }
        public void RemoveBudgetCategory(BudgetCategory categoryToRemove) { 
            repo.DeleteBudgetCategory(categoryToRemove);
        }

        public IQueryable<Payee> GetPayees() {
            return repo.Payees();
        }

        public void AddPayee(Payee payeeToAdd) {
            repo.AddPayee(payeeToAdd);
        }

        public void RemovePayee(Payee payeeToRemove) {
            repo.DeletePayee(payeeToRemove);
        }

        public void UpdatePayee(Payee editedPayee) {
            repo.EditPayee(editedPayee);
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
            repo.AddTransaction(transactionToAdd);
        }

        public void RemoveTransaction(Transaction transactionToRemove) {
            repo.DeleteTransaction(transactionToRemove);
        }

        public void UpdateTransaction(Transaction editedTransaction) {
            repo.EditTransaction(editedTransaction);
        }

        public async Task<int> SaveChangesAsync() {
            return await repo.SaveChangesAsync();
        }
    }
}