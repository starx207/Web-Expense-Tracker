using ExpenseTracker.Exceptions;
using ExpenseTracker.Models;
using ExpenseTracker.Repository.Extensions;
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
    public class TransactionController_Tests : TestCommon
    {
        private TransactionController controller;
        private readonly string categorySelectListKey = "CategoryList";
        private readonly string payeeSelectListKey = "PayeeList";
        private Mock<ITransactionManagerService> mockService;

        [TestInitialize]
        public void InitializeTestObjects() {
            mockService = new Mock<ITransactionManagerService>();
            controller = new TransactionController(mockService.Object);
        }

        #region Index Tests
            [TestMethod]
            public async Task Index_GET_returns_View() {
                // Act
                var result = await controller.Index();
                var viewResult = result as ViewResult;

                // Assert
                Assert.IsNotNull(viewResult);
                Assert.AreEqual("Index", viewResult.ViewName);
            }   

            [TestMethod]
            public async Task Index_GET_passes_list_of_transactions_to_viewmodel() {
                // Arrange
                var transactions = new List<Transaction>();
                var mockTransactionExt = new Mock<ITransactionExtMask>();
                mockTransactionExt.Setup(m => m.ToListAsync()).ReturnsAsync(transactions);
                ExtensionFactory.TransactionExtFactory = ext => mockTransactionExt.Object;

                // Act
                var result = (ViewResult)(await controller.Index());
                var model = result.Model;

                // Assert
                mockService.Verify(m => m.GetTransactions(true, true), Times.Once());
                Assert.AreSame(transactions, model);
            }  
        #endregion

        #region Details Tests
            [TestMethod]
            public async Task Details_GET_returns_view_with_correct_model() {
                // Arrange
                var transaction = new Transaction();
                mockService.Setup(m => m.GetSingleTransactionAsync(It.IsAny<int?>(), It.IsAny<bool>())).ReturnsAsync(transaction);

                // Act
                var result = await controller.Details(1);
                var viewResult = result as ViewResult;
                var model = viewResult.Model as Transaction;

                // Assert
                mockService.Verify(m => m.GetSingleTransactionAsync(1, true), Times.Once());
                Assert.IsNotNull(viewResult);
                Assert.AreEqual("Details", viewResult.ViewName);
                Assert.AreSame(transaction, model);
            }

            [TestMethod]
            public async Task Details_GET_returns_NotFound_when_ExpenseTrackerException_thrown() {
                // Arrange
                mockService.Setup(m => m.GetSingleTransactionAsync(It.IsAny<int?>(), It.IsAny<bool>())).ThrowsAsync(new ExpenseTrackerException());

                // Act
                var result = await controller.Details(null);

                // Assert
                Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            }

            [TestMethod]
            public async Task Details_GET_throws_exceptions_not_of_type_ExpenseTrackerException() {
                // Arrange
                mockService.Setup(m => m.GetSingleTransactionAsync(It.IsAny<int?>(), It.IsAny<bool>())).ThrowsAsync(new Exception());

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
            public void Create_GET_populates_category_and_payee_select_lists() {
                // Arrange
                var categories = new List<BudgetCategory> {
                    new BudgetCategory { ID = 1 },
                    new BudgetCategory { ID = 2 },
                    new BudgetCategory { ID = 3 }
                }.AsQueryable();
                mockService.Setup(m => m.GetCategories()).Returns(categories);

                var payees = new List<Payee> {
                    new Payee { ID = 1 },
                    new Payee { ID = 2 },
                    new Payee { ID = 3 }
                }.AsQueryable();
                mockService.Setup(m => m.GetPayees(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(payees);

                // Act
                var result = (ViewResult)controller.Create();

                // Assert
                AssertThatViewDataIsSelectList(result.ViewData, categorySelectListKey, categories.Select(c => c.ID.ToString()));
                AssertThatViewDataIsSelectList(result.ViewData, payeeSelectListKey, payees.Select(p => p.ID.ToString()));
            }

            [TestMethod]
            public async Task Create_POST_calls_AddTransactionAsync_and_redirects_to_index() {
                // Arrange
                var transaction = new Transaction();

                // Act
                var result = await controller.Create(transaction);
                var redirectResult = result as RedirectToActionResult;

                // Assert
                mockService.Verify(m => m.AddTransactionAsync(transaction), Times.Once());
                Assert.IsNotNull(redirectResult);
                Assert.AreEqual("Index", redirectResult.ActionName);
            }

            [TestMethod]
            public async Task Create_POST_returns_view_with_transaction_for_invalid_modelstate() {
                // Arrange
                var transaction = new Transaction();
                controller.ModelState.AddModelError("test", "test");

                // Act
                var result = await controller.Create(transaction);
                var viewResult = result as ViewResult;
                var model = viewResult.Model as Transaction;

                // Assert
                Assert.IsNotNull(viewResult);
                Assert.AreEqual("Create", viewResult.ViewName);
                Assert.AreSame(transaction, model);
            }

            [TestMethod]
            public async Task Create_POST_correctly_populates_category_and_payee_selects_on_invalid_modelstate() {
                // Arrange
                var categories = new List<BudgetCategory> {
                    new BudgetCategory { ID = 1 },
                    new BudgetCategory { ID = 2 },
                    new BudgetCategory { ID = 3 }
                }.AsQueryable();
                var payees = new List<Payee> {
                    new Payee { ID = 1 },
                    new Payee { ID = 2 },
                    new Payee { ID = 3 }
                }.AsQueryable();
                var transaction = new Transaction { 
                    OverrideCategoryID = 3,
                    PayeeID = 1
                };
                mockService.Setup(m => m.GetPayees(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(payees);
                mockService.Setup(m => m.GetCategories()).Returns(categories);
                controller.ModelState.AddModelError("test", "test");
                
                // Act
                var result = (ViewResult)(await controller.Create(transaction));

                // Assert
                AssertThatViewDataIsSelectList(result.ViewData, payeeSelectListKey, payees.Select(p => p.ID.ToString()), transaction.PayeeID.ToString());
                AssertThatViewDataIsSelectList(result.ViewData, categorySelectListKey, categories.Select(c => c.ID.ToString()), transaction.OverrideCategoryID.ToString());
            }
        #endregion

        #region "Delete Method Tests"
            [TestMethod]
            public async Task Delete_GET_calls_GetSingleTransactionAsync_and_returns_view() {
                // Arrange
                var transaction = new Transaction();
                mockService.Setup(m => m.GetSingleTransactionAsync(It.IsAny<int?>(), It.IsAny<bool>())).ReturnsAsync(transaction);

                // Act
                var result = await controller.Delete(1);
                var viewResult = result as ViewResult;
                var model = viewResult.Model as Transaction;

                // Assert
                mockService.Verify(m => m.GetSingleTransactionAsync(1, true), Times.Once());
                Assert.IsNotNull(viewResult);
                Assert.AreEqual("Delete", viewResult.ViewName);
                Assert.AreSame(transaction, model);
            }

            [TestMethod]
            public async Task Delete_GET_returns_NotFound_when_ExpenseTrackerException_thrown() {
                // Arrange
                mockService.Setup(m => m.GetSingleTransactionAsync(It.IsAny<int?>(), It.IsAny<bool>())).ThrowsAsync(new ExpenseTrackerException());

                // Act
                var result = await controller.Delete(null);

                // Assert
                Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            }

            [TestMethod]
            public async Task Delete_GET_throws_Exceptions_not_of_type_ExpenseTrackerException() {
                // Arrange
                mockService.Setup(m => m.GetSingleTransactionAsync(It.IsAny<int?>(), It.IsAny<bool>())).ThrowsAsync(new Exception());

                // Act & Assert
                await Assert.ThrowsExceptionAsync<Exception>(() => controller.Delete(1));
            }

            [TestMethod]
            public async Task Delete_POST_calls_RemoveTransactionAsync_and_redirects_to_index() {
                // Act
                var result = await controller.DeleteConfirmed(1);
                var redirectResult = result as RedirectToActionResult;

                // Assert
                mockService.Verify(m => m.RemoveTransactionAsync(1), Times.Once());
                Assert.IsNotNull(redirectResult);
                Assert.AreEqual("Index", redirectResult.ActionName);
            }
        #endregion

        #region Edit Method Tests
            [TestMethod]
            public async Task Edit_GET_calls_GetSingleTransactionAsync_and_returns_view_with_model() {
                // Arrange
                var transaction = new Transaction();
                mockService.Setup(m => m.GetSingleTransactionAsync(It.IsAny<int?>(), It.IsAny<bool>())).ReturnsAsync(transaction);

                // Act
                var result = await controller.Edit(1);
                var viewResult = result as ViewResult;
                var model = viewResult.Model as Transaction;

                // Assert
                mockService.Verify(m => m.GetSingleTransactionAsync(1, false), Times.Once());
                Assert.IsNotNull(viewResult);
                Assert.AreEqual("Edit", viewResult.ViewName);
                Assert.AreSame(transaction, model);
            }

            [TestMethod]
            public async Task Edit_GET_correctly_populates_category_and_payee_select_lists() {
                // Arrange
                var categories = new List<BudgetCategory> {
                    new BudgetCategory { ID = 1 },
                    new BudgetCategory { ID = 2 },
                    new BudgetCategory { ID = 3 }
                }.AsQueryable();
                var payees = new List<Payee> {
                    new Payee { ID = 1 },
                    new Payee { ID = 2 },
                    new Payee { ID = 3 }
                }.AsQueryable();
                var transaction = new Transaction { 
                    OverrideCategoryID = 1,
                    PayeeID = 3
                };
                mockService.Setup(m => m.GetPayees(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(payees);
                mockService.Setup(m => m.GetCategories()).Returns(categories);
                mockService.Setup(m => m.GetSingleTransactionAsync(It.IsAny<int?>(), It.IsAny<bool>())).ReturnsAsync(transaction);

                // Act
                var result = (ViewResult)(await controller.Edit(1));

                // Assert
                AssertThatViewDataIsSelectList(result.ViewData, payeeSelectListKey, payees.Select(p => p.ID.ToString()), transaction.PayeeID.ToString());
                AssertThatViewDataIsSelectList(result.ViewData, categorySelectListKey, categories.Select(c => c.ID.ToString()), transaction.OverrideCategoryID.ToString());
            }

            [TestMethod]
            public async Task Edit_GET_returns_NotFound_when_ExpenseTrackerException_thrown() {
                // Arrange
                mockService.Setup(m => m.GetSingleTransactionAsync(It.IsAny<int?>(), It.IsAny<bool>())).ThrowsAsync(new ExpenseTrackerException());

                // Act
                var result = await controller.Edit(null);

                // Assert
                Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            }

            [TestMethod]
            public async Task Edit_GET_throws_exceptions_not_of_type_ExpenseTrackerException() {
                // Arrange
                mockService.Setup(m => m.GetSingleTransactionAsync(It.IsAny<int?>(), It.IsAny<bool>())).ThrowsAsync(new Exception());

                // Act & Assert
                await Assert.ThrowsExceptionAsync<Exception>(() => controller.Edit(1));
            }

            [TestMethod]
            public async Task Edit_POST_calls_UpdateTransactionAsync_and_redirects_to_index() {
                // Arrange
                var transaction = new Transaction();

                // Act
                var result = await controller.Edit(1, transaction);
                var redirectResult = result as RedirectToActionResult;

                // Assert
                mockService.Verify(m => m.UpdateTransactionAsync(1, transaction), Times.Once());
                Assert.IsNotNull(redirectResult);
                Assert.AreEqual("Index", redirectResult.ActionName);
            }

            [TestMethod]
            public async Task Edit_POST_returns_transaction_to_view_when_modelstate_invalid() {
                // Arrange
                var transaction = new Transaction();
                controller.ModelState.AddModelError("test", "test");

                // Act
                var result = await controller.Edit(1, transaction);
                var viewResult = result as ViewResult;
                var model = viewResult.Model as Transaction;

                // Assert
                Assert.IsNotNull(viewResult);
                Assert.AreEqual("Edit", viewResult.ViewName);
                Assert.AreSame(transaction, model);
            }

            [TestMethod]
            public async Task Edit_POST_correctly_populates_category_and_payee_lists_when_ModelState_invalid() {
                // Arrange
                var categories = new List<BudgetCategory> {
                    new BudgetCategory { ID = 1 },
                    new BudgetCategory { ID = 2 },
                    new BudgetCategory { ID = 3 }
                }.AsQueryable();
                var payees = new List<Payee> {
                    new Payee { ID = 1 },
                    new Payee { ID = 2 },
                    new Payee { ID = 3 }
                }.AsQueryable();
                var transaction = new Transaction { 
                    OverrideCategoryID = 1,
                    PayeeID = 1
                };
                mockService.Setup(m => m.GetPayees(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(payees);
                mockService.Setup(m => m.GetCategories()).Returns(categories);
                controller.ModelState.AddModelError("test", "test");

                // Act
                var result = (ViewResult)(await controller.Edit(1, transaction));

                // Assert
                AssertThatViewDataIsSelectList(result.ViewData, payeeSelectListKey, payees.Select(p => p.ID.ToString()), transaction.PayeeID.ToString());
                AssertThatViewDataIsSelectList(result.ViewData, categorySelectListKey, categories.Select(c => c.ID.ToString()), transaction.OverrideCategoryID.ToString());
            }

            [TestMethod]
            public async Task Edit_POST_returns_NotFound_when_IdMismatchException_thrown() {
                // Arrange
                mockService.Setup(m => m.UpdateTransactionAsync(It.IsAny<int>(), It.IsAny<Transaction>())).ThrowsAsync(new IdMismatchException());
                var transaction = new Transaction();

                // Act
                var result = await controller.Edit(1, transaction);

                // Assert
                Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            }

            [TestMethod]
            public async Task Edit_POST_returns_NotFound_when_ConcurrencyException_thrown_and_transaction_does_not_exist() {
                // Arrange
                var transaction = new Transaction();
                mockService.Setup(m => m.UpdateTransactionAsync(It.IsAny<int>(), It.IsAny<Transaction>())).ThrowsAsync(new ConcurrencyException());
                mockService.Setup(m => m.TransactionExists(It.IsAny<int>())).Returns(false);

                // Act
                var result = await controller.Edit(1, transaction);

                // Assert
                Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            }

            [TestMethod]
            public async Task Edit_POST_thows_exceptions_not_of_type_IdMismatch() {
                // Arrange
                var transaction = new Transaction();
                mockService.Setup(m => m.UpdateTransactionAsync(It.IsAny<int>(), It.IsAny<Transaction>())).ThrowsAsync(new Exception());

                // Act & Assert
                await Assert.ThrowsExceptionAsync<Exception>(() => controller.Edit(1, transaction));
            }
        #endregion
    }
}