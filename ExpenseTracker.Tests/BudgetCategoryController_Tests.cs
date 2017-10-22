using ExpenseTracker.Controllers;
using ExpenseTracker.Data.Repository;
using ExpenseTracker.Models;
using ExpenseTracker.Tests.Mock;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Tests
{
    [TestClass]
    public class BudgetCategoryController_Tests
    {
        private IBudget budget;
        [TestInitialize]
        public void InitializeTestData() {
            // Create in-memory BudgetCategories
            List<BudgetCategory> categories = TestInitializer.CreateTestCategories();
            budget = new MockBudget(categories);
        }

        [TestMethod]
        public async Task ControllerIndexMethodReturnsView() {
            BudgetCategoryController controller = new BudgetCategoryController(budget);
            var actionResult = await controller.Index();
            //actionResult.Wait();
            var result = actionResult as ViewResult;
            Assert.AreEqual("Index", result.ViewName, $"Index method returned '{result.ViewName}' instead of 'Index'");
        }
    }
}