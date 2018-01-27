// using ExpenseTracker.Controllers;
using ExpenseTracker.Exceptions;
// using ExpenseTracker.Repository;
using ExpenseTracker.Repository.Extensions;
using ExpenseTracker.Services;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Controllers.Tests
{
    [TestClass]
    public class BudgetCategoryController_Tests
    {
//         private IBudgetService budget;
//         private Dictionary<int, string> categoryReference;
         private BudgetCategoryController controller;
         private Mock<ICategoryManagerService> mockService;
//         private Mock<IBudgetService> mockBudget;

//         [TestInitialize]
//         public void InitializeTestData() {
//             // Create in-memory BudgetCategories
//             List<BudgetCategory> categories = TestInitializer.CreateTestCategories();
//             mockBudget = new Mock<IBudgetService>();
//             mockBudget.Setup(m => m.GetCategories()).Returns(new TestAsyncEnumerable<BudgetCategory>(categories));
//             mockBudget.Setup(m => m.GetCategoryAsync(It.IsAny<int?>())).ReturnsAsync((int? x) => categories.AsQueryable().Where(c => c.ID == x).FirstOrDefault());

//             budget = mockBudget.Object;

//             categoryReference = new Dictionary<int, string>();
//             foreach (var category in budget.GetCategories()) {
//                 categoryReference.Add(category.ID, category.Name);
//             }

//             controller = new BudgetCategoryController(budget);
//         }
        [TestInitialize]
        public void Initialize_test_objects() {
            mockService = new Mock<ICategoryManagerService>();
            controller = new BudgetCategoryController(mockService.Object);
        }

        #region "Index Method Tests"
            [TestMethod]
            public async Task Index_GET_returns_index_view() {
                // Act
                var actionResult = await controller.Index();
                var result = actionResult as ViewResult;

                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual("Index", result.ViewName);
            }

            [TestMethod]
            public async Task Index_GET_passes_list_of_categories_to_viewmodel() {
                // Arrange
                var categories = new List<BudgetCategory>();
                var mockCategoryExt = new Mock<ICategoryExtMask>();
                mockCategoryExt.Setup(m => m.ToListAsync()).ReturnsAsync(categories);
                ExtensionFactory.CategoryExtFactory = ext => mockCategoryExt.Object;

                // Act
                var result = (ViewResult)(await controller.Index());
                var model = result.Model;

                // Assert
                mockService.Verify(m => m.GetOrderedCategories(nameof(BudgetCategory.Name), It.IsAny<bool>()), Times.Once());
                Assert.AreSame(categories, model);
            }
        #endregion

        #region "Details Method Tests"
            [TestMethod]
            public async Task Details_GET_returns_details_view() {
                // Arrange
                mockService.Setup(m => m.GetSingleCategoryAsync(It.IsAny<int>())).ReturnsAsync(new BudgetCategory());

                // Act
                var actionResult = await controller.Details(1);
                var result = actionResult as ViewResult;

                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual("Details", result.ViewName);
            }

            [TestMethod]
            public async Task Details_GET_passes_category_to_view() {
                // Arrange
                var category = new BudgetCategory { ID = 1 };
                mockService.Setup(m => m.GetSingleCategoryAsync(It.IsAny<int>())).ReturnsAsync(category);

                // Act
                var result = (ViewResult)(await controller.Details(1));
                var model = (BudgetCategory)result.Model;

                // Assert
                mockService.Verify(m => m.GetSingleCategoryAsync(1), Times.Once());
                Assert.AreEqual(category.ID, model.ID);
            }

            [TestMethod]
            public async Task Details_GET_returns_NotFound_When_IdNotFoundException_thrown() {
                // Arrange
                mockService.Setup(m => m.GetSingleCategoryAsync(It.IsAny<int?>())).ThrowsAsync(new IdNotFoundException());

                // Act
                var result = await controller.Details(1);

                // Assert
                Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            }

            [TestMethod]
            public async Task Details_GET_returns_NotFound_when_NullIdException_thrown() {
                // Arrange
                mockService.Setup(m => m.GetSingleCategoryAsync(It.IsAny<int?>())).ThrowsAsync(new NullIdException());

                // Act
                var result = await controller.Details(null);

                // Assert
                Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            }

            [TestMethod]
            public async Task Details_GET_throws_exceptions_not_of_type_NullId_or_IdNotFound() {
                // Arrange
                mockService.Setup(m => m.GetSingleCategoryAsync(It.IsAny<int?>())).ThrowsAsync(new Exception());

                // Act & Assert
                await Assert.ThrowsExceptionAsync<Exception>(() => controller.Details(1));
            }
        #endregion
        
        #region "Create Method Tests"
            [TestMethod]
            public void Create_GET_returns_create_view() {
                // Act
                var actionResult = controller.Create();
                var result = actionResult as ViewResult;

                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual("Create", result.ViewName);
            }

            [TestMethod]
            public async Task Create_POST_calls_AddCategoryAsync_and_redirects_to_index() {
                // Arrange
                var category = new BudgetCategory();

                // Act
                var result = await controller.Create(category);
                var redirectResult = result as RedirectToActionResult;

                // Assert
                mockService.Verify(m => m.AddCategoryAsync(category), Times.Once());
                Assert.IsNotNull(redirectResult);
                Assert.AreEqual("Index", redirectResult.ActionName);
            }

            [TestMethod]
            public async Task Create_POST_with_Invalid_model_state_returns_category_to_create_view() {
                // Arrange
                var category = new BudgetCategory();
                controller.ModelState.AddModelError("test", "test");

                // Act
                var result = await controller.Create(category);
                var viewResult = result as ViewResult;
                var model = viewResult.Model as BudgetCategory;

                // Assert
                Assert.IsNotNull(viewResult);
                Assert.AreEqual("Create", viewResult.ViewName);
                Assert.AreSame(category, model);
            }
        #endregion    

        #region "Delete Method Tests"
//             [TestMethod]
//             public async Task DeleteGETReturnsView() {
//                 int id = budget.GetCategories().Select(c => c.ID).First();

//                 IActionResult actionResult = await controller.Delete(id);
//                 var result = actionResult as ViewResult;
                
//                 Assert.IsNotNull(result);
//                 Assert.AreEqual("Delete", result.ViewName, $"Delete method returned '{result.ViewName}' instead of 'Delete'");
//             }

//             [DataTestMethod]
//             [DataRow(1), DataRow(2), DataRow(3), DataRow(-1), DataRow(300)]
//             public async Task DeleteGETReturnsCorrectBudgetCategory(int id) {
//                 IActionResult actionResult;

//                 if (!categoryReference.ContainsKey(id)) {
//                     GetCategoryAsync_ShouldThrow(new IdNotFoundException());

//                     actionResult = await controller.Delete(id);
//                     Assert.IsInstanceOfType(actionResult, typeof(NotFoundResult), $"The id ({id}) doesn't exist. 404 Not Found should have been called");
//                 } else {
//                     actionResult = await controller.Delete(id);
//                     string categoryName = categoryReference[id];

//                     var result = actionResult as ViewResult;
//                     BudgetCategory model = (BudgetCategory)result.ViewData.Model;

//                     Assert.AreEqual(categoryName, model.Name, $"The wrong BudgetCategory was returned by for ID = {id}");
//                 }
//             }

//             [TestMethod]
//             public async Task DeleteGETReturnsNotFoundForNullIndex() {
//                 GetCategoryAsync_ShouldThrow(new NullIdException());
//                 IActionResult actionResult = await controller.Delete(null);

//                 Assert.IsInstanceOfType(actionResult, typeof(NotFoundResult), "A NULL id should result in 404 Not Found");
//             }

//             [TestMethod]
//             public async Task DeletePOSTRemoveExistingCategory() {
//                 int testID = budget.GetCategories().Select(c => c.ID).First();

//                 IActionResult actionResult = await controller.DeleteConfirmed(testID);
//                 var result = actionResult as RedirectToActionResult;

//                 Assert.AreEqual("Index", result.ActionName, "DeletePOST should redirect to Index");

//                 BudgetCategory categoryShouldntBeThere = budget.GetCategories().Where(c => c.ID == testID).SingleOrDefault();

//                 mockBudget.Verify(m => m.RemoveBudgetCategoryAsync(It.IsAny<int>()), Times.Once());
//             }

//             [TestMethod]
//             public async Task DeletePOSTRemoveNonExistantCategory() {
//                 int testID = budget.GetCategories().OrderByDescending(c => c.ID).Select(c => c.ID).First() + 10;
//                 int preCount = budget.GetCategories().Count();

//                 IActionResult actionResult = await controller.DeleteConfirmed(testID);
//                 var result = actionResult as RedirectToActionResult;

//                 Assert.AreEqual("Index", result.ActionName, "DeletePOST should redirect to Index");

//                 Assert.AreEqual(preCount, budget.GetCategories().Count(), "No category should have been removed");
//             }
        #endregion







//         #region "Edit Method Tests"
//             [TestMethod]
//             public async Task EditGETReturnsView() {
//                 int testID = budget.GetCategories().First().ID;
//                 IActionResult actionResult = await controller.Edit(testID);

//                 var result = actionResult as ViewResult;

//                 Assert.IsNotNull(result, "A ViewResult was not returned");
//                 Assert.AreEqual("Edit", result.ViewName, $"Edit returned {result.ViewName} instead of 'Edit'");
//             }
            
//             [DataTestMethod]
//             [DataRow(1), DataRow(4), DataRow(-9), DataRow(5000000)]
//             public async Task EditGETUsesCorrectModel(int id) {
//                 IActionResult actionResult;
                
//                 if (!categoryReference.ContainsKey(id)) {
//                     GetCategoryAsync_ShouldThrow(new IdNotFoundException());

//                     actionResult = await controller.Edit(id);
//                     Assert.IsInstanceOfType(actionResult, typeof(NotFoundResult), $"ID = {id} should raise 404 Not Found");
//                 } else {
//                     actionResult = await controller.Edit(id);
//                     var result = (ViewResult)actionResult;
//                     BudgetCategory model = (BudgetCategory)result.Model;

//                     Assert.AreEqual(categoryReference[id], model.Name, $"The wrong Budget Category was returned for ID = {id}");
//                 }

//             }

//             [TestMethod]
//             public async Task EditGETReturnsNotFoundForNULLId() {
//                 GetCategoryAsync_ShouldThrow(new NullIdException());
//                 IActionResult actionResult = await controller.Edit(null);

//                 Assert.IsInstanceOfType(actionResult, typeof(NotFoundResult), "A NULL Id should raise 404 Not Found");
//             }
//         #endregion


//         private void GetCategoryAsync_ShouldThrow(Exception exToThrow) {
//             mockBudget.Setup(m => m.GetCategoryAsync(It.IsAny<int?>())).ThrowsAsync(exToThrow);
//         }
    }
}