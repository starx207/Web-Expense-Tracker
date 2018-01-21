using ExpenseTracker.Data;
using ExpenseTracker.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;

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
        #endregion
    }
}