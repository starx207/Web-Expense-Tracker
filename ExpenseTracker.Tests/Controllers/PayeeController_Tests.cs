using ExpenseTracker.Exceptions;
using ExpenseTracker.Repository.Extensions;
using ExpenseTracker.Services;
using ExpenseTracker.TestResources;
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
    public class PayeeController_Tests : TestCommon
    {
        #region Private Members

        private Mock<IPayeeManagerService> _mockService;
        private PayeeController _controller;
        private readonly string _categorySelectListKeyRO = "CategoryList";

        #endregion // Private Members

        #region Test Initialization

        [TestInitialize]
        public void InitializeTestObjects() {
            _mockService = new Mock<IPayeeManagerService>();
            _controller = new PayeeController(_mockService.Object);
        }

        #endregion // Test Initialization

        #region Tests

        #region Index Tests

        [TestMethod]
        public async Task Index_GET_returns_view() {
            // Act
            var result = await _controller.Index();
            var viewResult = result as ViewResult;

            // Assert
            Assert.IsNotNull(viewResult);
            Assert.AreEqual("Index", viewResult.ViewName);
        }

        [TestMethod]
        public async Task Index_GET_passes_list_of_payees_to_viewmodel() {
            // Arrange
            var payees = new List<Payee>();
            var mockPayeeExt = new Mock<IPayeeExtMask>();
            mockPayeeExt.Setup(m => m.ToListAsync()).ReturnsAsync(payees);
            ExtensionFactory.PayeeExtFactory = ext => mockPayeeExt.Object;

            // Act
            var result = (ViewResult)(await _controller.Index());
            var model = result.Model;

            // Assert
            _mockService.Verify(m => m.GetPayees(true, true, false, true), Times.Once());
            Assert.AreSame(payees, model);
        }  

        #endregion // Index Tests

        #region Details Tests

        [TestMethod]
        public async Task Details_GET_returns_view_with_correct_model() {
            // Arrange
            var payee = new Payee();
            _mockService.Setup(m => m.GetSinglePayeeAsync(It.IsAny<int?>(), It.IsAny<bool>())).ReturnsAsync(payee);

            // Act
            var result = await _controller.Details(1);
            var viewResult = result as ViewResult;
            var model = viewResult.Model as Payee;

            // Assert
            _mockService.Verify(m => m.GetSinglePayeeAsync(1, true), Times.Once());
            Assert.IsNotNull(viewResult);
            Assert.AreEqual("Details", viewResult.ViewName);
            Assert.AreSame(payee, model);
        }

        [TestMethod]
        public async Task Details_GET_returns_NotFound_when_ExpenseTrackerException_thrown() {
            // Arrange
            _mockService.Setup(m => m.GetSinglePayeeAsync(It.IsAny<int?>(), It.IsAny<bool>())).ThrowsAsync(new ExpenseTrackerException());

            // Act
            var result = await _controller.Details(null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Details_GET_throws_exceptions_not_of_type_ExpenseTrackerException() {
            // Arrange
            _mockService.Setup(m => m.GetSinglePayeeAsync(It.IsAny<int?>(), It.IsAny<bool>())).ThrowsAsync(new Exception());

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(() => _controller.Details(1));
        }

        #endregion // Details Tests

        #region Create Tests

        #region Create GET

        [TestMethod]
        public void Create_GET_returns_view() {
            // Act
            var result = _controller.Create();
            var viewResult = result as ViewResult;

            // Assert
            Assert.IsNotNull(viewResult);
            Assert.AreEqual("Create", viewResult.ViewName);
        }

        [TestMethod]
        public void Create_GET_populates_category_select_list() {
            // Arrange
            var categories = new List<BudgetCategory> {
                new BudgetCategory { ID = 1 },
                new BudgetCategory { ID = 2 },
                new BudgetCategory { ID = 3 }
            }.AsQueryable();
            _mockService.Setup(m => m.GetCategories(It.IsAny<bool>())).Returns(categories);

            // Act
            var result = (ViewResult)_controller.Create();

            // Assert
            AssertThatViewDataIsSelectList(result.ViewData, _categorySelectListKeyRO, categories.Select(c => c.ID.ToString()));
        }

        #endregion // Create GET

        #region Create POST

        [TestMethod]
        public async Task Create_POST_calls_AddPayeeAsync_and_redirects_to_index() {
            // Arrange
            var payee = new Payee();

            // Act
            var result = await _controller.Create(payee);
            var redirectResult = result as RedirectToActionResult;

            // Assert
            _mockService.Verify(m => m.AddPayeeAsync(payee), Times.Once());
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.ActionName);
        }

        [TestMethod]
        public async Task Create_POST_returns_view_with_payee_for_invalid_modelstate() {
            // Arrange
            var payee = new Payee();
            _controller.ModelState.AddModelError("test", "test");

            // Act
            var result = await _controller.Create(payee);
            var viewResult = result as ViewResult;
            var model = viewResult.Model as Payee;

            // Assert
            Assert.IsNotNull(viewResult);
            Assert.AreEqual("Create", viewResult.ViewName);
            Assert.AreSame(payee, model);
        }

        [TestMethod]
        public async Task Create_POST_correctly_populates_category_select_on_invalid_modelstate() {
            // Arrange
            var categories = new List<BudgetCategory> {
                new BudgetCategory { ID = 1 },
                new BudgetCategory { ID = 2 },
                new BudgetCategory { ID = 3 }
            }.AsQueryable();
            var payee = new Payee { BudgetCategoryID = 3 };
            _mockService.Setup(m => m.GetCategories(It.IsAny<bool>())).Returns(categories);
            _controller.ModelState.AddModelError("test", "test");
            
            // Act
            var result = (ViewResult)(await _controller.Create(payee));

            // Assert
            AssertThatViewDataIsSelectList(result.ViewData, _categorySelectListKeyRO, categories.Select(c => c.ID.ToString()), payee.BudgetCategoryID.ToString());
        }

        [TestMethod]
        public async Task Create_POST_adds_modelstate_error_when_UniqueConstraintViolationException_thrown() {
            // Arrange
            var payee = new Payee { Name = "test" };
            _mockService.Setup(m => m.AddPayeeAsync(It.IsAny<Payee>())).ThrowsAsync(new UniqueConstraintViolationException());

            // Act
            var result = await _controller.Create(payee);
            var viewResult = result as ViewResult;
            var model = viewResult.Model as Payee;

            // Assert
            Assert.AreEqual(1, _controller.ModelState.ErrorCount);
            Assert.IsNotNull(viewResult);
            Assert.AreEqual("Create", viewResult.ViewName);
            Assert.AreSame(payee, model);
        }

        #endregion // Create POST

        #endregion // Create Tests

        #region Delete Tests

        #region Delete GET

        [TestMethod]
        public async Task Delete_GET_calls_GetSinglePayeeAsync_and_returns_view() {
            // Arrange
            var payee = new Payee();
            _mockService.Setup(m => m.GetSinglePayeeAsync(It.IsAny<int?>(), It.IsAny<bool>())).ReturnsAsync(payee);

            // Act
            var result = await _controller.Delete(1);
            var viewResult = result as ViewResult;
            var model = viewResult.Model as Payee;

            // Assert
            _mockService.Verify(m => m.GetSinglePayeeAsync(1, true), Times.Once());
            Assert.IsNotNull(viewResult);
            Assert.AreEqual("Delete", viewResult.ViewName);
            Assert.AreSame(payee, model);
        }

        [TestMethod]
        public async Task Delete_GET_returns_NotFound_when_ExpenseTrackerException_thrown() {
            // Arrange
            _mockService.Setup(m => m.GetSinglePayeeAsync(It.IsAny<int?>(), It.IsAny<bool>())).ThrowsAsync(new ExpenseTrackerException());

            // Act
            var result = await _controller.Delete(null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Delete_GET_throws_Exceptions_not_of_type_ExpenseTrackerException() {
            // Arrange
            _mockService.Setup(m => m.GetSinglePayeeAsync(It.IsAny<int?>(), It.IsAny<bool>())).ThrowsAsync(new Exception());

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(() => _controller.Delete(1));
        }

        #endregion // Delete GET

        #region Delete POST

        [TestMethod]
        public async Task Delete_POST_calls_RemovePayeeAsync_and_redirects_to_index() {
            // Act
            var result = await _controller.DeleteConfirmed(1);
            var redirectResult = result as RedirectToActionResult;

            // Assert
            _mockService.Verify(m => m.RemovePayeeAsync(1), Times.Once());
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.ActionName);
        }

        #endregion // Delete POST

        #endregion // Delete Tests

        #region Edit Tests

        #region Edit GET

        [TestMethod]
        public async Task Edit_GET_calls_GetSinglePayeeAsync_and_returns_view_with_model() {
            // Arrange
            var payee = new Payee();
            _mockService.Setup(m => m.GetSinglePayeeAsync(It.IsAny<int?>(), It.IsAny<bool>())).ReturnsAsync(payee);

            // Act
            var result = await _controller.Edit(1);
            var viewResult = result as ViewResult;
            var model = viewResult.Model as Payee;

            // Assert
            _mockService.Verify(m => m.GetSinglePayeeAsync(1, false), Times.Once());
            Assert.IsNotNull(viewResult);
            Assert.AreEqual("Edit", viewResult.ViewName);
            Assert.AreSame(payee, model);
        }

        [TestMethod]
        public async Task Edit_GET_correctly_populates_category_select_list() {
            // Arrange
            var categories = new List<BudgetCategory> {
                new BudgetCategory { ID = 1 },
                new BudgetCategory { ID = 2 },
                new BudgetCategory { ID = 3 }
            }.AsQueryable();
            var payee = new Payee { BudgetCategoryID = 1 };
            _mockService.Setup(m => m.GetCategories(It.IsAny<bool>())).Returns(categories);
            _mockService.Setup(m => m.GetSinglePayeeAsync(It.IsAny<int?>(), It.IsAny<bool>())).ReturnsAsync(payee);

            // Act
            var result = (ViewResult)(await _controller.Edit(1));

            // Assert
            AssertThatViewDataIsSelectList(result.ViewData, _categorySelectListKeyRO, categories.Select(c => c.ID.ToString()), payee.BudgetCategoryID.ToString());
        }

        [TestMethod]
        public async Task Edit_GET_returns_NotFound_when_ExpenseTrackerException_thrown() {
            // Arrange
            _mockService.Setup(m => m.GetSinglePayeeAsync(It.IsAny<int?>(), It.IsAny<bool>())).ThrowsAsync(new ExpenseTrackerException());

            // Act
            var result = await _controller.Edit(null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Edit_GET_throws_exceptions_not_of_type_ExpenseTrackerException() {
            // Arrange
            _mockService.Setup(m => m.GetSinglePayeeAsync(It.IsAny<int?>(), It.IsAny<bool>())).ThrowsAsync(new Exception());

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(() => _controller.Edit(1));
        }

        #endregion // Edit GET

        #region Edit POST

        [TestMethod]
        public async Task Edit_POST_calls_UpdatePayeeAsync_and_redirects_to_index() {
            // Arrange
            var payee = new Payee();

            // Act
            var result = await _controller.Edit(1, payee);
            var redirectResult = result as RedirectToActionResult;

            // Assert
            _mockService.Verify(m => m.UpdatePayeeAsync(1, payee), Times.Once());
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.ActionName);
        }

        [TestMethod]
        public async Task Edit_POST_returns_payee_to_view_when_modelstate_invalid() {
            // Arrange
            var payee = new Payee();
            _controller.ModelState.AddModelError("test", "test");

            // Act
            var result = await _controller.Edit(1, payee);
            var viewResult = result as ViewResult;
            var model = viewResult.Model as Payee;

            // Assert
            Assert.IsNotNull(viewResult);
            Assert.AreEqual("Edit", viewResult.ViewName);
            Assert.AreSame(payee, model);
        }

        [TestMethod]
        public async Task Edit_POST_correctly_populates_category_list_when_ModelState_invalid() {
            // Arrange
            var categories = new List<BudgetCategory> {
                new BudgetCategory { ID = 1 },
                new BudgetCategory { ID = 2 },
                new BudgetCategory { ID = 3 }
            }.AsQueryable();
            var payee = new Payee { BudgetCategoryID = 1 };
            _mockService.Setup(m => m.GetCategories(It.IsAny<bool>())).Returns(categories);
            _controller.ModelState.AddModelError("test", "test");

            // Act
            var result = (ViewResult)(await _controller.Edit(1, payee));

            // Assert
            AssertThatViewDataIsSelectList(result.ViewData, _categorySelectListKeyRO, categories.Select(c => c.ID.ToString()), payee.BudgetCategoryID.ToString());
        }

        [TestMethod]
        public async Task Edit_POST_returns_NotFound_when_IdMismatchException_thrown() {
            // Arrange
            _mockService.Setup(m => m.UpdatePayeeAsync(It.IsAny<int>(), It.IsAny<Payee>())).ThrowsAsync(new IdMismatchException());
            var payee = new Payee();

            // Act
            var result = await _controller.Edit(1, payee);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Edit_POST_returns_NotFound_when_ConcurrencyException_thrown_and_payee_does_not_exist() {
            // Arrange
            var payee = new Payee();
            _mockService.Setup(m => m.UpdatePayeeAsync(It.IsAny<int>(), It.IsAny<Payee>())).ThrowsAsync(new ConcurrencyException());
            _mockService.Setup(m => m.PayeeExists(It.IsAny<int>())).Returns(false);

            // Act
            var result = await _controller.Edit(1, payee);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Edit_POST_thows_exceptions_not_of_type_IdMismatch() {
            // Arrange
            var payee = new Payee();
            _mockService.Setup(m => m.UpdatePayeeAsync(It.IsAny<int>(), It.IsAny<Payee>())).ThrowsAsync(new Exception());

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(() => _controller.Edit(1, payee));
        }

        #endregion // Edit POST

        #endregion // Edit Tests
    
        #endregion // Tests
    }
}