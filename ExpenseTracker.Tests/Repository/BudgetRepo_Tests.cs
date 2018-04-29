using ExpenseTracker.Data;
using ExpenseTracker.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ExpenseTracker.Repository.Tests
{
    [TestClass]
    public class BudgetRepo_Tests
    {
        #region Private Members

        private Mock<BudgetContext> _mockContext;
        private IBudgetRepo _testRepo;
        private List<BudgetCategory> _categories;
        private List<Payee> _payees;
        private List<Alias> _aliases;
        private List<Transaction> _transactions;

        #endregion // Private Members

        #region Test Initialization

        [TestInitialize]
        public void Initialize_Test_Objects() {
            _mockContext = new Mock<BudgetContext>();
            _testRepo = new BudgetRepo(_mockContext.Object);
            _categories = new List<BudgetCategory>();
            _payees = new List<Payee>();
            _aliases = new List<Alias>();
            _transactions = new List<Transaction>();
        }

        #endregion // Test Initialization

        #region Tests

        #region Delete Tests

        [TestMethod]
        public void DeleteTransaction_calls_EfCore_Remove() {
            // Arrange
            var mockTransactionSet = new Mock<DbSet<Transaction>>();
            _mockContext.SetupGet(m => m.Transactions).Returns(mockTransactionSet.Object);
            var testTransaction = new Transaction();

            // Act
            _testRepo.DeleteTransaction(testTransaction);

            // Assert
            mockTransactionSet.Verify(m => m.Remove(testTransaction), Times.Once());
        }

        [TestMethod]
        public void DeletePayee_calls_EfCore_Remove() {
            // Arrange
            var mockPayeeSet = new Mock<DbSet<Payee>>();
            _mockContext.SetupGet(m => m.Payees).Returns(mockPayeeSet.Object);
            var testPayee = new Payee();

            // Act
            _testRepo.DeletePayee(testPayee);

            // Assert
            mockPayeeSet.Verify(m => m.Remove(testPayee), Times.Once());
        }

        [TestMethod]
        public void DeleteAlias_calls_EfCore_Remove() {
            // Arrange
            var mockAliasSet = new Mock<DbSet<Alias>>();
            _mockContext.SetupGet(m => m.Aliases).Returns(mockAliasSet.Object);
            var testAlias = new Alias();

            // Act
            _testRepo.DeleteAlias(testAlias);

            // Assert
            mockAliasSet.Verify(m => m.Remove(testAlias), Times.Once());
        }

        [TestMethod]
        public void DeleteBudgetCategory_calls_EfCore_Remove() {
            // Arrange
            var mockBudgetCategorySet = new Mock<DbSet<BudgetCategory>>();
            _mockContext.SetupGet(m => m.BudgetCategories).Returns(mockBudgetCategorySet.Object);
            var testBudgetCategory = new BudgetCategory();

            // Act
            _testRepo.DeleteBudgetCategory(testBudgetCategory);

            // Assert
            mockBudgetCategorySet.Verify(m => m.Remove(testBudgetCategory), Times.Once());
        }

        #endregion // Delete Tests

        #region Add Tests

        [TestMethod]
        public void AddTransaction_calls_EFCore_Add() {
            // Arrange
            var mockTransactionSet = new Mock<DbSet<Transaction>>();
            _mockContext.SetupGet(m => m.Transactions).Returns(mockTransactionSet.Object);
            var testTransaction = new Transaction();

            // Act
            _testRepo.AddTransaction(testTransaction);

            // Assert
            mockTransactionSet.Verify(m => m.Add(testTransaction), Times.Once());
        }

        [TestMethod]
        public void AddPayee_calls_EFCore_Add() {
            // Arrange
            var mockPayeeSet = new Mock<DbSet<Payee>>();
            _mockContext.SetupGet(m => m.Payees).Returns(mockPayeeSet.Object);
            var testPayee = new Payee();

            // Act
            _testRepo.AddPayee(testPayee);

            // Assert
            mockPayeeSet.Verify(m => m.Add(testPayee), Times.Once());
        }

        [TestMethod]
        public void AddBudgetCategory_calls_EFCore_Add() {
            // Arrange
            var mockBudgetCategorySet = new Mock<DbSet<BudgetCategory>>();
            _mockContext.SetupGet(m => m.BudgetCategories).Returns(mockBudgetCategorySet.Object);
            var testBudgetCategory = new BudgetCategory();

            // Act
            _testRepo.AddBudgetCategory(testBudgetCategory);

            // Assert
            mockBudgetCategorySet.Verify(m => m.Add(testBudgetCategory), Times.Once());
        }

        [TestMethod]
        public void AddAlias_calls_EFCore_Add() {
            // Arrange
            var mockAliasSet = new Mock<DbSet<Alias>>();
            _mockContext.SetupGet(m => m.Aliases).Returns(mockAliasSet.Object);
            var testAlias = new Alias();

            // Act
            _testRepo.AddAlias(testAlias);

            // Assert
            mockAliasSet.Verify(m => m.Add(testAlias), Times.Once());
        }

        #endregion // Add Tests

        #region Edit Tests

        [TestMethod]
        public void EditTransaction_calls_EFCore_Update() {
            // Arrange
            var mockTransactionSet = new Mock<DbSet<Transaction>>();
            _mockContext.SetupGet(m => m.Transactions).Returns(mockTransactionSet.Object);
            var testTransaction = new Transaction();

            // Act
            _testRepo.EditTransaction(testTransaction);

            // Assert
            mockTransactionSet.Verify(m => m.Update(testTransaction), Times.Once());
        }

        [TestMethod]
        public void EditPayee_calls_EFCore_Update() {
            // Arrange
            var mockPayeeSet = new Mock<DbSet<Payee>>();
            _mockContext.SetupGet(m => m.Payees).Returns(mockPayeeSet.Object);
            var testPayee = new Payee();

            // Act
            _testRepo.EditPayee(testPayee);

            // Assert
            mockPayeeSet.Verify(m => m.Update(testPayee), Times.Once());
        }

        [TestMethod]
        public void EditBudgetCategory_calls_EFCore_Update() {
            // Arrange
            var mockBudgetCategorySet = new Mock<DbSet<BudgetCategory>>();
            _mockContext.SetupGet(m => m.BudgetCategories).Returns(mockBudgetCategorySet.Object);
            var testBudgetCategory = new BudgetCategory();

            // Act
            _testRepo.EditBudgetCategory(testBudgetCategory);

            // Assert
            mockBudgetCategorySet.Verify(m => m.Update(testBudgetCategory), Times.Once());
        }

        [TestMethod]
        public void EditAlias_calls_EFCore_Update() {
            // Arrange
            var mockAliasSet = new Mock<DbSet<Alias>>();
            _mockContext.SetupGet(m => m.Aliases).Returns(mockAliasSet.Object);
            var testAlias = new Alias();

            // Act
            _testRepo.EditAlias(testAlias);

            // Assert
            mockAliasSet.Verify(m => m.Update(testAlias), Times.Once());
        }

        #endregion // Edit Tests

        #region Save Tests

        [TestMethod]
        public async Task SaveChangesAsync_calls_EFCore_SaveChangesAsync() {
            // Act
            await _testRepo.SaveChangesAsync();

            // Assert
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion // Save Tests

        #endregion // Tests
    }
}