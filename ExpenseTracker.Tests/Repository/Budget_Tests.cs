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
        private int categoryCount, payeeCount;

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

            // Create in-memory Transactions
            List<Transaction> transactions = new List<Transaction>();

            // Iniitlize the IBudgetAccess repo with in-memory data
            repo = new MockDataAccess(transactions, payees, categories);
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
            DateTime orignalBegin = payeeToEdit.BeginEffectiveDate;
            DateTime? originalEnd = payeeToEdit.EndEffectiveDate;
            BudgetCategory originalCategory = payeeToEdit.Category;
            int? originalCategoryID = payeeToEdit.BudgetCategoryID;

            string newName = originalName += " plus something extra";

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
    }
}
