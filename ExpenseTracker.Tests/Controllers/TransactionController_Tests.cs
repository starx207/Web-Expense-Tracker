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
        #region Private Members

        private TransactionController _controller;
        private readonly string _categorySelectListKeyRO = "CategoryList";
        private readonly string _payeeSelectListKeyRO = "PayeeList";
        private Mock<ITransactionManagerService> _mockService;

        #endregion // Private Members

        #region Test Initialization

        [TestInitialize]
        public void InitializeTestObjects() {
            _mockService = new Mock<ITransactionManagerService>();
            _controller = new TransactionController(_mockService.Object);
        }

        #endregion // Test Initialization

        #region Tests

        #region Index Tests

        [TestMethod]
        public async Task Index_GET_returns_View() {
            // Act
            var result = await _controller.Index();
            var viewResult = result as ViewResult;

            // Assert
            Assert.IsNotNull(viewResult);
            Assert.AreEqual("Index", viewResult.ViewName);
        }   

        [TestMethod]
        public async Task Index_GET_passes_list_of_transactions_to_viewmodel() {
            // Arrange
            var transactions = new List<Transaction>();
            var mockTransactionExt = new Mock<TransactionExt>();
            mockTransactionExt.Setup(m => m.ToListAsync()).ReturnsAsync(transactions);
            ExtensionFactory.TransactionExtFactory = ext => mockTransactionExt.Object;

            // Act
            var result = (ViewResult)(await _controller.Index());
            var model = result.Model;

            // Assert
            _mockService.Verify(m => m.GetTransactions(true, true), Times.Once());
            Assert.AreSame(transactions, model);
        }  

        #endregion // Index Tests

        #region Details Tests

        [TestMethod]
        public async Task Details_GET_returns_view_with_correct_model() {
            // Arrange
            var transaction = new Transaction();
            _mockService.Setup(m => m.GetSingleTransactionAsync(It.IsAny<int?>(), It.IsAny<bool>())).ReturnsAsync(transaction);

            // Act
            var result = await _controller.Details(1);
            var viewResult = result as ViewResult;
            var model = viewResult.Model as Transaction;

            // Assert
            _mockService.Verify(m => m.GetSingleTransactionAsync(1, true), Times.Once());
            Assert.IsNotNull(viewResult);
            Assert.AreEqual("Details", viewResult.ViewName);
            Assert.AreSame(transaction, model);
        }

        [TestMethod]
        public async Task Details_GET_returns_NotFound_when_ExpenseTrackerException_thrown() {
            // Arrange
            _mockService.Setup(m => m.GetSingleTransactionAsync(It.IsAny<int?>(), It.IsAny<bool>())).ThrowsAsync(new ExpenseTrackerException());

            // Act
            var result = await _controller.Details(null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Details_GET_throws_exceptions_not_of_type_ExpenseTrackerException() {
            // Arrange
            _mockService.Setup(m => m.GetSingleTransactionAsync(It.IsAny<int?>(), It.IsAny<bool>())).ThrowsAsync(new Exception());

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
        public void Create_GET_populates_category_and_payee_select_lists() {
            // Arrange
            var categories = new List<BudgetCategory> {
                new BudgetCategory { ID = 1 },
                new BudgetCategory { ID = 2 },
                new BudgetCategory { ID = 3 }
            }.AsQueryable();
            _mockService.Setup(m => m.GetCategories(It.IsAny<bool>())).Returns(categories);

            var payees = new List<Payee> {
                new Payee { ID = 1 },
                new Payee { ID = 2 },
                new Payee { ID = 3 }
            }.AsQueryable();
            _mockService.Setup(m => m.GetPayees(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(payees);

            // Act
            var result = (ViewResult)_controller.Create();

            // Assert
            AssertThatViewDataIsSelectList(result.ViewData, _categorySelectListKeyRO, categories.Select(c => c.ID.ToString()));
            AssertThatViewDataIsSelectList(result.ViewData, _payeeSelectListKeyRO, payees.Select(p => p.ID.ToString()));
        }

        #endregion // Create GET

        #region Create POST

        [TestMethod]
        public async Task Create_POST_calls_AddTransactionAsync_and_redirects_to_index() {
            // Arrange
            var transaction = new Transaction();

            // Act
            var result = await _controller.Create(transaction);
            var redirectResult = result as RedirectToActionResult;

            // Assert
            _mockService.Verify(m => m.AddTransactionAsync(transaction), Times.Once());
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.ActionName);
        }

        [TestMethod]
        public async Task Create_POST_returns_view_with_transaction_for_invalid_modelstate() {
            // Arrange
            var transaction = new Transaction();
            _controller.ModelState.AddModelError("test", "test");

            // Act
            var result = await _controller.Create(transaction);
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
            _mockService.Setup(m => m.GetPayees(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(payees);
            _mockService.Setup(m => m.GetCategories(It.IsAny<bool>())).Returns(categories);
            _controller.ModelState.AddModelError("test", "test");
            
            // Act
            var result = (ViewResult)(await _controller.Create(transaction));

            // Assert
            AssertThatViewDataIsSelectList(result.ViewData, _payeeSelectListKeyRO, payees.Select(p => p.ID.ToString()), transaction.PayeeID.ToString());
            AssertThatViewDataIsSelectList(result.ViewData, _categorySelectListKeyRO, categories.Select(c => c.ID.ToString()), transaction.OverrideCategoryID.ToString());
        }

        #endregion // Create POST

        #endregion // Create Tests

        #region Delete Tests

        #region Delete GET

        [TestMethod]
        public async Task Delete_GET_calls_GetSingleTransactionAsync_and_returns_view() {
            // Arrange
            var transaction = new Transaction();
            _mockService.Setup(m => m.GetSingleTransactionAsync(It.IsAny<int?>(), It.IsAny<bool>())).ReturnsAsync(transaction);

            // Act
            var result = await _controller.Delete(1);
            var viewResult = result as ViewResult;
            var model = viewResult.Model as Transaction;

            // Assert
            _mockService.Verify(m => m.GetSingleTransactionAsync(1, true), Times.Once());
            Assert.IsNotNull(viewResult);
            Assert.AreEqual("Delete", viewResult.ViewName);
            Assert.AreSame(transaction, model);
        }

        [TestMethod]
        public async Task Delete_GET_returns_NotFound_when_ExpenseTrackerException_thrown() {
            // Arrange
            _mockService.Setup(m => m.GetSingleTransactionAsync(It.IsAny<int?>(), It.IsAny<bool>())).ThrowsAsync(new ExpenseTrackerException());

            // Act
            var result = await _controller.Delete(null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Delete_GET_throws_Exceptions_not_of_type_ExpenseTrackerException() {
            // Arrange
            _mockService.Setup(m => m.GetSingleTransactionAsync(It.IsAny<int?>(), It.IsAny<bool>())).ThrowsAsync(new Exception());

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(() => _controller.Delete(1));
        }

        #endregion // Delete GET

        #region Delete POST

        [TestMethod]
        public async Task Delete_POST_calls_RemoveTransactionAsync_and_redirects_to_index() {
            // Act
            var result = await _controller.DeleteConfirmed(1);
            var redirectResult = result as RedirectToActionResult;

            // Assert
            _mockService.Verify(m => m.RemoveTransactionAsync(1), Times.Once());
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.ActionName);
        }

        #endregion // Delete POST

        #endregion // Delete Tests

        #region Edit Tests

        #region Edit GET

        [TestMethod]
        public async Task Edit_GET_calls_GetSingleTransactionAsync_and_returns_view_with_model() {
            // Arrange
            var transaction = new Transaction();
            _mockService.Setup(m => m.GetSingleTransactionAsync(It.IsAny<int?>(), It.IsAny<bool>())).ReturnsAsync(transaction);

            // Act
            var result = await _controller.Edit(1);
            var viewResult = result as ViewResult;
            var model = viewResult.Model as Transaction;

            // Assert
            _mockService.Verify(m => m.GetSingleTransactionAsync(1, false), Times.Once());
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
            _mockService.Setup(m => m.GetPayees(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(payees);
            _mockService.Setup(m => m.GetCategories(It.IsAny<bool>())).Returns(categories);
            _mockService.Setup(m => m.GetSingleTransactionAsync(It.IsAny<int?>(), It.IsAny<bool>())).ReturnsAsync(transaction);

            // Act
            var result = (ViewResult)(await _controller.Edit(1));

            // Assert
            AssertThatViewDataIsSelectList(result.ViewData, _payeeSelectListKeyRO, payees.Select(p => p.ID.ToString()), transaction.PayeeID.ToString());
            AssertThatViewDataIsSelectList(result.ViewData, _categorySelectListKeyRO, categories.Select(c => c.ID.ToString()), transaction.OverrideCategoryID.ToString());
        }

        [TestMethod]
        public async Task Edit_GET_returns_NotFound_when_ExpenseTrackerException_thrown() {
            // Arrange
            _mockService.Setup(m => m.GetSingleTransactionAsync(It.IsAny<int?>(), It.IsAny<bool>())).ThrowsAsync(new ExpenseTrackerException());

            // Act
            var result = await _controller.Edit(null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Edit_GET_throws_exceptions_not_of_type_ExpenseTrackerException() {
            // Arrange
            _mockService.Setup(m => m.GetSingleTransactionAsync(It.IsAny<int?>(), It.IsAny<bool>())).ThrowsAsync(new Exception());

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(() => _controller.Edit(1));
        }

        #endregion // Edit GET

        #region Edit POST

        [TestMethod]
        public async Task Edit_POST_calls_UpdateTransactionAsync_and_redirects_to_index() {
            // Arrange
            var transaction = new Transaction();

            // Act
            var result = await _controller.Edit(1, transaction);
            var redirectResult = result as RedirectToActionResult;

            // Assert
            _mockService.Verify(m => m.UpdateTransactionAsync(1, transaction), Times.Once());
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.ActionName);
        }

        [TestMethod]
        public async Task Edit_POST_returns_transaction_to_view_when_modelstate_invalid() {
            // Arrange
            var transaction = new Transaction();
            _controller.ModelState.AddModelError("test", "test");

            // Act
            var result = await _controller.Edit(1, transaction);
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
            _mockService.Setup(m => m.GetPayees(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(payees);
            _mockService.Setup(m => m.GetCategories(It.IsAny<bool>())).Returns(categories);
            _controller.ModelState.AddModelError("test", "test");

            // Act
            var result = (ViewResult)(await _controller.Edit(1, transaction));

            // Assert
            AssertThatViewDataIsSelectList(result.ViewData, _payeeSelectListKeyRO, payees.Select(p => p.ID.ToString()), transaction.PayeeID.ToString());
            AssertThatViewDataIsSelectList(result.ViewData, _categorySelectListKeyRO, categories.Select(c => c.ID.ToString()), transaction.OverrideCategoryID.ToString());
        }

        [TestMethod]
        public async Task Edit_POST_returns_NotFound_when_IdMismatchException_thrown() {
            // Arrange
            _mockService.Setup(m => m.UpdateTransactionAsync(It.IsAny<int>(), It.IsAny<Transaction>())).ThrowsAsync(new IdMismatchException());
            var transaction = new Transaction();

            // Act
            var result = await _controller.Edit(1, transaction);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Edit_POST_returns_NotFound_when_ConcurrencyException_thrown_and_transaction_does_not_exist() {
            // Arrange
            var transaction = new Transaction();
            _mockService.Setup(m => m.UpdateTransactionAsync(It.IsAny<int>(), It.IsAny<Transaction>())).ThrowsAsync(new ConcurrencyException());
            _mockService.Setup(m => m.TransactionExists(It.IsAny<int>())).Returns(false);

            // Act
            var result = await _controller.Edit(1, transaction);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Edit_POST_thows_exceptions_not_of_type_IdMismatch() {
            // Arrange
            var transaction = new Transaction();
            _mockService.Setup(m => m.UpdateTransactionAsync(It.IsAny<int>(), It.IsAny<Transaction>())).ThrowsAsync(new Exception());

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(() => _controller.Edit(1, transaction));
        }

        #endregion // Edit POST

        #endregion // Edit Tests

        #endregion // Tests
    }
}