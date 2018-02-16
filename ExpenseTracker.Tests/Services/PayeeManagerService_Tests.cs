using ExpenseTracker.Exceptions;
using ExpenseTracker.Models;
using ExpenseTracker.Repository;
using ExpenseTracker.Repository.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Services.Tests
{
    [TestClass]
    public class PayeeManagerService_Tests
    {
        private Mock<IBudgetRepo> mockRepo;
        private IPayeeManagerService testService;
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Initialize_test_objects() {
            switch (TestContext.TestName) {
                case nameof(AddPayeeAsync_adds_payee_then_saves):
                case nameof(RemovePayeeAsync_removes_payee_then_saves):
                case nameof(UpdatePayeeAsync_edits_payee_then_saves):
                    mockRepo = new Mock<IBudgetRepo>(MockBehavior.Strict);
                    break;
                default:
                    mockRepo = new Mock<IBudgetRepo>();
                    break;
            }
            testService = new PayeeManagerService(mockRepo.Object);
        }

        [TestMethod]
        public async Task GetSinglePayeeAsync_returns_payee() {
            // Arrange
            var payee = new Payee { ID = 3 };
            var mockPayeeExt = new Mock<IPayeeExtMask>();
            mockPayeeExt.Setup(m => m.SingleOrDefaultAsync(It.IsAny<int>())).ReturnsAsync(payee);
            ExtensionFactory.PayeeExtFactory = ext => mockPayeeExt.Object;

            // Act
            var result = await testService.GetSinglePayeeAsync(3);

            // Assert
            mockPayeeExt.Verify(m => m.SingleOrDefaultAsync(3), Times.Once());
            Assert.AreEqual(3, result.ID);
        }

        [TestMethod]
        public async Task GetSinglePayeeAsync_throws_NullIdException_when_null_is_passed() {
            // Act & Assert
            await Assert.ThrowsExceptionAsync<NullIdException>(() =>
                testService.GetSinglePayeeAsync(null)
            , "No exception was thrown");
        }

        [TestMethod]
        public async Task GetSinglePayeeAsync_throws_IdNotFoundException_when_payee_doesnt_exist() {
            // Arrange
            var mockPayeeExt = new Mock<IPayeeExtMask>();
            mockPayeeExt.Setup(m => m.SingleOrDefaultAsync(It.IsAny<int>())).ReturnsAsync((Payee)null);
            ExtensionFactory.PayeeExtFactory = ext => mockPayeeExt.Object;

            // Act & Assert
            await Assert.ThrowsExceptionAsync<IdNotFoundException>(() =>
                testService.GetSinglePayeeAsync(40)
            , "IdNotFoundException should have been thrown");
        }

        [TestMethod]
        public async Task AddPayeeAsync_adds_payee_then_saves() {
            // Arrange
            var payee = new Payee { Name = "Test" };
            var sequence = new MockSequence();
            mockRepo.InSequence(sequence).Setup(m => m.AddPayee(It.IsAny<Payee>()));
            mockRepo.InSequence(sequence).Setup(m => m.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            await testService.AddPayeeAsync(payee);

            // Assert
            mockRepo.Verify(m => m.AddPayee(payee), Times.Once());
            mockRepo.Verify(m => m.SaveChangesAsync(), Times.Once());
        }

        [TestMethod]
        public async Task RemovePayeeAsync_removes_payee_then_saves() {
            // Arrange
            var payee = new Payee { ID = 1 };
            var payees = new List<Payee> { payee }.AsQueryable();
            var sequence = new MockSequence();
            mockRepo.Setup(m => m.GetPayees(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(payees);
            mockRepo.InSequence(sequence).Setup(m => m.DeletePayee(It.IsAny<Payee>()));
            mockRepo.InSequence(sequence).Setup(m => m.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            await testService.RemovePayeeAsync(1);

            // Assert
            mockRepo.Verify(m => m.DeletePayee(payee), Times.Once());
            mockRepo.Verify(m => m.SaveChangesAsync(), Times.Once());
        }

        [TestMethod]
        public async Task RemovePayeeAsync_skips_delete_if_id_not_present() {
            // Arrange
            var payees = new List<Payee>().AsQueryable();
            mockRepo.Setup(m => m.GetPayees(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(payees);

            // Act
            await testService.RemovePayeeAsync(20);

            // Assert
            mockRepo.Verify(m => m.DeletePayee(It.IsAny<Payee>()), Times.Never());
        }

        [DataTestMethod]
        [DataRow(1, true), DataRow(40, false)]
        public void PayeeExists_tells_if_an_id_exists(int testId, bool expectedResult) {
            // Arrange
            var payees = new List<Payee> {
                new Payee { ID = 1 }
            }.AsQueryable();
            mockRepo.Setup(m => m.GetPayees(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(payees);

            // Act
            var result = testService.PayeeExists(testId);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        public async Task UpdatePayeeAsync_edits_payee_then_saves() {
            // Arrange
            var testID = 1;
            var payee = new Payee { ID = testID };
            var sequence = new MockSequence();
            mockRepo.InSequence(sequence).Setup(m => m.EditPayee(It.IsAny<Payee>()));
            mockRepo.InSequence(sequence).Setup(m => m.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            await testService.UpdatePayeeAsync(testID, payee);

            // Assert
            mockRepo.Verify(m => m.EditPayee(payee), Times.Once());
            mockRepo.Verify(m => m.SaveChangesAsync(), Times.Once());
        }

        [TestMethod]
        public async Task UpdatePayeeAsync_throws_IdMismatchException_when_id_doesnt_match_PayeeID() {
            // Arrange
            var testID = 1;
            var payee = new Payee { ID = testID + 1 };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<IdMismatchException>(() =>
                testService.UpdatePayeeAsync(testID, payee)
            , $"No exception thrown for Id = {testID} and Payee.ID = {payee.ID}");
        }
    }
}