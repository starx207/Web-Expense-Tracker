using ExpenseTracker.Exceptions;
using ExpenseTracker.Models;
using ExpenseTracker.Repository;
using ExpenseTracker.Repository.Extensions;
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
    public class AliasManagerService_Tests
    {
        #region Private Members

        private Mock<IBudgetRepo> _mockRepo;
        private IAliasManagerService _testService;

        #endregion // Private Members

        #region Public Properties

        public TestContext TestContext { get; set; }

        #endregion // Public Properties

        #region Test Initialization

        [TestInitialize]
        public void Initialize_test_objects() {
            switch (TestContext.TestName) {
                case nameof(UpdateAliasAsync_edits_alias_then_saves):
                case nameof(AddAliasAsync_adds_alias_then_saves):
                    _mockRepo = new Mock<IBudgetRepo>(MockBehavior.Strict);
                    break;
                default:
                    _mockRepo = new Mock<IBudgetRepo>();
                    break;
            }
            _testService = new AliasManagerService(_mockRepo.Object);
        }

        #endregion // Test Initialization

        #region Tests

        #region GetSingleAliasAsync Tests

        [TestMethod]
        public async Task GetSingleAliasAsync_returns_alias() {
            // Arrange
            var alias = new Alias { ID = 3 };
            var mockAliasExt = new Mock<IExtensionMask<Alias>>();
            mockAliasExt.Setup(m => m.SingleOrDefaultAsync(It.IsAny<Expression<Func<Alias, bool>>>())).ReturnsAsync(alias);
            ExtensionFactoryHelpers<Alias>.ExtFactoryOverride = ext => mockAliasExt.Object;

            // Act
            var result = await _testService.GetSingleAliasAsync(3);

            // Assert
            Assert.AreEqual(3, result.ID);
        }

        [TestMethod]
        public async Task GetSingleAliasAsync_throws_NullIdException_when_null_is_passed() {
            // Act & Assert
            await Assert.ThrowsExceptionAsync<NullIdException>(() =>
                _testService.GetSingleAliasAsync(null)
            , "No exception was thrown");
        }

        [TestMethod]
        public async Task GetSingleAliasAsync_throws_IdNotFoundException_when_alias_doesnt_exist() {
            // Arrange
            var mockAliasExt = new Mock<IExtensionMask<Alias>>();
            mockAliasExt.Setup(m => m.SingleOrDefaultAsync(It.IsAny<Expression<Func<Alias, bool>>>())).ReturnsAsync((Alias)null);
            ExtensionFactoryHelpers<Alias>.ExtFactoryOverride = ext => mockAliasExt.Object;

            // Act & Assert
            await Assert.ThrowsExceptionAsync<IdNotFoundException>(() =>
                _testService.GetSingleAliasAsync(40)
            , "IdNotFoundException should have been thrown");
        }

        #endregion // GetSingleAliasAsync Tests

        #region UpdateAliasAsync Tests

        [TestMethod]
        public async Task UpdateAliasAsync_edits_alias_then_saves() {
            // Arrange
            var testID = 1;
            var alias = new Alias { ID = testID };
            var aliases = new List<Alias>().AsQueryable();
            var sequence = new MockSequence();
            _mockRepo.InSequence(sequence).Setup(m => m.GetAliases(It.IsAny<bool>())).Returns(aliases);
            _mockRepo.InSequence(sequence).Setup(m => m.EditAlias(It.IsAny<Alias>()));
            _mockRepo.InSequence(sequence).Setup(m => m.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            await _testService.UpdateAliasAsync(testID, alias);

            // Assert
            _mockRepo.Verify(m => m.EditAlias(alias), Times.Once());
            _mockRepo.Verify(m => m.SaveChangesAsync(), Times.Once());
        }

        [TestMethod]
        public async Task UpdateAliasAsync_throws_IdMismatchException_when_id_doesnt_match_AliasID() {
            // Arrange
            var testID = 1;
            var alias = new Alias { ID = testID + 1 };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<IdMismatchException>(() =>
                _testService.UpdateAliasAsync(testID, alias)
            , $"No exception thrown for Id = {testID} and Alias.ID = {alias.ID}");
        }

        [TestMethod]
        public async Task UpdateAliasAsync_throw_UniqueConstraintViolationException_when_duplicate_name() {
            // Arrange
            var aliases = new List<Alias> {
                new Alias { ID = 1, Name = "test" }
            }.AsQueryable();
            var alias = new Alias { ID = 2, Name = "test" };
            _mockRepo.Setup(m => m.GetAliases(It.IsAny<bool>())).Returns(aliases);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ModelValidationException>(() =>
                _testService.UpdateAliasAsync(2, alias) );
        }

        #endregion // UpdateAliasAsync Tests

        #region AddAliasAsync Tests

        [TestMethod]
        public async Task AddAliasAsync_adds_alias_then_saves() {
            // Arrange
            var alias = new Alias { Name = "Test" };
            var aliases = new List<Alias>().AsQueryable();
            var sequence = new MockSequence();
            _mockRepo.InSequence(sequence).Setup(m => m.GetAliases(It.IsAny<bool>())).Returns(aliases);
            _mockRepo.InSequence(sequence).Setup(m => m.AddAlias(It.IsAny<Alias>()));
            _mockRepo.InSequence(sequence).Setup(m => m.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            await _testService.AddAliasAsync(alias);

            // Assert
            _mockRepo.Verify(m => m.AddAlias(alias), Times.Once());
            _mockRepo.Verify(m => m.SaveChangesAsync(), Times.Once());
        }

        [TestMethod]
        public async Task AddAliasAsync_throw_UniqueConstraintViolationException_when_duplicate_name() {
            // Arrange
            var aliases = new List<Alias> {
                new Alias { Name = "test" }
            }.AsQueryable();
            var alias = new Alias { Name = "test" };
            _mockRepo.Setup(m => m.GetAliases(It.IsAny<bool>())).Returns(aliases);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ModelValidationException>(() =>
                _testService.AddAliasAsync(alias) );
        }

        #endregion // AddAliasAsync Tests

        #region RemoveAliasAsync Tests

        [TestMethod]
        public async Task RemoveAliasAsync_removes_alias_then_saves() {
            // Arrange
            var alias = new Alias { ID = 1 };
            var aliases = new List<Alias> { alias }.AsQueryable();
            var sequence = new MockSequence();
            _mockRepo.Setup(m => m.GetAliases(It.IsAny<bool>())).Returns(aliases);
            _mockRepo.InSequence(sequence).Setup(m => m.DeletePayee(It.IsAny<Payee>()));
            _mockRepo.InSequence(sequence).Setup(m => m.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            await _testService.RemoveAliasAsync(1);

            // Assert
            _mockRepo.Verify(m => m.DeleteAlias(alias), Times.Once());
            _mockRepo.Verify(m => m.SaveChangesAsync(), Times.Once());
        }

        [TestMethod]
        public async Task RemoveAliasAsync_skips_delete_if_id_not_present() {
            // Arrange
            var aliases = new List<Alias>().AsQueryable();
            _mockRepo.Setup(m => m.GetAliases(It.IsAny<bool>())).Returns(aliases);

            // Act
            await _testService.RemoveAliasAsync(20);

            // Assert
            _mockRepo.Verify(m => m.DeleteAlias(It.IsAny<Alias>()), Times.Never());
        }

        #endregion // RemoveAliasAsync Tests

        #region AliasExists Tests

        [DataTestMethod]
        [DataRow(1, true), DataRow(40, false)]
        public void AliasExists_tells_if_an_id_exists(int testId, bool expectedResult) {
            // Arrange
            var aliases = new List<Alias> {
                new Alias { ID = 1 }
            }.AsQueryable();
            _mockRepo.Setup(m => m.GetAliases(It.IsAny<bool>())).Returns(aliases);

            // Act
            var result = _testService.AliasExists(testId);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        #endregion // AliasExists Tests

        #endregion // Tests
    }
}