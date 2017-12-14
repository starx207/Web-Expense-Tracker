using ExpenseTracker.Data.Repository;
using ExpenseTracker.Models;
using ExpenseTracker.Tests.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Tests.Repository
{
    [TestClass]
    public class Budget_Tests
    {
        private IDataAccess repo;
        private IBudget budget;
        private Dictionary<int, string> categoryRef;
        private Dictionary<int, string> payeeRef;
        private Dictionary<int, string> aliasRef;
        private Dictionary<int, double> transactionRef;
        private int categoryCount, payeeCount, aliasCount, transactionCount;

        [TestInitialize]
        public void InitializeTestData() {
            // Create in-memory BudgetCategories
            List<BudgetCategory> categories = TestInitializer.CreateTestCategories();
            // Add categories and ids to dictionary for verification
            categoryRef = new Dictionary<int, string>();
            foreach (var category in categories) {
                categoryRef.Add(category.ID, category.Name);
            }
            // Record the number of categories at the start of the test
            categoryCount = categories.Count;

            // Create in-memory Payees
            List<Payee> payees = TestInitializer.CreateTestPayees(categories.AsQueryable());
            // Add payees and ids to dictionary for verification
            payeeRef = new Dictionary<int, string>();
            foreach (var payee in payees) {
                payeeRef.Add(payee.ID, payee.Name);
            }
            // Record the number of payees at the start of the test
            payeeCount = payees.Count;

            // Create in-memory Aliases
            List<Alias> aliases = TestInitializer.CreateTestAliases(payees.AsQueryable());
            aliasRef = new Dictionary<int, string>();
            foreach (var alias in aliases) {
                aliasRef.Add(alias.ID, alias.Name);
            }
            aliasCount = aliases.Count;

            // Create in-memory Transactions
            List<Transaction> transactions = TestInitializer.CreateTestTransactions(categories.AsQueryable(), payees.AsQueryable());
            transactionRef = new Dictionary<int, double>();
            foreach (var trans in transactions) {
                transactionRef.Add(trans.ID, trans.Amount);
            }
            transactionCount = transactions.Count;

            // Iniitlize the IBudgetAccess repo with in-memory data
            repo = new MockDataAccess(transactions, payees, categories, aliases);
        }

        [TestMethod]
        public void BudgetImplementsIBudget()
        {
            try {
                budget = new Budget(repo);
            } catch {
                Assert.Fail("Budget class does not implement IBudget");
            }
            // if no error, Budget implements IBudget. Test passes
            Assert.IsTrue(true);
        }

        #region "BudgetCategory Tests"
            [TestMethod]
            public void GetAllBudgetCategoriesReturnsCorrectCount() {
                budget = new Budget(repo);
                IQueryable<BudgetCategory> allCategories;

                allCategories = budget.GetCategories();

                Assert.AreEqual(categoryCount, allCategories.Count(), "GetCategories() returned wrong number of BudgetCategories");
            }

            [DataTestMethod]
            [DataRow(1), DataRow(2), DataRow(3), DataRow(4)]
            public void GetAllBudgetCategoriesReturnsCorrectCategories(int id) {
                budget = new Budget(repo);
                BudgetCategory category;
                IQueryable<BudgetCategory> allCategories;
                string expectedName = categoryRef[id];

                allCategories = budget.GetCategories();
                category = allCategories.Where(c => c.ID == id).First();

                Assert.AreEqual(expectedName, category.Name, $"BudgetCategory with ID = {id} should have Name = {expectedName}");
            }

            [TestMethod]
            public void AddANewBudgetCategory() {
                budget = new Budget(repo);
                int testID = repo.BudgetCategories().OrderByDescending(c => c.ID).Select(c => c.ID).First() + 1;
                string testName = "Insurance";
                BudgetCategory newCategory = new BudgetCategory {
                    ID = testID,
                    Name = testName,
                    Amount = 280.94,
                    BeginEffectiveDate = new DateTime(2016, 09, 01),
                    EndEffectiveDate = null,
                    Type = BudgetType.Expense
                };
                int newCount;

                budget.AddBudgetCategory(newCategory);
                newCount = budget.GetCategories().Count();

                Assert.IsTrue(newCount == categoryCount + 1, "No category was added");

                BudgetCategory retrievedCategory;
                try {
                    retrievedCategory = budget.GetCategories().Where(c => c.Name == testName).First();
                    Assert.AreEqual(testID, retrievedCategory.ID, $"'{testName}' should have ID = {testID}");
                } catch {
                    Assert.Fail($"No '{testName}' category was found");
                }
            }

            [TestMethod]
            public void RemoveABudgetCategory() {
                budget = new Budget(repo);
                int testID = repo.BudgetCategories().Select(c => c.ID).First();
                BudgetCategory remove = budget.GetCategories().Where(c => c.ID == testID).First();
                int newCount;

                budget.RemoveBudgetCategory(remove);
                newCount = budget.GetCategories().Count();

                Assert.IsTrue(newCount == categoryCount - 1, "No category was removed");

                BudgetCategory removedCategory;
                Assert.ThrowsException<InvalidOperationException>(() =>
                    removedCategory = budget.GetCategories().Where(c => c.ID == testID).First()
                , $"Category with id = {testID} should have been removed");
            }

            [TestMethod]
            public void EditABudgetCategory() {
                budget = new Budget(repo);
                BudgetCategory categoryToEdit = budget.GetCategories().First();
                int testID = categoryToEdit.ID;
                string originalName = categoryToEdit.Name;
                string newName = originalName + "_modified";

                categoryToEdit.Name = newName;
                budget.UpdateBudgetCategory(categoryToEdit);

                BudgetCategory editedCategory = budget.GetCategories().First(c => c.ID == testID);

                Assert.AreEqual(newName, editedCategory.Name, "The Budget Category name was not updated");
            }
        #endregion
    
        #region "Payee Tests"

            [TestMethod]
            public void GetAllPayeesReturnsCorrectCount() {
                budget = new Budget(repo);
                IQueryable<Payee> allPayees;

                allPayees = budget.GetPayees();

                Assert.AreEqual(payeeCount, allPayees.Count(), "The wrong number of Payees was returned");
            }

            [DataTestMethod]
            [DataRow(1), DataRow(2), DataRow(3), DataRow(4)]
            public void GetPayeesReturnsCorrectPayees(int id) {
                budget = new Budget(repo);
                Payee payee;
                IQueryable<Payee> allPayees;
                string expectedName = payeeRef[id];

                allPayees = budget.GetPayees();
                payee = allPayees.Where(p => p.ID == id).First();

                Assert.AreEqual(expectedName, payee.Name, $"Id = {id} should return '{expectedName}'");
            }

            [TestMethod]
            public void AddAPayee() {
                budget = new Budget(repo);
                int testID = budget.GetPayees().OrderByDescending(p => p.ID).Select(p => p.ID).First() + 1;
                string payeeName = "Sweetwater";
                BudgetCategory category = budget.GetCategories().First();
                Payee payee = new Payee {
                    ID = testID,
                    Name = payeeName,
                    BeginEffectiveDate = new DateTime(2017, 3, 25),
                    EndEffectiveDate = null,
                    BudgetCategoryID = category.ID,
                    Category = category
                };
                int newCount;

                budget.AddPayee(payee);
                newCount = budget.GetPayees().Count();

                Assert.AreEqual(payeeCount + 1, newCount, "No Payee was added");

                Payee retrievedPayee;
                try {
                    retrievedPayee = budget.GetPayees().Where(p => p.Name == payeeName).First();
                    Assert.AreEqual(testID, retrievedPayee.ID, $"'{payeeName}' should have ID = {testID}");
                } catch {
                    Assert.Fail($"No payee named '{payeeName}' was found");
                }
            }

            [TestMethod]
            public void DeleteAPayee() {
                budget = new Budget(repo);
                Payee payeeToRemove = budget.GetPayees().First();
                int testID = payeeToRemove.ID;
                int newCount;

                budget.RemovePayee(payeeToRemove);
                newCount = budget.GetPayees().Count();

                Assert.AreEqual(payeeCount - 1, newCount, "No payee was removed");

                Payee retrievedPayee;
                Assert.ThrowsException<InvalidOperationException>(() =>
                    retrievedPayee = budget.GetPayees().Where(p => p.ID == testID).First()
                , $"Payee with id = {testID} should have been removed");
            }

            [TestMethod]
            public void EditAPayee() {
                budget = new Budget(repo);
                Payee payeeToEdit = budget.GetPayees().First();
                int testID = payeeToEdit.ID;
                string originalName = payeeToEdit.Name;

                string newName = originalName + " plus something extra";

                payeeToEdit.Name = newName;
                budget.UpdatePayee(payeeToEdit);

                Payee editedPayee = budget.GetPayees().Where(p => p.ID == testID).First();

                Assert.AreEqual(newName, editedPayee.Name, $"Payee name was not updated");

                BudgetCategory newCategory = budget.GetCategories().Where(c => c.ID != editedPayee.BudgetCategoryID).First();

                testID = editedPayee.ID;
                editedPayee.Category = newCategory;
                editedPayee.BudgetCategoryID = newCategory.ID;

                budget.UpdatePayee(editedPayee);

                payeeToEdit = budget.GetPayees().Where(p => p.ID == testID).First();

                Assert.AreEqual(newCategory.Name, payeeToEdit.Category.Name, "The payee was not correctly reassigned to the new category");
                Assert.AreEqual(newCategory.ID, payeeToEdit.BudgetCategoryID, "The payee was not correctly reassinged to the new category");
            }

        #endregion

        #region Alias Tests
            [TestMethod]
            public void GetAliasesReturnsCorrectCount() {
                budget = new Budget(repo);
                IQueryable<Alias> allAliases;

                allAliases = budget.GetAliases();

                Assert.AreEqual(aliasCount, allAliases.Count(), "The wrong number of Aliases was returned");
            }

            [DataTestMethod]
            [DataRow(1), DataRow(2), DataRow(3)]
            public void GetAliasesReturnsCorrectAliases(int id) {
                budget = new Budget(repo);
                Alias alias;
                IQueryable<Alias> allAliases;
                string expectedName = aliasRef[id];

                allAliases = budget.GetAliases();
                alias = allAliases.Where(a => a.ID == id).First();

                Assert.AreEqual(expectedName, alias.Name, $"Id = {id}, should return '{expectedName}'");
            }

            [TestMethod]
            public void AddAnAlias() {
                budget = new Budget(repo);
                int testID = budget.GetAliases().OrderByDescending(a => a.ID).First().ID + 1;
                string aliasName = "Yet Another Walmart Alias";
                Payee payee = budget.GetPayees().First();
                Alias newAlias = new Alias {
                    ID = testID,
                    Name = aliasName,
                    PayeeID = payee.ID,
                    AliasForPayee = payee
                };
                int newCount;

                budget.AddAlias(newAlias);
                newCount = budget.GetAliases().Count();

                Assert.AreEqual(aliasCount + 1, newCount, "No Alias was added");

                Alias retrievedAlias;
                try {
                    retrievedAlias = budget.GetAliases().Where(a => a.Name == aliasName).First();
                    Assert.AreEqual(testID, retrievedAlias.ID, $"'{aliasName}' should have Id = {testID}");
                } catch {
                    Assert.Fail($"No alias with name = '{aliasName}' was found");
                }
            }

            [TestMethod]
            public void DeleteAnAlias() {
                budget = new Budget(repo);
                Alias aliasToRemove = budget.GetAliases().First();
                int testID = aliasToRemove.ID;
                int newCount;

                budget.RemoveAlias(aliasToRemove);
                newCount = budget.GetAliases().Count();

                Assert.AreEqual(aliasCount - 1, newCount, "No alias was removed");

                Alias retrievedAlias;
                Assert.ThrowsException<InvalidOperationException>(() =>
                    retrievedAlias = budget.GetAliases().Where(a => a.ID == testID).First()
                , $"Alias with Id = {testID} should have been removed");
            }

            [TestMethod]
            public void EditAnAlias() {
                budget = new Budget(repo);
                Alias aliasToEdit = budget.GetAliases().First();
                int testID = aliasToEdit.ID;
                string originalName = aliasToEdit.Name;
                int originalPayeeID = aliasToEdit.PayeeID;
                
                string newName = originalName + "_modified";
                Payee newPayee = budget.GetPayees().Where(p => p.ID != originalPayeeID).First();

                aliasToEdit.Name = newName;
                budget.UpdateAlias(aliasToEdit);

                Alias editedAlias = budget.GetAliases().Where(a => a.ID == testID).First();

                Assert.AreEqual(newName, editedAlias.Name, "Alias name was not updated");

                editedAlias.PayeeID = newPayee.ID;
                editedAlias.AliasForPayee = newPayee;
                budget.UpdateAlias(editedAlias);

                aliasToEdit = budget.GetAliases().Where(a => a.ID == testID).First();

                Assert.AreEqual(newPayee.ID, aliasToEdit.PayeeID, "The alias was not correctly reassigned to a new payee");
            }
        #endregion

        #region Transaction Tests
            [TestMethod]
            public void GetAllTransactionsReturnsCorrectCount() {
                budget = new Budget(repo);
                IQueryable<Transaction> allTransactions;

                allTransactions = budget.GetTransactions();

                Assert.AreEqual(transactionCount, allTransactions.Count(), "The wrong number of Transactions was returned");
            }

            [DataTestMethod]
            [DataRow(1), DataRow(2), DataRow(3), DataRow(4)]
            public void GetTransactionsReturnsCorrectTransaction(int id) {
                budget = new Budget(repo);
                Transaction transaction;
                IQueryable<Transaction> allTransactions;
                double expectedAmount = transactionRef[id];

                allTransactions = budget.GetTransactions();
                transaction = allTransactions.Where(p => p.ID == id).First();

                Assert.AreEqual(expectedAmount, transaction.Amount, $"Id = {id} should return '{expectedAmount}'");
            }

            [TestMethod]
            public void AddATransaction() {
                budget = new Budget(repo);
                int testID = budget.GetTransactions().OrderByDescending(p => p.ID).Select(p => p.ID).First() + 1;
                double transactionAmount = 123123;
                BudgetCategory category = budget.GetCategories().First();
                Payee payee = budget.GetPayees().First();
                Transaction trans = new Transaction {
                    ID = testID,
                    Date = DateTime.Now,
                    Amount = transactionAmount,
                    PayeeID = payee.ID,
                    PayableTo = payee,
                    OverrideCategoryID = category.ID,
                    OverrideCategory = category
                };
                int newCount;

                budget.AddTransaction(trans);
                newCount = budget.GetTransactions().Count();

                Assert.AreEqual(transactionCount + 1, newCount, "No Transaction was added");

                Transaction retrievedTransaction;
                try {
                    retrievedTransaction = budget.GetTransactions().Where(p => p.ID == testID).First();
                    Assert.AreEqual(transactionAmount, retrievedTransaction.Amount, $"Transaction with ID = {testID} should have amount = {transactionAmount}");
                    Assert.AreEqual(payee.ID, retrievedTransaction.PayeeID, $"Transaction with ID = {testID} should have PayeeID = {payee.ID}");
                    Assert.AreEqual(category.ID, retrievedTransaction.OverrideCategoryID, $"Transaction with ID = {testID} should have OverrideCategoryID = {category.ID}");
                } catch {
                    Assert.Fail($"No Transaction with ID = {testID} was found");
                }
            }

            [TestMethod]
            public void DeleteATransaction() {
                budget = new Budget(repo);
                Transaction transToRemove = budget.GetTransactions().First();
                int testID = transToRemove.ID;
                int newCount;

                budget.RemoveTransaction(transToRemove);
                newCount = budget.GetTransactions().Count();

                Assert.AreEqual(transactionCount - 1, newCount, "No transaction was removed");

                Transaction retrievedTrans;
                Assert.ThrowsException<InvalidOperationException>(() =>
                    retrievedTrans = budget.GetTransactions().Where(p => p.ID == testID).First()
                , $"Transaction with id = {testID} should have been removed");
            }

            [TestMethod]
            public void EditATransaction() {
                budget = new Budget(repo);
                Transaction transToEdit = budget.GetTransactions().First();
                int testID = transToEdit.ID;
                double originalAmount = transToEdit.Amount;

                double newAmount = originalAmount + 50;

                transToEdit.Amount = newAmount;
                budget.UpdateTransaction(transToEdit);

                Transaction editedTrans = budget.GetTransactions().Where(p => p.ID == testID).First();

                Assert.AreEqual(newAmount, editedTrans.Amount, $"Transaction amount was not updated");

                BudgetCategory newCategory = budget.GetCategories().Where(c => c.ID != editedTrans.OverrideCategoryID).First();

                testID = editedTrans.ID;
                editedTrans.OverrideCategory = newCategory;
                editedTrans.OverrideCategoryID = newCategory.ID;

                budget.UpdateTransaction(editedTrans);

                transToEdit = budget.GetTransactions().Where(p => p.ID == testID).First();

                Assert.AreEqual(newCategory.Name, transToEdit.OverrideCategory.Name, "The transaction was not correctly reassigned to the new category");
                Assert.AreEqual(newCategory.ID, transToEdit.OverrideCategoryID, "The transaction was not correctly reassinged to the new category");
            }

            [DataTestMethod]
            [DataRow(100.1254), DataRow(10.1234), DataRow(0.1), DataRow(-75.0349)]
            public void RoundAmountToNearestCentOnAdd(double testAmount) {
                budget = new Budget(repo);
                int testID = budget.GetTransactions().OrderByDescending(t => t.ID).First().ID + 1;
                double roundedAmount = Math.Round(testAmount, 2, MidpointRounding.AwayFromZero);
                Transaction testTrans = new Transaction {
                    ID = testID,
                    Date = DateTime.Parse("10/16/2017"),
                    Amount = testAmount
                };

                budget.AddTransaction(testTrans);

                Transaction retrievedTrans = budget.GetTransactions().Where(t => t.ID == testID).First();

                Assert.AreEqual(roundedAmount, retrievedTrans.Amount, $"Amount = {testAmount} should be rounded to {roundedAmount} when transaction is added");
            }

            [DataTestMethod]
            [DataRow(100.1254), DataRow(10.1234), DataRow(0.1), DataRow(-75.0349)]
            public void RoundAmountToNearestCentOnUpdate(double testAmount) {
                budget = new Budget(repo);
                Transaction testTrans = budget.GetTransactions().First();
                double roundedAmount = Math.Round(testAmount, 2, MidpointRounding.AwayFromZero);
                testTrans.Amount = testAmount;

                budget.UpdateTransaction(testTrans);

                Transaction retrievedTrans = budget.GetTransactions().Where(t => t.ID == testTrans.ID).First();

                Assert.AreEqual(roundedAmount, retrievedTrans.Amount, $"Amount = {testAmount} should be rounded to {roundedAmount} when transaction is edited");
            }
        #endregion
    }
}
