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
        private AliasController controller;
        private Mock<IAliasManagerService> mockService;
        private readonly string payeeListKey = "PayeeList";

        [TestInitialize]
        public void Initialize_test_objects() {
            mockService = new Mock<IAliasManagerService>();
            controller = new AliasController(mockService.Object);
        }

        #region Create Tests
            [TestMethod]
            public void Create_GET_returns_create_view() {
                // Act
                var result = controller.Create();
                var viewResult = result as ViewResult;

                // Assert
                Assert.IsNotNull(viewResult);
                Assert.AreEqual("Create", viewResult.ViewName);
            }

            [TestMethod]
            public void Create_GET_correctly_populates_payee_select_list() {
                // Arrange
                var payees = new List<Payee> {
                    new Payee { ID = 1 },
                    new Payee { ID = 2 }
                }.AsQueryable();
                int testID = 1;
                mockService.Setup(m => m.GetPayees(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
                    .Returns(payees);
                
                // Act
                var result = (ViewResult)controller.Create(testID);

                // Assert
                AssertThatViewDataIsSelectList(result.ViewData, payeeListKey, payees.Select(p => p.ID.ToString()), testID.ToString());
            }

            [TestMethod]
            public async Task Create_POST_calls_AddAliasAsync_and_redirects_to_payee_Index() {
                // Arrange
                var alias = new Alias();

                // Act
                var result = await controller.Create(alias);
                var redirectResult = result as RedirectToActionResult;

                // Assert
                mockService.Verify(m => m.AddAliasAsync(alias), Times.Once());
                Assert.AreEqual("Payee", redirectResult.ControllerName);
                Assert.AreEqual("Index", redirectResult.ActionName);
            }

            [TestMethod]
            public async Task Create_POST_returns_to_create_view_when_model_state_invalid() {
                // Arrange
                var alias = new Alias();
                controller.ModelState.AddModelError("test", "test");

                // Act
                var result = await controller.Create(alias);
                var viewResult = result as ViewResult;

                // Assert
                Assert.IsNotNull(viewResult);
                Assert.AreEqual("Create", viewResult.ViewName);
            }

            [TestMethod]
            public async Task Create_POST_correctly_populates_select_list_when_invalid_model_state() {
                // Arrange
                var payees = new List<Payee> {
                    new Payee { ID = 1 },
                    new Payee { ID = 2 }
                }.AsQueryable();
                mockService.Setup(m => m.GetPayees(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(payees);
                var alias = new Alias { PayeeID = 1 };
                controller.ModelState.AddModelError("test", "test");

                // Act
                var result = (ViewResult)(await controller.Create(alias));

                // Assert
                AssertThatViewDataIsSelectList(result.ViewData, payeeListKey, payees.Select(p => p.ID.ToString()), alias.PayeeID.ToString());
            }
        #endregion

        #region "Delete Method Tests"
            [TestMethod]
            public async Task Delete_GET_returns_delete_view_with_correct_model() {
                // Arrange
                var alias = new Alias();
                mockService.Setup(m => m.GetSingleAliasAsync(It.IsAny<int?>(), It.IsAny<bool>())).ReturnsAsync(alias);

                // Act
                var result = await controller.Delete(1);
                var viewResult = result as ViewResult;
                var model = viewResult.Model as Alias;

                // Assert
                mockService.Verify(m => m.GetSingleAliasAsync(1, true), Times.Once());
                Assert.IsNotNull(viewResult);
                Assert.AreEqual("Delete", viewResult.ViewName);
                Assert.AreSame(alias, model);
            }

            [TestMethod]
            public async Task Delete_GET_returns_NotFound_if_NullIdException_thrown() {
                // Arrange
                mockService.Setup(m => m.GetSingleAliasAsync(It.IsAny<int?>(), It.IsAny<bool>())).ThrowsAsync(new NullIdException());

                // Act
                var result = await controller.Delete(null);

                // Assert
                Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            }

            [TestMethod]
            public async Task Delete_GET_returns_NotFound_if_IdNotFoundException_thrown() {
                // Arrange
                mockService.Setup(m => m.GetSingleAliasAsync(It.IsAny<int?>(), It.IsAny<bool>())).ThrowsAsync(new IdNotFoundException());

                // Act
                var result = await controller.Delete(1);

                // Assert
                Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            }

            [TestMethod]
            public async Task Delete_GET_throws_exceptions_not_of_type_NullId_or_IdNotFound() {
                // Arrange
                mockService.Setup(m => m.GetSingleAliasAsync(It.IsAny<int?>(), It.IsAny<bool>())).ThrowsAsync(new Exception());

                // Act & Assert
                await Assert.ThrowsExceptionAsync<Exception>(() =>
                    controller.Delete(1));
            }

            [TestMethod]
            public async Task Delete_POST_calls_RemoveAliasAsync_and_redirects_to_Payee_index() {
                // Act
                var result = await controller.DeleteConfirmed(1);
                var redirectResult = result as RedirectToActionResult;

                // Assert
                mockService.Verify(m => m.RemoveAliasAsync(1), Times.Once());
                Assert.AreEqual("Payee", redirectResult.ControllerName);
                Assert.AreEqual("Index", redirectResult.ActionName);
            }
        #endregion

        #region Edit Method Tests
            [TestMethod]
            public async Task Edit_GET_returns_Edit_view_with_correct_model() {
                // Arrange
                var alias = new Alias();
                mockService.Setup(m => m.GetSingleAliasAsync(It.IsAny<int?>(), It.IsAny<bool>())).ReturnsAsync(alias);

                // Act
                var result = await controller.Edit(1);
                var viewResult = result as ViewResult;
                var model = viewResult.Model as Alias;

                // Assert
                mockService.Verify(m => m.GetSingleAliasAsync(1, false), Times.Once());
                Assert.IsNotNull(viewResult);
                Assert.AreEqual("Edit", viewResult.ViewName);
                Assert.AreSame(alias, model);
            }

            [TestMethod]
            public async Task Edit_GET_correctly_populates_select_list() {
                // Arrange
                var payees = new List<Payee> {
                    new Payee { ID = 1 },
                    new Payee { ID = 2 },
                    new Payee { ID = 3 }
                }.AsQueryable();
                var alias = new Alias { PayeeID = 2 };
                mockService.Setup(m => m.GetPayees(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(payees);
                mockService.Setup(m => m.GetSingleAliasAsync(It.IsAny<int?>(), It.IsAny<bool>())).ReturnsAsync(alias);

                // Act
                var result = (ViewResult)(await controller.Edit(1));

                // Assert
                AssertThatViewDataIsSelectList(result.ViewData, payeeListKey, payees.Select(p => p.ID.ToString()), alias.PayeeID.ToString());
            }

            [TestMethod]
            public async Task Edit_GET_returns_NotFound_when_NullIdException_thrown() {
                // Arrange
                mockService.Setup(m => m.GetSingleAliasAsync(It.IsAny<int?>(), It.IsAny<bool>())).ThrowsAsync(new NullIdException());

                // Act
                var result = await controller.Edit(null);

                // Assert
                Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            }

            [TestMethod]
            public async Task Edit_GET_returns_NotFound_when_IdNotFoundException_thrown() {
                // Arrange
                mockService.Setup(m => m.GetSingleAliasAsync(It.IsAny<int?>(), It.IsAny<bool>())).ThrowsAsync(new IdNotFoundException());

                // Act
                var result = await controller.Edit(1);

                // Assert
                Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            }

            [TestMethod]
            public async Task Edit_GET_throws_exceptions_not_of_type_NullId_or_IdNotFound() {
                // Arrange
                mockService.Setup(m => m.GetSingleAliasAsync(It.IsAny<int?>(), It.IsAny<bool>())).ThrowsAsync(new Exception());

                // Act & Assert
                await Assert.ThrowsExceptionAsync<Exception>(() =>
                    controller.Edit(1));
            }

            [TestMethod]
            public async Task Edit_POST_calls_UpdateAliasAsync_and_redirects_to_payee_index() {
                // Arrange
                var alias = new Alias();

                // Act
                var result = await controller.Edit(1, alias);
                var redirectResult = result as RedirectToActionResult;

                // Assert
                mockService.Verify(m => m.UpdateAliasAsync(1, alias), Times.Once());
                Assert.IsNotNull(redirectResult);
                Assert.AreEqual("Payee", redirectResult.ControllerName);
                Assert.AreEqual("Index", redirectResult.ActionName);
            }

            [TestMethod]
            public async Task Edit_POST_returns_NotFound_when_IdMismatchException_thrown() {
                // Arrange
                var alias = new Alias();
                mockService.Setup(m => m.UpdateAliasAsync(It.IsAny<int>(), It.IsAny<Alias>())).ThrowsAsync(new IdMismatchException());

                // Act
                var result = await controller.Edit(1, alias);

                // Assert
                Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            }

            [TestMethod]
            public async Task Edit_POST_returns_NotFound_when_ConcurrencyException_thrown_and_alias_does_not_exist() {
                // Arrange
                var alias = new Alias();
                mockService.Setup(m => m.UpdateAliasAsync(It.IsAny<int>(), It.IsAny<Alias>())).ThrowsAsync(new ConcurrencyException());
                mockService.Setup(m => m.AliasExists(It.IsAny<int>())).Returns(false);

                // Act
                var result = await controller.Edit(1, alias);

                // Assert
                Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            }

            [TestMethod]
            public async Task Edit_POST_rethrows_ConcurrencyException_when_alias_exists() {
                // Arrange
                var alias = new Alias();
                mockService.Setup(m => m.UpdateAliasAsync(It.IsAny<int>(), It.IsAny<Alias>())).ThrowsAsync(new ConcurrencyException());
                mockService.Setup(m => m.AliasExists(It.IsAny<int>())).Returns(true);

                // Act & Assert
                await Assert.ThrowsExceptionAsync<ConcurrencyException>(() =>
                    controller.Edit(1, alias));
            }

            [TestMethod]
            public async Task Edit_POST_throws_exceptions_not_of_type_IdMismatch() {
                // Arrange
                var alias = new Alias();
                mockService.Setup(m => m.UpdateAliasAsync(It.IsAny<int>(), It.IsAny<Alias>())).ThrowsAsync(new Exception());

                // Act & Assert
                await Assert.ThrowsExceptionAsync<Exception>(() =>
                    controller.Edit(1, alias));
            }

            [TestMethod]
            public async Task Edit_POST_returns_to_view_with_correct_model_when_ModelState_is_invalid() {
                // Arrange
                var alias = new Alias();
                controller.ModelState.AddModelError("test", "test");

                // Act
                var result = await controller.Edit(1, alias);
                var viewResult = result as ViewResult;
                var model = viewResult.Model as Alias;

                // Assert
                Assert.IsNotNull(viewResult);
                Assert.AreEqual("Edit", viewResult.ViewName);
                Assert.AreSame(alias, model);
            }

            [TestMethod]
            public async Task Edit_POST_correctly_populates_select_list_when_ModelState_invalid() {
                // Arrange
                var payees = new List<Payee> {
                    new Payee { ID = 1 },
                    new Payee { ID = 2 },
                    new Payee { ID = 3 }
                }.AsQueryable();
                var alias = new Alias { PayeeID = 3 };
                controller.ModelState.AddModelError("test", "test");
                mockService.Setup(m => m.GetPayees(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(payees);

                // Act
                var result = (ViewResult)(await controller.Edit(1, alias));

                // Assert
                AssertThatViewDataIsSelectList(result.ViewData, payeeListKey, payees.Select(p => p.ID.ToString()), alias.PayeeID.ToString());
            }
        #endregion
    }
}