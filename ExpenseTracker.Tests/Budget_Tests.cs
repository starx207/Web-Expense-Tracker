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
        private IBudgetAccess _repo;

        [TestInitialize]
        public void InitializeBudgetAccessObject() {
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
        public void GetAllBudgetCategories() {
            IBudget budget = new Budget(repo);

            IQueryable<BudgetCategory> allCategories;

            allCategories = budget.GetCategories();

            Assert.AreEqual(4, allCategories.Count(), "GetCategories() should return 4 BudgetCategories");
        }
        
        [TestMethod]
        public void GetBudgetCategoryById() {
            throw new NotImplementedException();   
        }
    }
}
