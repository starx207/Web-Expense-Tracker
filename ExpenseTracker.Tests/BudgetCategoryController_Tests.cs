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
        private Dictionary<int, string> categoryReference;
        private BudgetCategoryController controller;

        [TestInitialize]
        public void InitializeTestData() {
            // Create in-memory BudgetCategories
            List<BudgetCategory> categories = TestInitializer.CreateTestCategories();
            budget = new MockBudget(new TestAsyncEnumerable<BudgetCategory>(categories));

            categoryReference = new Dictionary<int, string>();
            foreach (var category in budget.GetCategories()) {
                categoryReference.Add(category.ID, category.Name);
            }

            controller = new BudgetCategoryController(budget);
        }

        [TestMethod]
        public async Task IndexMethodReturnsView() {
            IActionResult actionResult = await controller.Index();
            var result = actionResult as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ViewName, $"Index method returned '{result.ViewName}' instead of 'Index'");
        }

        [TestMethod]
        public async Task DetailsMethodReturnsView() {
            int id = budget.GetCategories().First().ID;

            IActionResult actionResult = await controller.Details(id);
            var result = actionResult as ViewResult;
            
            Assert.IsNotNull(result);
            Assert.AreEqual("Details", result.ViewName, $"Details method returned '{result.ViewName}' instead of 'Details'");
        }

        [DataTestMethod]
        [DataRow(1), DataRow(2), DataRow(3), DataRow(-1), DataRow(300)]
        public async Task DetailsMethodReturnsCorrectBudgetCategory(int id) {
            IActionResult actionResult = await controller.Details(id);

            if (!categoryReference.ContainsKey(id)) {
                Assert.IsInstanceOfType(actionResult, typeof(NotFoundResult), $"The id ({id}) doesn't exist. NotFound404 should have been called");
            } else {
                string categoryName = categoryReference[id];

                var result = actionResult as ViewResult;
                BudgetCategory model = (BudgetCategory)result.ViewData.Model;

                Assert.AreEqual(categoryName, model.Name, $"The wrong BudgetCategory was returned by for ID = {id}");
            }
        }

        [TestMethod]
        public async Task DetailsMethodReturnsNotFoundForNullIndex() {
            IActionResult actionResult = await controller.Details(null);

            Assert.IsInstanceOfType(actionResult, typeof(NotFoundResult));
        }
    }
}