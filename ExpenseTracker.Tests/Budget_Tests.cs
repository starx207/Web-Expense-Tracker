using ExpenseTracker.Data.Repository;
using ExpenseTracker.Models;
using ExpenseTracker.Tests.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Tests
{
    [TestClass]
    public class Budget_Tests
    {
        private IBudgetAccess repo;
        private Dictionary<int, string> categoryRef;
        private int categoryCount;

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

            // Create in-memory Transactions
            List<Transaction> transactions = new List<Transaction>();
            // Create in-memory Payees
            List<Payee> payees = new List<Payee>();

            // Iniitlize the IBudgetAccess repo with in-memory data
            repo = new MockBudgetAccess(transactions, payees, categories);
        }

        [TestMethod]
        public void BudgetImplementsIBudget()
        {
            IBudget budget;
            try {
                budget = new Budget(repo);
            } catch {
                Assert.Fail("Budget class does not implement IBudget");
            }
            // if no error, Budget implements IBudget. Test passes
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void GetAllBudgetCategoriesReturnsCorrectCount() {
            IBudget budget = new Budget(repo);
            IQueryable<BudgetCategory> allCategories;

            allCategories = budget.GetCategories();

            Assert.AreEqual(categoryCount, allCategories.Count(), "GetCategories() returned wrong number of BudgetCategories");
        }

        [DataTestMethod]
        [DataRow(1), DataRow(2), DataRow(3), DataRow(4)]
        public void GetAllBudgetCategoriesReturnsCorrectCategories(int id) {
            IBudget budget = new Budget(repo);
            BudgetCategory category;
            IQueryable<BudgetCategory> allCategories;
            string expectedName = categoryRef[id];

            allCategories = budget.GetCategories();
            category = allCategories.Where(c => c.ID == id).First();

            Assert.AreEqual(expectedName, category.Name, $"BudgetCategory with ID = {id} should have Name = {expectedName}");
        }

        [TestMethod]
        public void AddANewBudgetCategory() {
            IBudget budget = new Budget(repo);
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
            IBudget budget = new Budget(repo);
            int testID = repo.BudgetCategories().Select(c => c.ID).First();
            BudgetCategory remove = budget.GetCategories().Where(c => c.ID == testID).First();
            int newCount;

            budget.RemoveBudgetCategory(remove);
            newCount = budget.GetCategories().Count();

            Assert.IsTrue(newCount == categoryCount - 1, "No category was removed");

            BudgetCategory removedCategory;
            try {
                removedCategory = budget.GetCategories().Where(c => c.ID == testID).First();
                Assert.Fail($"'{removedCategory.Name}' should have been removed");
            } catch {
                Assert.IsTrue(true); // if an error occurs, that means the testID no longer exists
            }
        }
    }
}
