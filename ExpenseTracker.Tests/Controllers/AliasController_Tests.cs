using ExpenseTracker.Exceptions;
using ExpenseTracker.Models;
using ExpenseTracker.Services;
using ExpenseTracker.TestResources;
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
    public class AliasController_Tests : TestCommon
    {
        #region Private Members

        private AliasController _controller;
        private Mock<IAliasManagerService> _mockService;

        #endregion // Private Members

        #region Test Initialize

        [TestInitialize]
        public void Initialize_test_objects() {
            _mockService = new Mock<IAliasManagerService>();
            _controller = new AliasController(_mockService.Object);
        }

        #endregion // Test Initialize

        #region Tests

        #region Create Tests

        #region Create GET

        [TestMethod]
        public async Task Create_GET_returns_create_view() {
            // Arrange
            SetupControllerRouteData(_controller, "action", "Create");

            // Act
            var result = await _controller.Create();
            var viewResult = result as ViewResult;

            // Assert
            Assert.IsNotNull(viewResult);
            Assert.AreEqual("Create", viewResult.ViewName);
        }

        // TODO: Move this test to View Model tests since view model now handles this
        // [TestMethod]
        // public async Task Create_GET_correctly_populates_payee_select_list() {
        //     // Arrange
        //     var payees = new List<Payee> {
        //         new Payee { ID = 1 },
        //         new Payee { ID = 2 }
        //     }.AsQueryable();
        //     int testID = 1;
        //     _mockService.Setup(m => m.GetPayees(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
        //         .Returns(payees);
            
        //     // Act
        //     var result = (ViewResult)(await _controller.Create());

        //     // Assert
        //     AssertThatViewDataIsSelectList(result.ViewData, _payeeListKeyRO, payees.Select(p => p.ID.ToString()), testID.ToString());
        // }

        #endregion // Create GET

        #region Create POST

        // TODO: Add test that Alias.Index redirects to Payee.Index
        [TestMethod]
        public async Task Create_POST_calls_AddAliasAsync_and_redirects_to_Index() {
            // Arrange
            var alias = new AliasCrudVm();

            // Act
            var result = await _controller.Create(alias);
            var redirectResult = result as RedirectToActionResult;

            // Assert
            _mockService.Verify(m => m.AddAliasAsync(alias.Name, alias.PayeeName), Times.Once());
            Assert.AreEqual("Index", redirectResult.ActionName);
        }

        [TestMethod]
        public async Task Create_POST_returns_to_create_view_when_ModelValidationException_thrown() {
            // Arrange
            var alias = new AliasCrudVm();
            _mockService.Setup(m => m.AddAliasAsync(It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new ModelValidationException());

            // Act
            var result = await _controller.Create(alias);
            var viewResult = result as ViewResult;

            // Assert
            Assert.IsNotNull(viewResult);
            Assert.AreEqual("Create", viewResult.ViewName);
        }

        // TODO: Move this test to model validation tests. View model now handles the select list
        // [TestMethod]
        // public async Task Create_POST_correctly_populates_select_list_when_ModelValidationException_thrown() {
        //     // Arrange
        //     var payees = new List<Payee> {
        //         new Payee { ID = 1 },
        //         new Payee { ID = 2 }
        //     }.AsQueryable();
        //     _mockService.Setup(m => m.GetPayees(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(payees);
        //     _mockService.Setup(m => m.AddAliasAsync(It.IsAny<string>(), It.IsAny<int>())).ThrowsAsync(new ModelValidationException());
        //     var alias = new AliasCrudVm { PayeeID = 1 };

        //     // Act
        //     var result = (ViewResult)(await _controller.Create(alias));

        //     // Assert
        //     AssertThatViewDataIsSelectList(result.ViewData, _payeeListKeyRO, payees.Select(p => p.ID.ToString()), alias.PayeeID.ToString());
        // }

        [TestMethod]
        public async Task Create_POST_adds_modelstate_error_when_UniqueConstraintViolationException_thrown() {
            // Arrange
            var alias = new AliasCrudVm { NavId = 1 };
            _mockService.Setup(m => m.AddAliasAsync(It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new ModelValidationException());

            // Act
            var result = await _controller.Create(alias);
            var viewResult = result as ViewResult;
            var model = viewResult.Model as AliasCrudVm;

            // Assert
            Assert.AreEqual(1, _controller.ModelState.ErrorCount);
            Assert.IsNotNull(viewResult);
            Assert.AreEqual("Create", viewResult.ViewName);
            Assert.AreEqual(alias.NavId, model.NavId);
        }

        #endregion // Create POST

        #endregion // Create Tests

        #region Delete Tests

        #region Delete GET

        [TestMethod]
        public async Task Delete_GET_returns_delete_view_with_correct_model() {
            // Arrange
            var alias = new Alias { ID = 1 };
            _mockService.Setup(m => m.GetSingleAliasAsync(It.IsAny<int?>(), It.IsAny<bool>())).ReturnsAsync(alias);
            SetupControllerRouteData(_controller, "action", "Delete");

            // Act
            var result = await _controller.Delete(1);
            var viewResult = result as ViewResult;
            var model = viewResult.Model as AliasCrudVm;

            // Assert
            _mockService.Verify(m => m.GetSingleAliasAsync(1, true), Times.Once());
            Assert.IsNotNull(viewResult);
            Assert.AreEqual("Delete", viewResult.ViewName);
            Assert.AreEqual(alias.ID, model.NavId);
        }

        [TestMethod]
        public async Task Delete_GET_returns_NotFound_if_ExpenseTrackerException_thrown() {
            // Arrange
            SetupControllerRouteData(_controller, "action", "Edit");
            _mockService.Setup(m => m.GetSingleAliasAsync(It.IsAny<int?>(), It.IsAny<bool>())).ThrowsAsync(new ExpenseTrackerException());

            // Act
            var result = await _controller.Delete(null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Delete_GET_throws_exceptions_not_of_type_ExpenseTrackerException() {
            // Arrange
            _mockService.Setup(m => m.GetSingleAliasAsync(It.IsAny<int?>(), It.IsAny<bool>())).ThrowsAsync(new Exception());

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(() =>
                _controller.Delete(1));
        }

        #endregion // Delete GET

        #region Delete POST

        // TODO: Add test to check that Alias.Index redirects to Payee.Index
        [TestMethod]
        public async Task Delete_POST_calls_RemoveAliasAsync_and_redirects_to_index() {
            // Act
            var result = await _controller.DeleteConfirmed(1);
            var redirectResult = result as RedirectToActionResult;

            // Assert
            _mockService.Verify(m => m.RemoveAliasAsync(1), Times.Once());
            Assert.AreEqual("Index", redirectResult.ActionName);
        }

        #endregion // Delete POST

        #endregion // Delete Tests

        #region Edit Tests

        #region Edit GET

        [TestMethod]
        public async Task Edit_GET_returns_Edit_view_with_correct_model() {
            // Arrange
            var alias = new Alias { ID = 1 };
            SetupControllerRouteData(_controller, "action", "Edit");
            _mockService.Setup(m => m.GetSingleAliasAsync(It.IsAny<int?>(), It.IsAny<bool>())).ReturnsAsync(alias);

            // Act
            var result = await _controller.Edit(1);
            var viewResult = result as ViewResult;
            var model = viewResult.Model as AliasCrudVm;

            // Assert
            _mockService.Verify(m => m.GetSingleAliasAsync(1, true), Times.Once());
            Assert.IsNotNull(viewResult);
            Assert.AreEqual("Edit", viewResult.ViewName);
            Assert.AreEqual(alias.ID, model.NavId);
        }

        // TODO: move this test to View Model tests since this list is now populated from view model
        // [TestMethod]
        // public async Task Edit_GET_correctly_populates_select_list() {
        //     // Arrange
        //     var payees = new List<Payee> {
        //         new Payee { ID = 1 },
        //         new Payee { ID = 2 },
        //         new Payee { ID = 3 }
        //     }.AsQueryable();
        //     var alias = new Alias { PayeeID = 2 };
        //     SetupControllerRouteData(_controller, "action", "Edit");
        //     _mockService.Setup(m => m.GetPayees(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(payees);
        //     _mockService.Setup(m => m.GetSingleAliasAsync(It.IsAny<int?>(), It.IsAny<bool>())).ReturnsAsync(alias);

        //     // Act
        //     var result = (ViewResult)(await _controller.Edit(1));

        //     // Assert
        //     AssertThatViewDataIsSelectList(result.ViewData, _payeeListKeyRO, payees.Select(p => p.ID.ToString()), alias.PayeeID.ToString());
        // }

        [TestMethod]
        public async Task Edit_GET_returns_NotFound_when_ExpenseTrackerException_thrown() {
            // Arrange
            _mockService.Setup(m => m.GetSingleAliasAsync(It.IsAny<int?>(), It.IsAny<bool>())).ThrowsAsync(new ExpenseTrackerException());

            // Act
            var result = await _controller.Edit(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Edit_GET_throws_exceptions_not_of_type_ExpenseTrackerException() {
            // Arrange
            _mockService.Setup(m => m.GetSingleAliasAsync(It.IsAny<int?>(), It.IsAny<bool>())).ThrowsAsync(new Exception());

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(() =>
                _controller.Edit(1));
        }

        #endregion // Edit GET

        #region Edit POST

        // TODO: Add tests to verify Alias.Index redirects to Payee.Index
        [TestMethod]
        public async Task Edit_POST_calls_UpdateAliasAsync_and_redirects_to_index() {
            // Arrange
            var alias = new AliasCrudVm { NavId = 1 };
            SetupControllerRouteData(_controller, "id", 1);

            // Act
            var result = await _controller.Edit(alias);
            var redirectResult = result as RedirectToActionResult;

            // Assert
            _mockService.Verify(m => m.UpdateAliasAsync(1, alias.Name, alias.PayeeName), Times.Once());
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.ActionName);
        }

        [TestMethod]
        public async Task Edit_POST_returns_NotFound_when_IdMismatchException_thrown() {
            // Arrange
            var alias = new AliasCrudVm { NavId = 1 };
            _mockService.Setup(m => m.UpdateAliasAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new IdMismatchException());
            SetupControllerRouteData(_controller, "id", 1);

            // Act
            var result = await _controller.Edit(alias);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Edit_POST_returns_NotFound_when_ConcurrencyException_thrown_and_alias_does_not_exist() {
            // Arrange
            var alias = new AliasCrudVm { NavId = 1 };
            _mockService.Setup(m => m.UpdateAliasAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new ConcurrencyException());
            _mockService.Setup(m => m.AliasExists(It.IsAny<int>())).Returns(false);
            SetupControllerRouteData(_controller, "id", 1);

            // Act
            var result = await _controller.Edit(alias);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Edit_POST_rethrows_ConcurrencyException_when_alias_exists() {
            // Arrange
            var alias = new AliasCrudVm { NavId = 1 };
            _mockService.Setup(m => m.UpdateAliasAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new ConcurrencyException());
            _mockService.Setup(m => m.AliasExists(It.IsAny<int>())).Returns(true);
            SetupControllerRouteData(_controller, "id", 1);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ConcurrencyException>(() =>
                _controller.Edit(alias));
        }

        [TestMethod]
        public async Task Edit_POST_throws_exceptions_not_of_type_IdMismatch() {
            // Arrange
            var alias = new AliasCrudVm { NavId = 1 };
            _mockService.Setup(m => m.UpdateAliasAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new Exception());
            SetupControllerRouteData(_controller, "id", 1);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(() =>
                _controller.Edit(alias));
        }

        [TestMethod]
        public async Task Edit_POST_returns_to_view_with_correct_model_when_ModelValidationException_thrown() {
            // Arrange
            var alias = new AliasCrudVm{ NavId = 1 };
            SetupControllerRouteData(_controller, "id", alias.NavId);
            _mockService.Setup(m => m.UpdateAliasAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new ModelValidationException());

            // Act
            var result = await _controller.Edit(alias);
            var viewResult = result as ViewResult;
            var model = viewResult.Model as AliasCrudVm;

            // Assert
            Assert.IsNotNull(viewResult);
            Assert.AreEqual("Edit", viewResult.ViewName);
            Assert.AreSame(alias, model);
        }

        // TODO: Move this test to AliasCrudVm tests because that view model not handles the select list
        // [TestMethod]
        // public async Task Edit_POST_correctly_populates_select_list_when_ModelState_invalid() {
        //     // Arrange
        //     var payees = new List<Payee> {
        //         new Payee { ID = 1 },
        //         new Payee { ID = 2 },
        //         new Payee { ID = 3 }
        //     }.AsQueryable();
        //     var alias = new AliasCrudVm { PayeeID = 3 };
        //     _controller.ModelState.AddModelError("test", "test");
        //     _mockService.Setup(m => m.GetPayees(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(payees);

        //     // Act
        //     var result = (ViewResult)(await _controller.Edit(alias));

        //     // Assert
        //     AssertThatViewDataIsSelectList(result.ViewData, _payeeListKeyRO, payees.Select(p => p.ID.ToString()), alias.PayeeID.ToString());
        // }

        [TestMethod]
        public async Task Edit_POST_adds_modelstate_error_when_ModelValidationException_thrown() {
            // Arrange
            var alias = new AliasCrudVm { NavId = 3 };
            _mockService.Setup(m => m.UpdateAliasAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new ModelValidationException());
            SetupControllerRouteData(_controller, "id", 3);

            // Act
            var result = await _controller.Edit(alias);
            var viewResult = result as ViewResult;
            var model = viewResult.Model as AliasCrudVm;

            // Assert
            Assert.AreEqual(1, _controller.ModelState.ErrorCount);
            Assert.IsNotNull(viewResult);
            Assert.AreEqual("Edit", viewResult.ViewName);
            Assert.AreEqual(alias.NavId, model.NavId);
        }

        #endregion // Edit POST

        #endregion // Edit Tests
    
        #endregion // Tests
    }
}