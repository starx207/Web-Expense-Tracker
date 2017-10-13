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

        [TestInitialize]
        public void InitializeTestData() {
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
            categoryRef = new Dictionary<int, string>();
            categoryRef.Add(1, "Rent");
            categoryRef.Add(2, "Groceries");
            categoryRef.Add(3, "Old Groceries");
            categoryRef.Add(4, "ARC Income");

            List<Transaction> transactions = new List<Transaction>();
            List<Payee> payees = new List<Payee>();
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
            // if no error, test passes
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void GetAllBudgetCategoriesReturnsCorrectCount() {
            IBudget budget = new Budget(repo);
            IQueryable<BudgetCategory> allCategories;

            allCategories = budget.GetCategories();

            Assert.AreEqual(4, allCategories.Count(), "GetCategories() should return 4 BudgetCategories");
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
    }
}
