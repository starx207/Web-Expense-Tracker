using ExpenseTracker.Exceptions;
using ExpenseTracker.Models;
using ExpenseTracker.Repository;
using ExpenseTracker.Repository.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Services.Tests
{
    [TestClass]
    public class TransactionManagerService_Tests
    {
        #region Private Members

        private Mock<IBudgetRepo> _mockRepo;
        private ITransactionManagerService _testService;

        #endregion // Private Members

        #region Public Properties

        public TestContext TestContext { get; set; }

        #endregion // Public Properties

        #region Test Initialization

        [TestInitialize]
        public void Initialize_test_objects() {
            switch (TestContext.TestName) {
                case nameof(AddTransactionAsync_adds_transaction_then_saves):
                case nameof(RemoveTransactionAsync_removes_transaction_then_saves):
                case nameof(UpdateTransactionAsync_edits_transaction_then_saves):
                    _mockRepo = new Mock<IBudgetRepo>(MockBehavior.Strict);
                    break;
                default:
                    _mockRepo = new Mock<IBudgetRepo>();
                    break;
            }
            _testService = new TransactionManagerService(_mockRepo.Object);
        }

        #endregion // Test Initialization

        #region Tests

        #region GetSingleTransactionAsync Tests

        [TestMethod]
        public async Task GetSingleTransactionAsync_returns_transaction() {
            // Arrange
            var transaction = new Transaction { ID = 3 };
            var mockTransExt = new Mock<ITransactionExtMask>();
            mockTransExt.Setup(m => m.SingleOrDefaultAsync(It.IsAny<int>())).ReturnsAsync(transaction);
            ExtensionFactory.TransactionExtFactory = ext => mockTransExt.Object;

            // Act
            var result = await _testService.GetSingleTransactionAsync(3);

            // Assert
            mockTransExt.Verify(m => m.SingleOrDefaultAsync(3), Times.Once());
            Assert.AreEqual(3, result.ID);
        }

        [TestMethod]
        public async Task GetSingleTransactionAsync_throws_NullIdException_when_null_is_passed() {
            // Act & Assert
            await Assert.ThrowsExceptionAsync<NullIdException>(() =>
                _testService.GetSingleTransactionAsync(null)
            , "No exception was thrown");
        }

        [TestMethod]
        public async Task GetSingleTransactionAsync_throws_IdNotFoundException_when_transaction_doesnt_exist() {
            // Arrange
            var mockTransExt = new Mock<ITransactionExtMask>();
            mockTransExt.Setup(m => m.SingleOrDefaultAsync(It.IsAny<int>())).ReturnsAsync((Transaction)null);
            ExtensionFactory.TransactionExtFactory = ext => mockTransExt.Object;

            // Act & Assert
            await Assert.ThrowsExceptionAsync<IdNotFoundException>(() =>
                _testService.GetSingleTransactionAsync(40)
            , "IdNotFoundException should have been thrown");
        }

        #endregion // GetSingleTransactionAsync Tests

        #region AddTransactionAsync Tests

        [TestMethod]
        public async Task AddTransactionAsync_adds_transaction_then_saves() {
            // Arrange
            var transaction = new Transaction { Amount = 10 };
            var sequence = new MockSequence();
            _mockRepo.InSequence(sequence).Setup(m => m.AddTransaction(It.IsAny<Transaction>()));
            _mockRepo.InSequence(sequence).Setup(m => m.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            await _testService.AddTransactionAsync(transaction);

            // Assert
            _mockRepo.Verify(m => m.AddTransaction(transaction), Times.Once());
            _mockRepo.Verify(m => m.SaveChangesAsync(), Times.Once());
        }

        #endregion // AddTransactionAsync Tests

        #region RemoveTransactionAsync Tests

        [TestMethod]
        public async Task RemoveTransactionAsync_removes_transaction_then_saves() {
            // Arrange
            var transaction = new Transaction { ID = 1 };
            var transactions = new List<Transaction> { transaction }.AsQueryable();
            var sequence = new MockSequence();
            _mockRepo.Setup(m => m.GetTransactions(It.IsAny<bool>(), It.IsAny<bool>())).Returns(transactions);
            _mockRepo.InSequence(sequence).Setup(m => m.DeleteTransaction(It.IsAny<Transaction>()));
            _mockRepo.InSequence(sequence).Setup(m => m.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            await _testService.RemoveTransactionAsync(1);

            // Assert
            _mockRepo.Verify(m => m.DeleteTransaction(transaction), Times.Once());
            _mockRepo.Verify(m => m.SaveChangesAsync(), Times.Once());
        }

        [TestMethod]
        public async Task RemoveTransactionAsync_skips_delete_if_id_not_present() {
            // Arrange
            var transactions = new List<Transaction>().AsQueryable();
            _mockRepo.Setup(m => m.GetTransactions(It.IsAny<bool>(), It.IsAny<bool>())).Returns(transactions);

            // Act
            await _testService.RemoveTransactionAsync(20);

            // Assert
            _mockRepo.Verify(m => m.DeleteTransaction(It.IsAny<Transaction>()), Times.Never());
        }

        #endregion // RemoveTransactionAsync Tests

        #region TransactionExists Tests

        [DataTestMethod]
        [DataRow(1, true), DataRow(40, false)]
        public void TransactionExists_tells_if_an_id_exists(int testId, bool expectedResult) {
            // Arrange
            var transactions = new List<Transaction> {
                new Transaction { ID = 1 }
            }.AsQueryable();
            _mockRepo.Setup(m => m.GetTransactions(It.IsAny<bool>(), It.IsAny<bool>())).Returns(transactions);

            // Act
            var result = _testService.TransactionExists(testId);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        #endregion // TransactionExists Tests

        #region UpdateTransactionAsync Tests

        [TestMethod]
        public async Task UpdateTransactionAsync_edits_transaction_then_saves() {
            // Arrange
            var testID = 1;
            var transaction = new Transaction { ID = testID };
            var sequence = new MockSequence();
            _mockRepo.InSequence(sequence).Setup(m => m.EditTransaction(It.IsAny<Transaction>()));
            _mockRepo.InSequence(sequence).Setup(m => m.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            await _testService.UpdateTransactionAsync(testID, transaction);

            // Assert
            _mockRepo.Verify(m => m.EditTransaction(transaction), Times.Once());
            _mockRepo.Verify(m => m.SaveChangesAsync(), Times.Once());
        }

        [TestMethod]
        public async Task UpdateTransactionAsync_throws_IdMismatchException_when_id_doesnt_match_TransactionID() {
            // Arrange
            var testID = 1;
            var transaction = new Transaction { ID = testID + 1 };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<IdMismatchException>(() =>
                _testService.UpdateTransactionAsync(testID, transaction)
            , $"No exception thrown for Id = {testID} and Transaction.ID = {transaction.ID}");
        }

        #endregion // UpdateTransactionAsync Tests

        #region GetTransactions Tests

        [TestMethod]
        public void GetTransactions_calls_repo_GetTransactions() {
            // Act
            var result = _testService.GetTransactions();

            // Assert
            _mockRepo.Verify(m => m.GetTransactions(false, false), Times.Once());
        }

        #endregion // GetTransactions Tests

        #endregion // Tests
    }
}