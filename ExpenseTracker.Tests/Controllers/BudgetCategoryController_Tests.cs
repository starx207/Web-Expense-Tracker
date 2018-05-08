using ExpenseTracker.Exceptions;
using ExpenseTracker.Models;
using ExpenseTracker.Repository.Extensions;
using ExpenseTracker.Services;
using ExpenseTracker.TestResources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Controllers.Tests
{
    [TestClass]
    public class BudgetCategoryController_Tests : TestCommon
    {
        #region Private Members

        private BudgetCategoryController _controller;
        private Mock<ICategoryManagerService> _mockService;

        #endregion // Private Members

        #region Test Initialization

        [TestInitialize]
        public void Initialize_test_objects() {
            _mockService = new Mock<ICategoryManagerService>();
            _controller = new BudgetCategoryController(_mockService.Object);
        }

        #endregion // Test Initialization

        #region Tests

        #region Index Tests

        [TestMethod]
        public async Task Index_GET_returns_index_view() {
            // Arrange
            var mockExtension = new Mock<IExtensionMask<CategoryCrudVm>>();
            ExtensionFactoryHelpers<CategoryCrudVm>.ExtFactoryOverride = ext => mockExtension.Object;

            // Act
            var actionResult = await _controller.Index();
            var result = actionResult as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ViewName);
        }

        [TestMethod]
        public async Task Index_GET_passes_list_of_categories_to_viewmodel() {
            // Arrange
            var categories = new List<CategoryCrudVm>();
            var mockCategoryExt = new Mock<IExtensionMask<CategoryCrudVm>>();
            mockCategoryExt.Setup(m => m.ToListAsync()).ReturnsAsync(categories);
            ExtensionFactoryHelpers<CategoryCrudVm>.ExtFactoryOverride = ext => mockCategoryExt.Object;

            // Act
            var result = (ViewResult)(await _controller.Index());
            var model = result.Model;

            // Assert
            _mockService.Verify(m => m.GetCategories(true), Times.Once());
            Assert.AreSame(categories, model);
        }

        #endregion // Index Tests

        #region Details Tests

        [TestMethod]
        public async Task Details_GET_returns_details_view() {
            // Arrange
            _mockService.Setup(m => m.GetSingleCategoryAsync(It.IsAny<int>())).ReturnsAsync(new BudgetCategory());

            // Act
            var actionResult = await _controller.Details(1);
            var result = actionResult as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Details", result.ViewName);
        }

        [TestMethod]
        public async Task Details_GET_passes_category_to_view() {
            // Arrange
            var category = new BudgetCategory { ID = 1 };
            _mockService.Setup(m => m.GetSingleCategoryAsync(It.IsAny<int>())).ReturnsAsync(category);

            // Act
            var result = (ViewResult)(await _controller.Details(1));
            var model = (CategoryCrudVm)result.Model;

            // Assert
            _mockService.Verify(m => m.GetSingleCategoryAsync(1), Times.Once());
            Assert.AreEqual(category.ID, model.NavId);
        }

        [TestMethod]
        public async Task Details_GET_returns_NotFound_When_IdNotFoundException_thrown() {
            // Arrange
            _mockService.Setup(m => m.GetSingleCategoryAsync(It.IsAny<int?>())).ThrowsAsync(new IdNotFoundException());

            // Act
            var result = await _controller.Details(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Details_GET_returns_NotFound_when_id_is_null() {
            // Act
            var result = await _controller.Details(null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Details_GET_throws_exceptions_not_of_type_NullId_or_IdNotFound() {
            // Arrange
            _mockService.Setup(m => m.GetSingleCategoryAsync(It.IsAny<int?>())).ThrowsAsync(new Exception());

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(() => _controller.Details(1));
        }

        #endregion // Details Tests
        
        #region Create Tests

        #region Create GET

        [TestMethod]
        public async Task Create_GET_returns_create_view() {
            // Act
            var actionResult = await _controller.Create();
            var result = actionResult as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Create", result.ViewName);
        }

        #endregion // Create GET

        #region Create POST

        [TestMethod]
        public async Task Create_POST_calls_AddCategoryAsync_and_redirects_to_index() {
            // Arrange
            var category = new CategoryCrudVm();

            // Act
            var result = await _controller.Create(category);
            var redirectResult = result as RedirectToActionResult;

            // Assert
            _mockService.Verify(m => m.AddCategoryAsync(category.Name, category.Amount, category.Type), Times.Once());
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.ActionName);
        }

        [TestMethod]
        public async Task Create_POST_returns_category_to_create_view_when_ModelValidationException_thrown() {
            // Arrange
            var category = new CategoryCrudVm { NavId = 1 };
            SetupControllerRouteData(_controller, "id", category.NavId);
            _mockService.Setup(m => m.AddCategoryAsync(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<BudgetType>()))
                .ThrowsAsync(new ModelValidationException());

            // Act
            var result = await _controller.Create(category);
            var viewResult = result as ViewResult;
            var model = viewResult.Model as CategoryCrudVm;

            // Assert
            Assert.IsNotNull(viewResult);
            Assert.AreEqual("Create", viewResult.ViewName);
            Assert.AreSame(category, model);
        }

        [TestMethod]
        public async Task Create_POST_adds_modelstate_error_when_UniqueConstraintViolationException_thrown() {
            // Arrange
            var category = new CategoryCrudVm { Name = "test" };
            _mockService.Setup(m => m.AddCategoryAsync(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<BudgetType>()))
                .ThrowsAsync(new ModelValidationException());

            // Act
            var result = await _controller.Create(category);
            var viewResult = result as ViewResult;
            var model = viewResult.Model as CategoryCrudVm;

            // Assert
            Assert.AreEqual(1, _controller.ModelState.ErrorCount);
            Assert.IsNotNull(viewResult);
            Assert.AreEqual("Create", viewResult.ViewName);
            Assert.AreSame(category, model);
        }

        #endregion // Create POST

        #endregion // Create Tests

        #region Delete Tests

        #region Delete GET

        [TestMethod]
        public async Task Delete_GET_returns_delete_view_with_model() {
            // Arrange
            var category = new BudgetCategory { ID = 2 };
            _mockService.Setup(m => m.GetSingleCategoryAsync(It.IsAny<int?>())).ReturnsAsync(category);

            // Act
            var result = await _controller.Delete(2);
            var viewResult = result as ViewResult;
            var model = viewResult.Model as CategoryCrudVm;

            // Assert
            _mockService.Verify(m => m.GetSingleCategoryAsync(2), Times.Once());
            Assert.IsNotNull(viewResult);
            Assert.AreEqual(category.ID, model.NavId);
        }

        [TestMethod]
        public async Task Delete_GET_returns_NotFound_when_IdNotFoundException_is_thrown() {
            // Arrange
            _mockService.Setup(m => m.GetSingleCategoryAsync(It.IsAny<int?>())).ThrowsAsync(new IdNotFoundException());

            // Act
            var result = await _controller.Delete(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Delete_GET_returns_NotFound_when_null_id_passed() {
            // Act
            var result = await _controller.Delete(null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Delete_GET_throws_exceptions_not_of_type_NullId_or_IdNotFound() {
            // Arrange
            _mockService.Setup(m => m.GetSingleCategoryAsync(It.IsAny<int?>())).ThrowsAsync(new Exception());

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(() => _controller.Delete(1));
        }

        #endregion // Delete GET

        #region Delete POST

        [TestMethod]
        public async Task Delete_POST_calls_RemoveCategoryAsync_and_redirects_to_Index() {
            // Act
            var result = await _controller.DeleteConfirmed(1);
            var redirectResult = result as RedirectToActionResult;

            // Assert
            _mockService.Verify(m => m.RemoveCategoryAsync(1), Times.Once());
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.ActionName);
        }

        #endregion // Delete POST

        #endregion // Delete Tests

        #region Edit Tests

        #region Edit GET

        [TestMethod]
        public async Task Edit_GET_returns_edit_view_with_correct_model() {
            // Arrange
            var category = new BudgetCategory { ID = 8 };
            _mockService.Setup(m => m.GetSingleCategoryAsync(It.IsAny<int?>())).ReturnsAsync(category);

            // Act
            var result = await _controller.Edit(8);
            var viewResult = result as ViewResult;
            var model = viewResult.Model as CategoryCrudVm;

            // Assert
            _mockService.Verify(m => m.GetSingleCategoryAsync(8), Times.Once());
            Assert.IsNotNull(viewResult);
            Assert.AreEqual("Edit", viewResult.ViewName);
            Assert.AreEqual(category.ID, model.NavId);
        }

        [TestMethod]
        public async Task Edit_GET_returns_NotFound_when_ExpenseTrackerException_thrown() {
            // Arrange
            _mockService.Setup(m => m.GetSingleCategoryAsync(It.IsAny<int?>())).ThrowsAsync(new ExpenseTrackerException());

            // Act
            var result = await _controller.Edit(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Edit_GET_throws_exceptions_not_of_type_ExpenseTrackerException() {
            // Arrange
            _mockService.Setup(m => m.GetSingleCategoryAsync(It.IsAny<int?>())).ThrowsAsync(new Exception());

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(() => _controller.Edit(1));
        }

        #endregion // Edit GET

        #region Edit POST

        [TestMethod]
        public async Task Edit_POST_calls_UpdateCategoryAsync_and_redirects_to_Index() {
            // Arrange
            var category = new CategoryCrudVm {
                NavId = 1,
                Amount = 20.12,
                EffectiveFrom = DateTime.Parse("1/1/2018"),
                Type = BudgetType.Expense
            };
            SetupControllerRouteData(_controller, "id", 1);
            
            // Act
            var result = await _controller.Edit(category);
            var redirectResult = result as RedirectToActionResult;

            // Assert
            _mockService.Verify(m => m.UpdateCategoryAsync(category.NavId, category.Amount, category.EffectiveFrom, category.Type), Times.Once());
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.ActionName);
        }

        [TestMethod]
        public async Task Edit_POST_returns_to_view_when_ModelValidationException_thrown() {
            // Arrange
            var category = new CategoryCrudVm { NavId = 1 };
            SetupControllerRouteData(_controller, "id", category.NavId);
            _mockService.Setup(m => m.UpdateCategoryAsync(It.IsAny<int>(), It.IsAny<double>(), It.IsAny<DateTime>(), It.IsAny<BudgetType>()))
                .ThrowsAsync(new ModelValidationException());

            // Act
            var result = await _controller.Edit(category);
            var viewResult = result as ViewResult;
            var model = viewResult.Model as CategoryCrudVm;

            // Assert
            Assert.IsNotNull(viewResult);
            Assert.AreEqual("Edit", viewResult.ViewName);
            Assert.AreSame(category, model);
        }

        [TestMethod]
        public async Task Edit_POST_returns_NotFound_when_ExpenseTrackerException_thrown() {
            // Arrange
            var category = new CategoryCrudVm { NavId = 1 };
            _mockService.Setup(m => m.UpdateCategoryAsync(It.IsAny<int>(),
                It.IsAny<double>(),
                It.IsAny<DateTime>(),
                It.IsAny<BudgetType>())).ThrowsAsync(new ExpenseTrackerException());
            SetupControllerRouteData(_controller, "id", 1);

            // Act
            var result = await _controller.Edit(category);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Edit_POST_throws_ConcurrencyExceptions_if_the_category_exists() {
            // Arrange
            var category = new CategoryCrudVm { NavId = 1 };
            _mockService.Setup(m => m.UpdateCategoryAsync(It.IsAny<int>(),
                It.IsAny<double>(),
                It.IsAny<DateTime>(),
                It.IsAny<BudgetType>())).ThrowsAsync(new ConcurrencyException());
            _mockService.Setup(m => m.CategoryExists(It.IsAny<int>())).Returns(true);
            SetupControllerRouteData(_controller, "id", 1);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ConcurrencyException>(() => _controller.Edit(category));
        }

        [TestMethod]
        public async Task Edit_POST_throws_Exceptions_not_of_type_ExpesneTrackerException() {
            // Arrange
            var category = new CategoryCrudVm { NavId = 1 };
            _mockService.Setup(m => m.UpdateCategoryAsync(It.IsAny<int>(),
                It.IsAny<double>(),
                It.IsAny<DateTime>(),
                It.IsAny<BudgetType>())).ThrowsAsync(new Exception());
            SetupControllerRouteData(_controller, "id", 1);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(() => _controller.Edit(category));
        }

        [TestMethod]
        public async Task Edit_POST_returns_to_Edit_and_sets_ModelState_Error_when_InvalidDateException_thrown() {
            // Arrange
            var category = new CategoryCrudVm { NavId = 1 };
            _mockService.Setup(m => m.UpdateCategoryAsync(It.IsAny<int>(),
                It.IsAny<double>(),
                It.IsAny<DateTime>(),
                It.IsAny<BudgetType>())).ThrowsAsync(new InvalidDateExpection("Test Message"));
            SetupControllerRouteData(_controller, "id", 1);

            // Act
            var result = await _controller.Edit(category);
            var viewResult = result as ViewResult;

            // Assert
            Assert.IsNotNull(viewResult);
            Assert.AreEqual("Edit", viewResult.ViewName);
            Assert.AreEqual(1, _controller.ViewData.ModelState.ErrorCount);
        }

        #endregion // Edit POST

        #endregion // Edit Tests
    
        #endregion // Tests
    }
}