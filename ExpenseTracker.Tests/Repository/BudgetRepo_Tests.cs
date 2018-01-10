using ExpenseTracker.Data;
using ExpenseTracker.Models;
using ExpenseTracker.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ExpenseTracker.Tests.Repository
{
    [TestClass]
    public class BudgetRepo_Tests
    {
        private Mock<BudgetContext> mockContext;
        private List<BudgetCategory> categories;
        private List<Transaction> transactions;
        private List<Alias> aliases;
        private List<Payee> payees;
        private IBudgetRepo repo;

        [TestInitialize]
        public void InitializeTestData() {
            categories = TestInitializer.CreateTestCategories();
            payees = TestInitializer.CreateTestPayees(categories.AsQueryable());
            transactions = TestInitializer.CreateTestTransactions(categories.AsQueryable(), payees.AsQueryable());
            aliases = TestInitializer.CreateTestAliases(payees.AsQueryable());

            mockContext = new Mock<BudgetContext>();

            repo = new BudgetRepo(mockContext.Object);
        }

        [TestMethod]
        public async Task SaveChangesAsync_calls_EF_SaveChangesAsync() {
            int result;

            result = await repo.SaveChangesAsync();

            mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        #region BudgetCategory Tests
            [TestMethod]
            public void BudgetCategories_returns_IQueryable_collection() {
                mockContext.Setup(m => m.BudgetCategories).ReturnsDbSet(categories);
                
                var result = repo.BudgetCategories();

                Assert.IsInstanceOfType(result, typeof(IQueryable<BudgetCategory>));
            }

            [TestMethod]
            public void DeleteBudgetCategory_calls_DbSet_Remove() {
                var category = categories.First();
                var mockCategoryDbSet = DbSetMocking.CreateMockSet(categories);
                mockContext.Setup(m => m.BudgetCategories).Returns(mockCategoryDbSet.Object);

                repo.DeleteBudgetCategory(category);

                VerifyDbDelete(mockCategoryDbSet);
            }

            [TestMethod]
            public void AddBudgetCategory_calls_DbSet_Add() {
                var category = new BudgetCategory();
                var mockCategoryDbSet = DbSetMocking.CreateMockSet(categories);
                mockContext.Setup(m => m.BudgetCategories).Returns(mockCategoryDbSet.Object);

                repo.AddBudgetCategory(category);

                VerifyDbAdd(mockCategoryDbSet);
            }

            [TestMethod]
            public void EditBudgetCategory_calls_DbSet_Update() {
                var category = categories.First();
                category.Name += "_modified";
                var mockCategoryDbSet = DbSetMocking.CreateMockSet(categories);
                mockContext.Setup(m => m.BudgetCategories).Returns(mockCategoryDbSet.Object);

                repo.EditBudgetCategory(category);

                VerifyDbUpdate(mockCategoryDbSet);
            }

            [TestMethod]
            public async Task BudgetCategoriesAsync_returns_sorted_categories() {
                categories = new List<BudgetCategory> {
                    new BudgetCategory {
                        Name = "ZZZ"
                    },
                    new BudgetCategory {
                        Name = "AAA"
                    },
                    new BudgetCategory {
                        Name = "CCC"
                    }
                };
                var mockCategoryDbSet = DbSetMocking.CreateMockSet(new TestAsyncEnumerable<BudgetCategory>(categories));
                mockContext.Setup(m => m.BudgetCategories).Returns(mockCategoryDbSet.Object);

                var ascendingResults = await repo.BudgetCategoriesAsync(nameof(BudgetCategory.Name));

                Assert.AreEqual("AAA", ascendingResults[0].Name, "Sort ascending failed");
                Assert.AreEqual("CCC", ascendingResults[1].Name, "Sort ascending failed");
                Assert.AreEqual("ZZZ", ascendingResults[2].Name, "Sort ascending failed");

                var descendingResults = await repo.BudgetCategoriesAsync(nameof(BudgetCategory.Name), true);

                Assert.AreEqual("ZZZ", descendingResults[0].Name, "Sort descending failed");
                Assert.AreEqual("CCC", descendingResults[1].Name, "Sort descending failed");
                Assert.AreEqual("AAA", descendingResults[2].Name, "Sort descending failed");
            }

            // This test does not work currently, but I'm not super concerned because what it's testing is that
            // the categories are returned with ToListAsync but no other operations are performed.
            [TestMethod]
            public async Task BudgetCategoriesAsync_throws_AgurmentException_when_non_existant_property_passed() {
                categories = new List<BudgetCategory> {
                    new BudgetCategory {
                        Name = "ZZZ"
                    },
                    new BudgetCategory {
                        Name = "AAA"
                    },
                    new BudgetCategory {
                        Name = "CCC"
                    }
                };
                var mockCategoryDbSet = DbSetMocking.CreateMockSet(new TestAsyncEnumerable<BudgetCategory>(categories));
                mockContext.Setup(m => m.BudgetCategories).Returns(mockCategoryDbSet.Object);

                await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
                    repo.BudgetCategoriesAsync(nameof(BudgetCategory.Name) + "FAKE!")
                );
            }
        #endregion

        #region Payee Tests
            [TestMethod]
            public void Payees_returns_IQueryable_collection() {
                mockContext.Setup(m => m.Payees).ReturnsDbSet(payees);

                var result = repo.Payees();

                Assert.IsInstanceOfType(result, typeof(IQueryable<Payee>));
            }

            [TestMethod]
            public void DeletePayee_calls_DbSet_Remove() {
                var payee = payees.First();
                var mockPayeeDbSet = DbSetMocking.CreateMockSet(payees);
                mockContext.Setup(m => m.Payees).Returns(mockPayeeDbSet.Object);

                repo.DeletePayee(payee);

                VerifyDbDelete(mockPayeeDbSet);
            }

            [TestMethod]
            public void AddPayee_calls_DbSet_Add() {
                var payee = new Payee();
                var mockPayeeDbSet = DbSetMocking.CreateMockSet(payees);
                mockContext.Setup(m => m.Payees).Returns(mockPayeeDbSet.Object);

                repo.AddPayee(payee);

                VerifyDbAdd(mockPayeeDbSet);
            }

            [TestMethod]
            public void EditPayee_calls_DbSet_Update() {
                var payee = payees.First();
                payee.Name += "_modified";
                var mockPayeeDbSet = DbSetMocking.CreateMockSet(payees);
                mockContext.Setup(m => m.Payees).Returns(mockPayeeDbSet.Object);

                repo.EditPayee(payee);

                VerifyDbUpdate(mockPayeeDbSet);
            }
        #endregion

        #region Transaction Tests
            [TestMethod]
            public void Transactions_returns_IQueryable_collection() {
                mockContext.Setup(m => m.Transactions).ReturnsDbSet(transactions);

                var result = repo.Transactions();

                Assert.IsInstanceOfType(result, typeof(IQueryable<Transaction>));
            }

            [TestMethod]
            public void DeleteTransaction_calls_DbSet_Remove() {
                var transaction = transactions.First();
                var mockTransactionDbSet = DbSetMocking.CreateMockSet(transactions);
                mockContext.Setup(m => m.Transactions).Returns(mockTransactionDbSet.Object);

                repo.DeleteTransaction(transaction);

                VerifyDbDelete(mockTransactionDbSet);
            }

            [TestMethod]
            public void AddTransaction_calls_DbSet_Add() {
                var transaction = new Transaction();
                var mockTransactionDbSet = DbSetMocking.CreateMockSet(transactions);
                mockContext.Setup(m => m.Transactions).Returns(mockTransactionDbSet.Object);

                repo.AddTransaction(transaction);

                VerifyDbAdd(mockTransactionDbSet);
            }

            [TestMethod]
            public void EditTransaction_calls_DbSet_Update() {
                var transaction = transactions.First();
                transaction.Amount += 50;
                var mockTransactionDbSet = DbSetMocking.CreateMockSet(transactions);
                mockContext.Setup(m => m.Transactions).Returns(mockTransactionDbSet.Object);

                repo.EditTransaction(transaction);

                VerifyDbUpdate(mockTransactionDbSet);
            }
        #endregion

        #region Alias Tests
            [TestMethod]
            public void Aliases_returns_IQueryable_collection () {
                mockContext.Setup(m => m.Aliases).ReturnsDbSet(aliases);

                var result = repo.Aliases();

                Assert.IsInstanceOfType(result, typeof(IQueryable<Alias>));
            }

            [TestMethod]
            public void DeleteAlias_calls_DbSet_Remove() {
                var alias = aliases.First();
                var mockAliasDbSet = DbSetMocking.CreateMockSet(aliases);
                mockContext.Setup(m => m.Aliases).Returns(mockAliasDbSet.Object);

                repo.DeleteAlias(alias);

                VerifyDbDelete(mockAliasDbSet);
            }

            [TestMethod]
            public void AddAlias_calls_DbSet_Add() {
                var alias = new Alias();
                var mockAliasDbSet = DbSetMocking.CreateMockSet(aliases);
                mockContext.Setup(m => m.Aliases).Returns(mockAliasDbSet.Object);

                repo.AddAlias(alias);

                VerifyDbAdd(mockAliasDbSet);
            }

            [TestMethod]
            public void EditAlias_calls_DbSet_Update() {
                var alias = aliases.First();
                alias.Name += "_modified";
                var mockAliasDbSet = DbSetMocking.CreateMockSet(aliases);
                mockContext.Setup(m => m.Aliases).Returns(mockAliasDbSet.Object);

                repo.EditAlias(alias);

                VerifyDbUpdate(mockAliasDbSet);
            }
        #endregion

        private void VerifyDbDelete<T>(Mock<DbSet<T>> mockDb) where T : class {
            mockDb.Verify(m => m.Remove(It.IsAny<T>()), Times.Once());
        }

        private void VerifyDbAdd<T>(Mock<DbSet<T>> mockDb) where T : class {
            mockDb.Verify(m => m.Add(It.IsAny<T>()), Times.Once());
        }

        private void VerifyDbUpdate<T>(Mock<DbSet<T>> mockDb) where T : class {
            mockDb.Verify(m => m.Update(It.IsAny<T>()), Times.Once());
        }
    }
}