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
        private Mock<IPayeeManagerService> mockService;
        private PayeeController controller;
        private readonly string categorySelectListKey = "CategoryList";

        [TestInitialize]
        public void InitializeTestObjects() {
            mockService = new Mock<IPayeeManagerService>();
            controller = new PayeeController(mockService.Object);
        }

        #region Index Tests
            [TestMethod]
            public async Task Index_GET_returns_view() {
                // Act
                var result = await controller.Index();
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
                var result = (ViewResult)(await controller.Index());
                var model = result.Model;

                // Assert
                mockService.Verify(m => m.GetPayees(true, true, false), Times.Once());
                Assert.AreSame(payees, model);
            }  
        #endregion

        #region Details Tests
            [TestMethod]
            public async Task Details_GET_returns_view_with_correct_model() {
                // Arrange
                var payee = new Payee();
                mockService.Setup(m => m.GetSinglePayeeAsync(It.IsAny<int?>(), It.IsAny<bool>())).ReturnsAsync(payee);

                // Act
                var result = await controller.Details(1);
                var viewResult = result as ViewResult;
                var model = viewResult.Model as Payee;

                // Assert
                mockService.Verify(m => m.GetSinglePayeeAsync(1, true), Times.Once());
                Assert.IsNotNull(viewResult);
                Assert.AreEqual("Details", viewResult.ViewName);
                Assert.AreSame(payee, model);
            }

            [TestMethod]
            public async Task Details_GET_returns_NotFound_when_NullIdException_thrown() {
                // Arrange
                mockService.Setup(m => m.GetSinglePayeeAsync(It.IsAny<int?>(), It.IsAny<bool>())).ThrowsAsync(new NullIdException());

                // Act
                var result = await controller.Details(null);

                // Assert
                Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            }

            [TestMethod]
            public async Task Details_GET_returns_NotFound_when_IdNotFoundException_thrown() {
                // Arrange
                mockService.Setup(m => m.GetSinglePayeeAsync(It.IsAny<int?>(), It.IsAny<bool>())).ThrowsAsync(new IdNotFoundException());

                // Act
                var result = await controller.Details(1);

                // Assert
                Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            }

            [TestMethod]
            public async Task Details_GET_throws_exceptions_not_of_type_NullId_or_IdNotFound() {
                // Arrange
                mockService.Setup(m => m.GetSinglePayeeAsync(It.IsAny<int?>(), It.IsAny<bool>())).ThrowsAsync(new Exception());

                // Act & Assert
                await Assert.ThrowsExceptionAsync<Exception>(() => controller.Details(1));
            }
        #endregion

        #region Create Tests
            [TestMethod]
            public void Create_GET_returns_view() {
                // Act
                var result = controller.Create();
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
                mockService.Setup(m => m.GetCategories()).Returns(categories);

                // Act
                var result = (ViewResult)controller.Create();

                // Assert
                AssertThatViewDataIsSelectList(result.ViewData, categorySelectListKey, categories.Select(c => c.ID.ToString()));
            }

            [TestMethod]
            public async Task Create_POST_calls_AddPayeeAsync_and_redirects_to_index() {
                // Arrange
                var payee = new Payee();

                // Act
                var result = await controller.Create(payee);
                var redirectResult = result as RedirectToActionResult;

                // Assert
                mockService.Verify(m => m.AddPayeeAsync(payee), Times.Once());
                Assert.IsNotNull(redirectResult);
                Assert.AreEqual("Index", redirectResult.ActionName);
            }

            [TestMethod]
            public async Task Create_POST_returns_view_with_payee_for_invalid_modelstate() {
                // Arrange
                var payee = new Payee();
                controller.ModelState.AddModelError("test", "test");

                // Act
                var result = await controller.Create(payee);
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
                mockService.Setup(m => m.GetCategories()).Returns(categories);
                controller.ModelState.AddModelError("test", "test");
                
                // Act
                var result = (ViewResult)(await controller.Create(payee));

                // Assert
                AssertThatViewDataIsSelectList(result.ViewData, categorySelectListKey, categories.Select(c => c.ID.ToString()), payee.BudgetCategoryID.ToString());
            }
        #endregion

        #region "Delete Method Tests"
            [TestMethod]
            public async Task Delete_GET_calls_GetSinglePayeeAsync_and_returns_view() {
                // Arrange
                var payee = new Payee();
                mockService.Setup(m => m.GetSinglePayeeAsync(It.IsAny<int?>(), It.IsAny<bool>())).ReturnsAsync(payee);

                // Act
                var result = await controller.Delete(1);
                var viewResult = result as ViewResult;
                var model = viewResult.Model as Payee;

                // Assert
                mockService.Verify(m => m.GetSinglePayeeAsync(1, true), Times.Once());
                Assert.IsNotNull(viewResult);
                Assert.AreEqual("Delete", viewResult.ViewName);
                Assert.AreSame(payee, model);
            }

            [TestMethod]
            public async Task Delete_GET_returns_NotFound_when_NullIdException_thrown() {
                // Arrange
                mockService.Setup(m => m.GetSinglePayeeAsync(It.IsAny<int?>(), It.IsAny<bool>())).ThrowsAsync(new NullIdException());

                // Act
                var result = await controller.Delete(null);

                // Assert
                Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            }

            [TestMethod]
            public async Task Delete_GET_returns_NotFound_when_IdNotFoundException_thrown() {
                // Arrange
                mockService.Setup(m => m.GetSinglePayeeAsync(It.IsAny<int?>(), It.IsAny<bool>())).ThrowsAsync(new IdNotFoundException());

                // Act
                var result = await controller.Delete(1);

                // Assert
                Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            }

            [TestMethod]
            public async Task Delete_GET_throws_Exceptions_not_of_type_NullId_or_IdNotFound() {
                // Arrange
                mockService.Setup(m => m.GetSinglePayeeAsync(It.IsAny<int?>(), It.IsAny<bool>())).ThrowsAsync(new Exception());

                // Act & Assert
                await Assert.ThrowsExceptionAsync<Exception>(() => controller.Delete(1));
            }

            [TestMethod]
            public async Task Delete_POST_calls_RemovePayeeAsync_and_redirects_to_index() {
                // Act
                var result = await controller.DeleteConfirmed(1);
                var redirectResult = result as RedirectToActionResult;

                // Assert
                mockService.Verify(m => m.RemovePayeeAsync(1), Times.Once());
                Assert.IsNotNull(redirectResult);
                Assert.AreEqual("Index", redirectResult.ActionName);
            }
        #endregion

        #region Edit Method Tests
            [TestMethod]
            public async Task Edit_GET_calls_GetSinglePayeeAsync_and_returns_view_with_model() {
                // Arrange
                var payee = new Payee();
                mockService.Setup(m => m.GetSinglePayeeAsync(It.IsAny<int?>(), It.IsAny<bool>())).ReturnsAsync(payee);

                // Act
                var result = await controller.Edit(1);
                var viewResult = result as ViewResult;
                var model = viewResult.Model as Payee;

                // Assert
                mockService.Verify(m => m.GetSinglePayeeAsync(1, false), Times.Once());
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
                mockService.Setup(m => m.GetCategories()).Returns(categories);
                mockService.Setup(m => m.GetSinglePayeeAsync(It.IsAny<int?>(), It.IsAny<bool>())).ReturnsAsync(payee);

                // Act
                var result = (ViewResult)(await controller.Edit(1));

                // Assert
                AssertThatViewDataIsSelectList(result.ViewData, categorySelectListKey, categories.Select(c => c.ID.ToString()), payee.BudgetCategoryID.ToString());
            }

            [TestMethod]
            public async Task Edit_GET_returns_NotFound_when_NullIdException_thrown() {
                // Arrange
                mockService.Setup(m => m.GetSinglePayeeAsync(It.IsAny<int?>(), It.IsAny<bool>())).ThrowsAsync(new NullIdException());

                // Act
                var result = await controller.Edit(null);

                // Assert
                Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            }

            [TestMethod]
            public async Task Edit_GET_returns_NotFound_when_IdNotFoundException_thrown() {
                // Arrange
                mockService.Setup(m => m.GetSinglePayeeAsync(It.IsAny<int?>(), It.IsAny<bool>())).ThrowsAsync(new IdNotFoundException());

                // Act
                var result = await controller.Edit(1);

                // Assert
                Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            }

            [TestMethod]
            public async Task Edit_GET_throws_exceptions_not_of_type_NullId_or_IdNotFound() {
                // Arrange
                mockService.Setup(m => m.GetSinglePayeeAsync(It.IsAny<int?>(), It.IsAny<bool>())).ThrowsAsync(new Exception());

                // Act & Assert
                await Assert.ThrowsExceptionAsync<Exception>(() => controller.Edit(1));
            }

            [TestMethod]
            public async Task Edit_POST_calls_UpdatePayeeAsync_and_redirects_to_index() {
                // Arrange
                var payee = new Payee();

                // Act
                var result = await controller.Edit(1, payee);
                var redirectResult = result as RedirectToActionResult;

                // Assert
                mockService.Verify(m => m.UpdatePayeeAsync(1, payee), Times.Once());
                Assert.IsNotNull(redirectResult);
                Assert.AreEqual("Index", redirectResult.ActionName);
            }

            [TestMethod]
            public async Task Edit_POST_returns_payee_to_view_when_modelstate_invalid() {
                // Arrange
                var payee = new Payee();
                controller.ModelState.AddModelError("test", "test");

                // Act
                var result = await controller.Edit(1, payee);
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
                mockService.Setup(m => m.GetCategories()).Returns(categories);
                controller.ModelState.AddModelError("test", "test");

                // Act
                var result = (ViewResult)(await controller.Edit(1, payee));

                // Assert
                AssertThatViewDataIsSelectList(result.ViewData, categorySelectListKey, categories.Select(c => c.ID.ToString()), payee.BudgetCategoryID.ToString());
            }

            [TestMethod]
            public async Task Edit_POST_returns_NotFound_when_IdMismatchException_thrown() {
                // Arrange
                mockService.Setup(m => m.UpdatePayeeAsync(It.IsAny<int>(), It.IsAny<Payee>())).ThrowsAsync(new IdMismatchException());
                var payee = new Payee();

                // Act
                var result = await controller.Edit(1, payee);

                // Assert
                Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            }

            [TestMethod]
            public async Task Edit_POST_returns_NotFound_when_ConcurrencyException_thrown_and_payee_does_not_exist() {
                // Arrange
                var payee = new Payee();
                mockService.Setup(m => m.UpdatePayeeAsync(It.IsAny<int>(), It.IsAny<Payee>())).ThrowsAsync(new ConcurrencyException());
                mockService.Setup(m => m.PayeeExists(It.IsAny<int>())).Returns(false);

                // Act
                var result = await controller.Edit(1, payee);

                // Assert
                Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            }

            [TestMethod]
            public async Task Edit_POST_thows_exceptions_not_of_type_IdMismatch() {
                // Arrange
                var payee = new Payee();
                mockService.Setup(m => m.UpdatePayeeAsync(It.IsAny<int>(), It.IsAny<Payee>())).ThrowsAsync(new Exception());

                // Act & Assert
                await Assert.ThrowsExceptionAsync<Exception>(() => controller.Edit(1, payee));
            }
        #endregion
    }
}