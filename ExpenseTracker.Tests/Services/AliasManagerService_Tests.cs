using ExpenseTracker.Exceptions;
using ExpenseTracker.Models;
using ExpenseTracker.Repository;
using ExpenseTracker.Repository.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Services.Tests
{
    [TestClass]
    public class AliasManagerService_Tests
    {
        private Mock<IBudgetRepo> mockRepo;
        private IAliasManagerService testService;
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Initialize_test_objects() {
            switch (TestContext.TestName) {
                case nameof(UpdateAliasAsync_edits_alias_then_saves):
                case nameof(AddAliasAsync_adds_alias_then_saves):
                    mockRepo = new Mock<IBudgetRepo>(MockBehavior.Strict);
                    break;
                default:
                    mockRepo = new Mock<IBudgetRepo>();
                    break;
            }
            testService = new AliasManagerService(mockRepo.Object);
        }

        [TestMethod]
        public async Task GetSingleAliasAsync_returns_alias() {
            // Arrange
            var alias = new Alias { ID = 3 };
            var mockAliasExt = new Mock<IAliasExtMask>();
            mockAliasExt.Setup(m => m.SingleOrDefaultAsync(It.IsAny<int>())).ReturnsAsync(alias);
            ExtensionFactory.AliasExtFactory = ext => mockAliasExt.Object;

            // Act
            var result = await testService.GetSingleAliasAsync(3);

            // Assert
            mockAliasExt.Verify(m => m.SingleOrDefaultAsync(3), Times.Once());
            Assert.AreEqual(3, result.ID);
        }

        [TestMethod]
        public async Task GetSingleAliasAsync_throws_NullIdException_when_null_is_passed() {
            // Act & Assert
            await Assert.ThrowsExceptionAsync<NullIdException>(() =>
                testService.GetSingleAliasAsync(null)
            , "No exception was thrown");
        }

        [TestMethod]
        public async Task GetSingleAliasAsync_throws_IdNotFoundException_when_alias_doesnt_exist() {
            // Arrange
            var mockAliasExt = new Mock<IAliasExtMask>();
            mockAliasExt.Setup(m => m.SingleOrDefaultAsync(It.IsAny<int>())).ReturnsAsync((Alias)null);
            ExtensionFactory.AliasExtFactory = ext => mockAliasExt.Object;

            // Act & Assert
            await Assert.ThrowsExceptionAsync<IdNotFoundException>(() =>
                testService.GetSingleAliasAsync(40)
            , "IdNotFoundException should have been thrown");
        }

        [TestMethod]
        public async Task UpdateAliasAsync_edits_alias_then_saves() {
            // Arrange
            var testID = 1;
            var alias = new Alias { ID = testID };
            var sequence = new MockSequence();
            mockRepo.InSequence(sequence).Setup(m => m.EditAlias(It.IsAny<Alias>()));
            mockRepo.InSequence(sequence).Setup(m => m.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            await testService.UpdateAliasAsync(testID, alias);

            // Assert
            mockRepo.Verify(m => m.EditAlias(alias), Times.Once());
            mockRepo.Verify(m => m.SaveChangesAsync(), Times.Once());
        }

        [TestMethod]
        public async Task UpdateAliasAsync_throws_IdMismatchException_when_id_doesnt_match_AliasID() {
            // Arrange
            var testID = 1;
            var alias = new Alias { ID = testID + 1 };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<IdMismatchException>(() =>
                testService.UpdateAliasAsync(testID, alias)
            , $"No exception thrown for Id = {testID} and Alias.ID = {alias.ID}");
        }

        [TestMethod]
        public async Task AddAliasAsync_adds_alias_then_saves() {
            // Arrange
            var alias = new Alias { Name = "Test" };
            var sequence = new MockSequence();
            mockRepo.InSequence(sequence).Setup(m => m.AddAlias(It.IsAny<Alias>()));
            mockRepo.InSequence(sequence).Setup(m => m.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            await testService.AddAliasAsync(alias);

            // Assert
            mockRepo.Verify(m => m.AddAlias(alias), Times.Once());
            mockRepo.Verify(m => m.SaveChangesAsync(), Times.Once());
        }

        [TestMethod]
        public async Task RemoveAliasAsync_removes_alias_then_saves() {
            // Arrange
            var alias = new Alias { ID = 1 };
            var aliases = new List<Alias> { alias }.AsQueryable();
            var sequence = new MockSequence();
            mockRepo.Setup(m => m.GetAliases(It.IsAny<bool>())).Returns(aliases);
            mockRepo.InSequence(sequence).Setup(m => m.DeletePayee(It.IsAny<Payee>()));
            mockRepo.InSequence(sequence).Setup(m => m.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            await testService.RemoveAliasAsync(1);

            // Assert
            mockRepo.Verify(m => m.DeleteAlias(alias), Times.Once());
            mockRepo.Verify(m => m.SaveChangesAsync(), Times.Once());
        }

        [TestMethod]
        public async Task RemoveAliasAsync_skips_delete_if_id_not_present() {
            // Arrange
            var aliases = new List<Alias>().AsQueryable();
            mockRepo.Setup(m => m.GetAliases(It.IsAny<bool>())).Returns(aliases);

            // Act
            await testService.RemoveAliasAsync(20);

            // Assert
            mockRepo.Verify(m => m.DeleteAlias(It.IsAny<Alias>()), Times.Never());
        }

        [DataTestMethod]
        [DataRow(1, true), DataRow(40, false)]
        public void AliasExists_tells_if_an_id_exists(int testId, bool expectedResult) {
            // Arrange
            var aliases = new List<Alias> {
                new Alias { ID = 1 }
            }.AsQueryable();
            mockRepo.Setup(m => m.GetAliases(It.IsAny<bool>())).Returns(aliases);

            // Act
            var result = testService.AliasExists(testId);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }
    }
}