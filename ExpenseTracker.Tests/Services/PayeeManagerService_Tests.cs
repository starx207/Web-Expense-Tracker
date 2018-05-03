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
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ExpenseTracker.Services.Tests
{
    [TestClass]
    public class PayeeManagerService_Tests
    {
        #region Private Members

        private Mock<IBudgetRepo> _mockRepo;
        private IPayeeManagerService _testService;

        #endregion // Private Members
        
        #region Public Properties
        
        public TestContext TestContext { get; set; }

        #endregion // Public Properties

        #region Test Initialization

        [TestInitialize]
        public void Initialize_test_objects() {
            switch (TestContext.TestName) {
                case nameof(AddPayeeAsync_adds_payee_then_saves):
                case nameof(RemovePayeeAsync_removes_payee_then_saves):
                case nameof(UpdatePayeeAsync_edits_payee_then_saves):
                    _mockRepo = new Mock<IBudgetRepo>(MockBehavior.Strict);
                    break;
                default:
                    _mockRepo = new Mock<IBudgetRepo>();
                    break;
            }
            _testService = new PayeeManagerService(_mockRepo.Object);
        }

        #endregion // Test Initialization

        #region Tests

        #region GetSinglePayeeAsync Tests

        [TestMethod]
        public async Task GetSinglePayeeAsync_returns_payee() {
            // Arrange
            var payee = new Payee { ID = 3 };
            var mockPayeeExt = new Mock<PayeeExt>();
            mockPayeeExt.Setup(m => m.SingleOrDefaultAsync(It.IsAny<Expression<Func<Payee, bool>>>())).ReturnsAsync(payee);
            ExtensionFactory.PayeeExtFactory = ext => mockPayeeExt.Object;

            // Act
            var result = await _testService.GetSinglePayeeAsync(3);

            // Assert
            Assert.AreEqual(3, result.ID);
        }

        [TestMethod]
        public async Task GetSinglePayeeAsync_throws_NullIdException_when_null_is_passed() {
            // Act & Assert
            await Assert.ThrowsExceptionAsync<NullIdException>(() =>
                _testService.GetSinglePayeeAsync(null)
            , "No exception was thrown");
        }

        [TestMethod]
        public async Task GetSinglePayeeAsync_throws_IdNotFoundException_when_payee_doesnt_exist() {
            // Arrange
            var mockPayeeExt = new Mock<PayeeExt>();
            mockPayeeExt.Setup(m => m.SingleOrDefaultAsync(It.IsAny<Expression<Func<Payee, bool>>>())).ReturnsAsync((Payee)null);
            ExtensionFactory.PayeeExtFactory = ext => mockPayeeExt.Object;

            // Act & Assert
            await Assert.ThrowsExceptionAsync<IdNotFoundException>(() =>
                _testService.GetSinglePayeeAsync(40)
            , "IdNotFoundException should have been thrown");
        }

        #endregion // GetSinglePayeeAsync Tests

        #region AddPayeeAsync Tests

        [TestMethod]
        public async Task AddPayeeAsync_adds_payee_then_saves() {
            // Arrange
            var payee = new Payee { Name = "Test" };
            var sequence = new MockSequence();
            _mockRepo.Setup(m => m.GetPayees(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(new List<Payee>().AsQueryable());
            _mockRepo.InSequence(sequence).Setup(m => m.AddPayee(It.IsAny<Payee>()));
            _mockRepo.InSequence(sequence).Setup(m => m.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            await _testService.AddPayeeAsync(payee);

            // Assert
            _mockRepo.Verify(m => m.AddPayee(payee), Times.Once());
            _mockRepo.Verify(m => m.SaveChangesAsync(), Times.Once());
        }

        [TestMethod]
        public async Task AddPayeeAsync_throw_UniqueConstraintViolationException_when_duplicate_name() {
            // Arrange
            var payees = new List<Payee> {
                new Payee { Name = "test" }
            }.AsQueryable();
            var payee = new Payee { Name = "test" };
            _mockRepo.Setup(m => m.GetPayees(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(payees);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<UniqueConstraintViolationException>(() =>
                _testService.AddPayeeAsync(payee) );
        }

        #endregion // AddPayeeAsync Tests

        #region RemovePayeeAsync Tests

        [TestMethod]
        public async Task RemovePayeeAsync_removes_payee_then_saves() {
            // Arrange
            var payee = new Payee { ID = 1 };
            var payees = new List<Payee> { payee }.AsQueryable();
            var sequence = new MockSequence();
            _mockRepo.Setup(m => m.GetPayees(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(payees);
            _mockRepo.InSequence(sequence).Setup(m => m.DeletePayee(It.IsAny<Payee>()));
            _mockRepo.InSequence(sequence).Setup(m => m.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            await _testService.RemovePayeeAsync(1);

            // Assert
            _mockRepo.Verify(m => m.DeletePayee(payee), Times.Once());
            _mockRepo.Verify(m => m.SaveChangesAsync(), Times.Once());
        }

        [TestMethod]
        public async Task RemovePayeeAsync_skips_delete_if_id_not_present() {
            // Arrange
            var payees = new List<Payee>().AsQueryable();
            _mockRepo.Setup(m => m.GetPayees(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(payees);

            // Act
            await _testService.RemovePayeeAsync(20);

            // Assert
            _mockRepo.Verify(m => m.DeletePayee(It.IsAny<Payee>()), Times.Never());
        }

        #endregion // RemovePayeeAsync Tests

        #region PayeeExists Tests

        [DataTestMethod]
        [DataRow(1, true), DataRow(40, false)]
        public void PayeeExists_tells_if_an_id_exists(int testId, bool expectedResult) {
            // Arrange
            var payees = new List<Payee> {
                new Payee { ID = 1 }
            }.AsQueryable();
            _mockRepo.Setup(m => m.GetPayees(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(payees);

            // Act
            var result = _testService.PayeeExists(testId);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        #endregion // PayeeExists Tests

        #region UpdatePayeeAsync Tests

        [TestMethod]
        public async Task UpdatePayeeAsync_edits_payee_then_saves() {
            // Arrange
            var testID = 1;
            var payee = new Payee { ID = testID };
            var sequence = new MockSequence();
            _mockRepo.InSequence(sequence).Setup(m => m.EditPayee(It.IsAny<Payee>()));
            _mockRepo.InSequence(sequence).Setup(m => m.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            await _testService.UpdatePayeeAsync(testID, payee);

            // Assert
            _mockRepo.Verify(m => m.EditPayee(payee), Times.Once());
            _mockRepo.Verify(m => m.SaveChangesAsync(), Times.Once());
        }

        [TestMethod]
        public async Task UpdatePayeeAsync_throws_IdMismatchException_when_id_doesnt_match_PayeeID() {
            // Arrange
            var testID = 1;
            var payee = new Payee { ID = testID + 1 };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<IdMismatchException>(() =>
                _testService.UpdatePayeeAsync(testID, payee)
            , $"No exception thrown for Id = {testID} and Payee.ID = {payee.ID}");
        }

        #endregion // UpdatePayeeAsync Tests

        #endregion // Tests
    }
}