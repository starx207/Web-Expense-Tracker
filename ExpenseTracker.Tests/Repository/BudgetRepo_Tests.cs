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
        private Mock<BudgetContext> mockContext;
        private IBudgetRepo testRepo;
        private List<BudgetCategory> categories;
        private List<Payee> payees;
        private List<Alias> aliases;
        private List<Transaction> transactions;

        [TestInitialize]
        public void Initialize_Test_Objects() {
            mockContext = new Mock<BudgetContext>();
            testRepo = new BudgetRepo(mockContext.Object);
            categories = new List<BudgetCategory>();
            payees = new List<Payee>();
            aliases = new List<Alias>();
            transactions = new List<Transaction>();
        }

        #region Delete Method Tests
            [TestMethod]
            public void DeleteTransaction_calls_EfCore_Remove() {
                // Arrange
                var mockTransactionSet = new Mock<DbSet<Transaction>>();
                mockContext.SetupGet(m => m.Transactions).Returns(mockTransactionSet.Object);
                var testTransaction = new Transaction();

                // Act
                testRepo.DeleteTransaction(testTransaction);

                // Assert
                mockTransactionSet.Verify(m => m.Remove(testTransaction), Times.Once());
            }

            [TestMethod]
            public void DeletePayee_calls_EfCore_Remove() {
                // Arrange
                var mockPayeeSet = new Mock<DbSet<Payee>>();
                mockContext.SetupGet(m => m.Payees).Returns(mockPayeeSet.Object);
                var testPayee = new Payee();

                // Act
                testRepo.DeletePayee(testPayee);

                // Assert
                mockPayeeSet.Verify(m => m.Remove(testPayee), Times.Once());
            }

            [TestMethod]
            public void DeleteAlias_calls_EfCore_Remove() {
                // Arrange
                var mockAliasSet = new Mock<DbSet<Alias>>();
                mockContext.SetupGet(m => m.Aliases).Returns(mockAliasSet.Object);
                var testAlias = new Alias();

                // Act
                testRepo.DeleteAlias(testAlias);

                // Assert
                mockAliasSet.Verify(m => m.Remove(testAlias), Times.Once());
            }

            [TestMethod]
            public void DeleteBudgetCategory_calls_EfCore_Remove() {
                // Arrange
                var mockBudgetCategorySet = new Mock<DbSet<BudgetCategory>>();
                mockContext.SetupGet(m => m.BudgetCategories).Returns(mockBudgetCategorySet.Object);
                var testBudgetCategory = new BudgetCategory();

                // Act
                testRepo.DeleteBudgetCategory(testBudgetCategory);

                // Assert
                mockBudgetCategorySet.Verify(m => m.Remove(testBudgetCategory), Times.Once());
            }
        #endregion

        #region Add Method Tests
            [TestMethod]
            public void AddTransaction_calls_EFCore_Add() {
                // Arrange
                var mockTransactionSet = new Mock<DbSet<Transaction>>();
                mockContext.SetupGet(m => m.Transactions).Returns(mockTransactionSet.Object);
                var testTransaction = new Transaction();

                // Act
                testRepo.AddTransaction(testTransaction);

                // Assert
                mockTransactionSet.Verify(m => m.Add(testTransaction), Times.Once());
            }

            [TestMethod]
            public void AddPayee_calls_EFCore_Add() {
                // Arrange
                var mockPayeeSet = new Mock<DbSet<Payee>>();
                mockContext.SetupGet(m => m.Payees).Returns(mockPayeeSet.Object);
                var testPayee = new Payee();

                // Act
                testRepo.AddPayee(testPayee);

                // Assert
                mockPayeeSet.Verify(m => m.Add(testPayee), Times.Once());
            }

            [TestMethod]
            public void AddBudgetCategory_calls_EFCore_Add() {
                // Arrange
                var mockBudgetCategorySet = new Mock<DbSet<BudgetCategory>>();
                mockContext.SetupGet(m => m.BudgetCategories).Returns(mockBudgetCategorySet.Object);
                var testBudgetCategory = new BudgetCategory();

                // Act
                testRepo.AddBudgetCategory(testBudgetCategory);

                // Assert
                mockBudgetCategorySet.Verify(m => m.Add(testBudgetCategory), Times.Once());
            }

            [TestMethod]
            public void AddAlias_calls_EFCore_Add() {
                // Arrange
                var mockAliasSet = new Mock<DbSet<Alias>>();
                mockContext.SetupGet(m => m.Aliases).Returns(mockAliasSet.Object);
                var testAlias = new Alias();

                // Act
                testRepo.AddAlias(testAlias);

                // Assert
                mockAliasSet.Verify(m => m.Add(testAlias), Times.Once());
            }
        #endregion

        #region Edit Method Tests
            [TestMethod]
            public void EditTransaction_calls_EFCore_Update() {
                // Arrange
                var mockTransactionSet = new Mock<DbSet<Transaction>>();
                mockContext.SetupGet(m => m.Transactions).Returns(mockTransactionSet.Object);
                var testTransaction = new Transaction();

                // Act
                testRepo.EditTransaction(testTransaction);

                // Assert
                mockTransactionSet.Verify(m => m.Update(testTransaction), Times.Once());
            }

            [TestMethod]
            public void EditPayee_calls_EFCore_Update() {
                // Arrange
                var mockPayeeSet = new Mock<DbSet<Payee>>();
                mockContext.SetupGet(m => m.Payees).Returns(mockPayeeSet.Object);
                var testPayee = new Payee();

                // Act
                testRepo.EditPayee(testPayee);

                // Assert
                mockPayeeSet.Verify(m => m.Update(testPayee), Times.Once());
            }

            [TestMethod]
            public void EditBudgetCategory_calls_EFCore_Update() {
                // Arrange
                var mockBudgetCategorySet = new Mock<DbSet<BudgetCategory>>();
                mockContext.SetupGet(m => m.BudgetCategories).Returns(mockBudgetCategorySet.Object);
                var testBudgetCategory = new BudgetCategory();

                // Act
                testRepo.EditBudgetCategory(testBudgetCategory);

                // Assert
                mockBudgetCategorySet.Verify(m => m.Update(testBudgetCategory), Times.Once());
            }

            [TestMethod]
            public void EditAlias_calls_EFCore_Update() {
                // Arrange
                var mockAliasSet = new Mock<DbSet<Alias>>();
                mockContext.SetupGet(m => m.Aliases).Returns(mockAliasSet.Object);
                var testAlias = new Alias();

                // Act
                testRepo.EditAlias(testAlias);

                // Assert
                mockAliasSet.Verify(m => m.Update(testAlias), Times.Once());
            }
        #endregion

        [TestMethod]
        public async Task SaveChangesAsync_calls_EFCore_SaveChangesAsync() {
            // Act
            await testRepo.SaveChangesAsync();

            // Assert
            mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}