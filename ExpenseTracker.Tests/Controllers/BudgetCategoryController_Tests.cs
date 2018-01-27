using ExpenseTracker.Exceptions;
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
        private BudgetCategoryController controller;
        private Mock<ICategoryManagerService> mockService;

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
            [TestMethod]
            public async Task Delete_GET_returns_delete_view_with_model() {
                // Arrange
                var category = new BudgetCategory();
                mockService.Setup(m => m.GetSingleCategoryAsync(It.IsAny<int?>())).ReturnsAsync(category);

                // Act
                var result = await controller.Delete(1);
                var viewResult = result as ViewResult;
                var model = viewResult.Model as BudgetCategory;

                // Assert
                mockService.Verify(m => m.GetSingleCategoryAsync(1), Times.Once());
                Assert.IsNotNull(viewResult);
                Assert.AreSame(category, model);
            }

            [TestMethod]
            public async Task Delete_GET_returns_NotFound_when_IdNotFoundException_is_thrown() {
                // Arrange
                mockService.Setup(m => m.GetSingleCategoryAsync(It.IsAny<int?>())).ThrowsAsync(new IdNotFoundException());

                // Act
                var result = await controller.Delete(1);

                // Assert
                Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            }

            [TestMethod]
            public async Task Delete_GET_returns_NotFound_when_NullIdException_thrown() {
                // Arrange
                mockService.Setup(m => m.GetSingleCategoryAsync(It.IsAny<int?>())).ThrowsAsync(new NullIdException());

                // Act
                var result = await controller.Delete(null);

                // Assert
                Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            }

            [TestMethod]
            public async Task Delete_GET_throws_exceptions_not_of_type_NullId_or_IdNotFound() {
                // Arrange
                mockService.Setup(m => m.GetSingleCategoryAsync(It.IsAny<int?>())).ThrowsAsync(new Exception());

                // Act & Assert
                await Assert.ThrowsExceptionAsync<Exception>(() => controller.Delete(1));
            }

            [TestMethod]
            public async Task Delete_POST_calls_RemoveCategoryAsync_and_redirects_to_Index() {
                // Act
                var result = await controller.DeleteConfirmed(1);
                var redirectResult = result as RedirectToActionResult;

                // Assert
                mockService.Verify(m => m.RemoveCategoryAsync(1), Times.Once());
                Assert.IsNotNull(redirectResult);
                Assert.AreEqual("Index", redirectResult.ActionName);
            }
        #endregion

        #region "Edit Method Tests"
            [TestMethod]
            public async Task Edit_GET_returns_edit_view_with_correct_model() {
                // Arrange
                var category = new BudgetCategory();
                mockService.Setup(m => m.GetSingleCategoryAsync(It.IsAny<int?>())).ReturnsAsync(category);

                // Act
                var result = await controller.Edit(1);
                var viewResult = result as ViewResult;
                var model = viewResult.Model as BudgetCategory;

                // Assert
                mockService.Verify(m => m.GetSingleCategoryAsync(1), Times.Once());
                Assert.IsNotNull(viewResult);
                Assert.AreEqual("Edit", viewResult.ViewName);
                Assert.AreSame(category, model);
            }

            [TestMethod]
            public async Task Edit_GET_returns_NotFound_when_IdNotFoundException_thrown() {
                // Arrange
                mockService.Setup(m => m.GetSingleCategoryAsync(It.IsAny<int?>())).ThrowsAsync(new IdNotFoundException());

                // Act
                var result = await controller.Edit(1);

                // Assert
                Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            }

            [TestMethod]
            public async Task Edit_GET_returns_NotFound_when_NullIdException_thrown() {
                // Arrange
                mockService.Setup(m => m.GetSingleCategoryAsync(It.IsAny<int?>())).ThrowsAsync(new NullIdException());

                // Act
                var result = await controller.Edit(null);

                // Assert
                Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            }

            [TestMethod]
            public async Task Edit_GET_throws_exceptions_not_of_type_NullId_or_IdNotFound() {
                // Arrange
                mockService.Setup(m => m.GetSingleCategoryAsync(It.IsAny<int?>())).ThrowsAsync(new Exception());

                // Act & Assert
                await Assert.ThrowsExceptionAsync<Exception>(() => controller.Edit(1));
            }
        #endregion
    }
}