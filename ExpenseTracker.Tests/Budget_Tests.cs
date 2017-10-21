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
            List<BudgetCategory> categories = new List<BudgetCategory> {
                new BudgetCategory {
                    ID = 1,
                    Name = "Rent",
                    Amount = 575,
                    BeginEffectiveDate = new DateTime(2015, 02, 01),
                    EndEffectiveDate = null,
                    Type = BudgetType.Expense
                },
                new BudgetCategory {
                    ID = 2,
                    Name = "Groceries",
                    Amount = 750,
                    BeginEffectiveDate = new DateTime(2017, 10, 01),
                    EndEffectiveDate = null,
                    Type = BudgetType.Expense
                },
                new BudgetCategory {
                    ID = 3,
                    Name = "Old Groceries",
                    Amount = 700,
                    BeginEffectiveDate = new DateTime(2016, 10, 01),
                    EndEffectiveDate = new DateTime(2017, 09, 30),
                    Type = BudgetType.Expense
                },
                new BudgetCategory {
                    ID = 4,
                    Name = "ARC Income",
                    Amount = 0,
                    BeginEffectiveDate = new DateTime(2016, 06, 27),
                    EndEffectiveDate = null,
                    Type = BudgetType.Income
                }
            };
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

            Assert.AreEqual(categoryCount, allCategories.Count(), $"GetCategories() should return {categoryCount} BudgetCategories");
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
            BudgetCategory newCategory = new BudgetCategory {
                ID = 5,
                Name = "Insurance",
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
                retrievedCategory = budget.GetCategories().Where(c => c.Name == "Insurance").First();
                Assert.AreEqual(5, retrievedCategory.ID, "Category ID does not match expected ID of category added");
            } catch {
                Assert.Fail("No \"Insurance\" category was found");
            }
        }

        [TestMethod]
        public void RemoveABudgetCategory() {
            IBudget budget = new Budget(repo);
            BudgetCategory remove = budget.GetCategories().Where(c => c.ID == 1).First();
            int newCount;

            budget.RemoveBudgetCategory(remove);
            newCount = budget.GetCategories().Count();

            Assert.IsTrue(newCount == categoryCount - 1, "No category was removed");
        }
    }
}
