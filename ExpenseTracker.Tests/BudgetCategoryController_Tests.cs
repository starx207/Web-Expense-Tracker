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

        #region "Index Method Tests"
        [TestMethod]
        public async Task IndexMethodReturnsView() {
            IActionResult actionResult = await controller.Index();
            var result = actionResult as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ViewName, $"Index method returned '{result.ViewName}' instead of 'Index'");
        }
        #endregion

        #region "Details Method Tests"
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
        #endregion
        
        #region "Create Method Tests"
        [TestMethod]
        public void CreateMethodGetReturnsView() {
            IActionResult actionResult = controller.Create();
            var result = actionResult as ViewResult;

            Assert.AreEqual("Create", result.ViewName, $"Create method returned '{result.ViewName}' instead of 'Create'");
        }

        [TestMethod]
        public async Task CreatePostAddsCategoryAndRedirectsToIndex() {
            int testID = budget.GetCategories().OrderByDescending(c => c.ID).Select(c => c.ID).First() + 1;
            BudgetCategory newCategory = new BudgetCategory {
                ID = testID,
                Name = "New Test Category",
                Amount = 700,
                Type = BudgetType.Expense,
                BeginEffectiveDate = new DateTime(2016, 09, 01),
                EndEffectiveDate = null
            };

            IActionResult actionResult = await controller.Create(newCategory);
            var result = actionResult as RedirectToActionResult;
            
            Assert.AreEqual("Index", result.ActionName, "Create should redirect to Index after successful create");

            BudgetCategory category = budget.GetCategories().Where(c => c.ID == testID).First();
            Assert.AreEqual(newCategory.Name, category.Name, "New category was not properly added");
        }

        [TestMethod]
        public async Task CreatePostWithInvalidModelStateReturnsToView() {
            int testID = budget.GetCategories().OrderByDescending(c => c.ID).Select(c => c.ID).First() + 1;
            BudgetCategory newCategory = new BudgetCategory {
                ID = testID,
                Name = "New Test Category",
                Type = BudgetType.Expense,
                Amount = 389,
                BeginEffectiveDate = new DateTime(2016, 12, 31),
                EndEffectiveDate = null
            };

            controller.ModelState.AddModelError("test", "test");

            IActionResult actionResult = await controller.Create(newCategory);
            var viewResult = actionResult as ViewResult;

            Assert.AreEqual("Create", viewResult.ViewName, "Create should return to itself if ModelState is invalid");

            BudgetCategory model = (BudgetCategory)viewResult.Model;

            Assert.AreEqual(testID, model.ID, "The BudgetCategory was not sent back to the view");
        }
        #endregion
        
        #region "Delete Method Tests"
        #endregion

        #region "Edit Method Tests"
        #endregion
    }
}