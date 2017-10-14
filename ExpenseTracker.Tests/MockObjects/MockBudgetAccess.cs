using ExpenseTracker.Data.Repository;
using ExpenseTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Tests.Mock
{
    public class MockBudgetAccess : IBudgetAccess
    {
        private List<Transaction> transactions;
        private List<Payee> payees;
        private List<BudgetCategory> categories;

        public MockBudgetAccess() {
            transactions = new List<Transaction>();
            payees = new List<Payee>();
            categories = new List<BudgetCategory>();
        }

        public MockBudgetAccess(IEnumerable<Transaction> _transactions, IEnumerable<Payee> _payees, IEnumerable<BudgetCategory> _categories) {
            transactions = _transactions.ToList();
            payees = _payees.ToList();
            categories = _categories.ToList();
        }

        public IQueryable<Transaction> Transactions() {
            return transactions.AsQueryable();
        }
        public IQueryable<Payee> Payees() {
            return payees.AsQueryable();
        }
        public IQueryable<BudgetCategory> BudgetCategories() {
            return categories.AsQueryable();
        }
        public void DeleteTransaction(Transaction transactionToDelete) {
            transactions.Remove(transactionToDelete);
        }
        public void DeletePayee(Payee payeeToDelete) {
            payees.Remove(payeeToDelete);
        }
        public void DeleteBudgetCategory(BudgetCategory categoryToDelete) {
            categories.Remove(categoryToDelete);
        }
        public void AddTransaction(Transaction transactionToAdd) {
            transactions.Add(transactionToAdd);
        }
        public void AddPayee(Payee payeeToAdd) {
            payees.Add(payeeToAdd);
        }
        public void AddBudgetCategory(BudgetCategory categoryToAdd) {
            categories.Add(categoryToAdd);
        }
        public void EditTransaction(Transaction transactionToEdit) {
            Transaction transactionToRemove = null;
            foreach (var transaction in transactions) {
                if (transaction.ID == transactionToEdit.ID) {
                    transactionToRemove = transaction;
                }
            }
            if (transactionToRemove != null) {
                transactions.Remove(transactionToRemove);
                transactions.Add(transactionToEdit);
            }
        }
        public void EditPayee(Payee payeeToEdit) {
            Payee payeeToRemove = null;
            foreach (var payee in payees) {
                if (payee.ID == payeeToEdit.ID) {
                    payeeToRemove = payee;
                }
            }
            if (payeeToRemove != null) {
                payees.Remove(payeeToRemove);
                payees.Add(payeeToEdit);
            }
        }
        public void EditBudgetCategory(BudgetCategory categoryToEdit) {
            BudgetCategory categoryToRemove = null;
            foreach (var category in categories) {
                if (category.ID == categoryToEdit.ID) {
                    categoryToRemove = category;
                }
            }
            if (categoryToRemove != null) {
                categories.Remove(categoryToRemove);
                categories.Add(categoryToEdit);
            }
        }

        public async Task SaveChangesAsync() {
            await new Task(PretendToDoSomething);
        }

        private void PretendToDoSomething() { }
    }
}