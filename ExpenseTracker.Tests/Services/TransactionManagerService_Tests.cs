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
        private Mock<IBudgetRepo> mockRepo;
        private ITransactionManagerService testService;
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Initialize_test_objects() {
            switch (TestContext.TestName) {
                case nameof(AddTransactionAsync_adds_transaction_then_saves):
                case nameof(RemoveTransactionAsync_removes_transaction_then_saves):
                case nameof(UpdateTransactionAsync_edits_transaction_then_saves):
                    mockRepo = new Mock<IBudgetRepo>(MockBehavior.Strict);
                    break;
                default:
                    mockRepo = new Mock<IBudgetRepo>();
                    break;
            }
            testService = new TransactionManagerService(mockRepo.Object);
        }

        [TestMethod]
        public async Task GetSingleTransactionAsync_returns_transaction() {
            // Arrange
            var transaction = new Transaction { ID = 3 };
            var mockTransExt = new Mock<ITransactionExtMask>();
            mockTransExt.Setup(m => m.SingleOrDefaultAsync(It.IsAny<int>())).ReturnsAsync(transaction);
            ExtensionFactory.TransactionExtFactory = ext => mockTransExt.Object;

            // Act
            var result = await testService.GetSingleTransactionAsync(3);

            // Assert
            mockTransExt.Verify(m => m.SingleOrDefaultAsync(3), Times.Once());
            Assert.AreEqual(3, result.ID);
        }

        [TestMethod]
        public async Task GetSingleTransactionAsync_throws_NullIdException_when_null_is_passed() {
            // Act & Assert
            await Assert.ThrowsExceptionAsync<NullIdException>(() =>
                testService.GetSingleTransactionAsync(null)
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
                testService.GetSingleTransactionAsync(40)
            , "IdNotFoundException should have been thrown");
        }

        [TestMethod]
        public async Task AddTransactionAsync_adds_transaction_then_saves() {
            // Arrange
            var transaction = new Transaction { Amount = 10 };
            var sequence = new MockSequence();
            mockRepo.InSequence(sequence).Setup(m => m.AddTransaction(It.IsAny<Transaction>()));
            mockRepo.InSequence(sequence).Setup(m => m.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            await testService.AddTransactionAsync(transaction);

            // Assert
            mockRepo.Verify(m => m.AddTransaction(transaction), Times.Once());
            mockRepo.Verify(m => m.SaveChangesAsync(), Times.Once());
        }

        [TestMethod]
        public async Task RemoveTransactionAsync_removes_transaction_then_saves() {
            // Arrange
            var transaction = new Transaction { ID = 1 };
            var transactions = new List<Transaction> { transaction }.AsQueryable();
            var sequence = new MockSequence();
            mockRepo.Setup(m => m.GetTransactions(It.IsAny<bool>(), It.IsAny<bool>())).Returns(transactions);
            mockRepo.InSequence(sequence).Setup(m => m.DeleteTransaction(It.IsAny<Transaction>()));
            mockRepo.InSequence(sequence).Setup(m => m.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            await testService.RemoveTransactionAsync(1);

            // Assert
            mockRepo.Verify(m => m.DeleteTransaction(transaction), Times.Once());
            mockRepo.Verify(m => m.SaveChangesAsync(), Times.Once());
        }

        [TestMethod]
        public async Task RemoveTransactionAsync_skips_delete_if_id_not_present() {
            // Arrange
            var transactions = new List<Transaction>().AsQueryable();
            mockRepo.Setup(m => m.GetTransactions(It.IsAny<bool>(), It.IsAny<bool>())).Returns(transactions);

            // Act
            await testService.RemoveTransactionAsync(20);

            // Assert
            mockRepo.Verify(m => m.DeleteTransaction(It.IsAny<Transaction>()), Times.Never());
        }

        [DataTestMethod]
        [DataRow(1, true), DataRow(40, false)]
        public void TransactionExists_tells_if_an_id_exists(int testId, bool expectedResult) {
            // Arrange
            var transactions = new List<Transaction> {
                new Transaction { ID = 1 }
            }.AsQueryable();
            mockRepo.Setup(m => m.GetTransactions(It.IsAny<bool>(), It.IsAny<bool>())).Returns(transactions);

            // Act
            var result = testService.TransactionExists(testId);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        public async Task UpdateTransactionAsync_edits_transaction_then_saves() {
            // Arrange
            var testID = 1;
            var transaction = new Transaction { ID = testID };
            var sequence = new MockSequence();
            mockRepo.InSequence(sequence).Setup(m => m.EditTransaction(It.IsAny<Transaction>()));
            mockRepo.InSequence(sequence).Setup(m => m.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            await testService.UpdateTransactionAsync(testID, transaction);

            // Assert
            mockRepo.Verify(m => m.EditTransaction(transaction), Times.Once());
            mockRepo.Verify(m => m.SaveChangesAsync(), Times.Once());
        }

        [TestMethod]
        public async Task UpdateTransactionAsync_throws_IdMismatchException_when_id_doesnt_match_TransactionID() {
            // Arrange
            var testID = 1;
            var transaction = new Transaction { ID = testID + 1 };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<IdMismatchException>(() =>
                testService.UpdateTransactionAsync(testID, transaction)
            , $"No exception thrown for Id = {testID} and Transaction.ID = {transaction.ID}");
        }

        [DataTestMethod]
        [DataRow(true), DataRow(false)]
        public void GetOrderedTransactions_orders_by_specified_property(bool descending) {
            // Arrange
            string property = nameof(Transaction.Amount);
            var transactions = new List<Transaction> {
                new Transaction { Amount = 20 },
                new Transaction { Amount = 30 },
                new Transaction { Amount = 10 }
            }.AsQueryable();
            int pos10 = descending ? 2 : 0;
            int pos30 = descending ? 0 : 2;
            mockRepo.Setup(m => m.GetTransactions(It.IsAny<bool>(), It.IsAny<bool>())).Returns(transactions);

            // Act
            var result = testService.GetOrderedTransactions(property, descending).ToArray();

            // Assert
            Assert.AreEqual(10, result[pos10].Amount, "Transaction not ordered correctly");
            Assert.AreEqual(20, result[1].Amount, "Transaction not ordered correctly");
            Assert.AreEqual(30, result[pos30].Amount, "Transaction not ordered correctly");
        }

        [TestMethod]
        public void GetOrderedPayees_throws_ArgumentException_when_ordering_by_non_existant_property() {
            // Arrange
            string property = nameof(Transaction.Amount) + "FAKE";
            var transactions = new List<Transaction>().AsQueryable();

            mockRepo.Setup(m => m.GetTransactions(It.IsAny<bool>(), It.IsAny<bool>())).Returns(transactions);
            
            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() =>
                testService.GetOrderedTransactions(property)
            , $"ArgumentException should be thrown when attempting to sort by Transaction.{property}");
        }
    }
}