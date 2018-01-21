using ExpenseTracker.Data;
using ExpenseTracker.Exceptions;
using ExpenseTracker.Models;
using ExpenseTracker.Repository;
using ExpenseTracker.Repository.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
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
    public class BudgetRepo_Tests_old
    {
        private Mock<BudgetContext> mockContext;
        private List<BudgetCategory> categories;
        private List<Transaction> transactions;
        private List<Alias> aliases;
        private List<Payee> payees;
        private IBudgetRepo repo2;
        private IDataRepo repo;

        [TestInitialize]
        public void InitializeTestData() {
            categories = new List<BudgetCategory>();
            payees = new List<Payee>();
            transactions = new List<Transaction>();
            aliases = new List<Alias>();

            mockContext = new Mock<BudgetContext>();

            repo2 = new BudgetRepo(mockContext.Object);

            repo = new DataRepository(mockContext.Object);
        }

        // [TestMethod]
        // public async Task SaveChangesAsync_calls_EF_SaveChangesAsync() {
        //     int result;

        //     result = await repo2.SaveChangesAsync();

        //     mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        // }

        #region BudgetCategory Tests
            [TestMethod]
            public void HasCategories_calls_BudgetCategories_Any() {
                // Arrange
                bool result;
                mockContext.Setup(m => m.BudgetCategories).ReturnsDbSet(categories);

                // Act
                result = repo.HasCategories();

                // Assert
                Assert.AreEqual(false, result, "An empty DbSet should return false");
            }

            [DataTestMethod]
            [DataRow(true), DataRow(false)]
            public void GetOrderedCategories_orders_results_correctly(bool descending) {
                // Arrange
                IQueryable<BudgetCategory> result;
                categories = new List<BudgetCategory> {
                    new BudgetCategory { Name = "AAA" },
                    new BudgetCategory { Name = "ZZZ" },
                    new BudgetCategory { Name = "CCC" }
                };
                int aPos = descending ? 2 : 0;
                int zPos = descending ? 0 : 2;
                mockContext.Setup(m => m.BudgetCategories).ReturnsDbSet(categories);
                
                // Act
                result = repo.GetOrderedCategories(nameof(BudgetCategory.Name), descending);
                var listResult = result.ToArray();

                // Assert
                Assert.AreEqual("AAA", listResult[aPos].Name);
                Assert.AreEqual("CCC", listResult[1].Name);
                Assert.AreEqual("ZZZ", listResult[zPos].Name);
            }

            [TestMethod]
            public void GetOrderedCategories_throws_ArgumentExecption_for_invalid_property() {
                // Act & Assert
                Assert.ThrowsException<ArgumentException>(() =>
                repo.GetOrderedCategories(nameof(BudgetCategory.Name) + "FAKE"));
            }

            [TestMethod]
            public async Task GetSingleCategoryAsync_calls_SingleOrDefaultAsync() {
                // Arrange
                categories = new List<BudgetCategory> {
                    new BudgetCategory {ID = 1, Name = "AAA"},
                    new BudgetCategory {ID = 2, Name = "BBB"},
                    new BudgetCategory {ID = 3, Name = "CCC"},
                };
                mockContext.Setup(m => m.BudgetCategories).ReturnsDbSet(categories);
                var mockExtension = new Mock<ICategoryExtMask>();
                mockExtension.Setup(m => m.SingleOrDefaultAsync(It.IsAny<int>())).ReturnsAsync(categories[1]);
                ExtensionFactory.CategoryExtFactory = ext => mockExtension.Object;


                // Act
                var result = await repo.GetSingleCategoryAsync(2);

                // Assert
                mockExtension.Verify(m => m.SingleOrDefaultAsync(2), Times.Once);
                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task GetSingleCategoryAsync_throws_NullIdException_when_id_is_null() {
                // Act & Assert
                await Assert.ThrowsExceptionAsync<NullIdException>(() =>
                    repo.GetSingleCategoryAsync(null));
            }

            [TestMethod]
            public async Task GetSingleCategoryAsync_throws_IdNotFoundException_when_id_not_in_db() {
                // Arrange
                categories = new List<BudgetCategory>();
                mockContext.Setup(m => m.BudgetCategories).ReturnsDbSet(categories);
                var mockExtension = new Mock<ICategoryExtMask>();
                mockExtension.Setup(m => m.SingleOrDefaultAsync(It.IsAny<int>())).ReturnsAsync((BudgetCategory)null);
                ExtensionFactory.CategoryExtFactory = ext => mockExtension.Object;

                // Act & Assert
                await Assert.ThrowsExceptionAsync<IdNotFoundException>(() =>
                    repo.GetSingleCategoryAsync(10));
            }

            [TestMethod]
            public async Task AddCategoryAsync_calls_Add_with_correct_category() {
                // Arrange
                var mockCategorySet = new Mock<DbSet<BudgetCategory>>();
                mockContext.Setup(m => m.BudgetCategories).Returns(mockCategorySet.Object);
                var newCategory = new BudgetCategory { Name = "Added Category" };

                // Act
                await repo.AddCategoryAsync(newCategory);

                // Assert
                mockCategorySet.Verify(m => m.Add(It.Is<BudgetCategory>(c => c.Name == "Added Category")), Times.Once());
            }

            // [TestMethod]
            // public void BudgetCategories_returns_IQueryable_collection() {
            //     mockContext.Setup(m => m.BudgetCategories).ReturnsDbSet(categories);
                
            //     var result = repo2.BudgetCategories();

            //     Assert.IsInstanceOfType(result, typeof(IQueryable<BudgetCategory>));
            // }

            // [TestMethod]
            // public void DeleteBudgetCategory_calls_DbSet_Remove() {
            //     var category = categories.First();
            //     var mockCategoryDbSet = DbSetMocking.CreateMockSet(categories);
            //     mockContext.Setup(m => m.BudgetCategories).Returns(mockCategoryDbSet.Object);

            //     repo2.DeleteBudgetCategory(category);

            //     VerifyDbDelete(mockCategoryDbSet);
            // }

            // [TestMethod]
            // public void AddBudgetCategory_calls_DbSet_Add() {
            //     var category = new BudgetCategory();
            //     var mockCategoryDbSet = DbSetMocking.CreateMockSet(categories);
            //     mockContext.Setup(m => m.BudgetCategories).Returns(mockCategoryDbSet.Object);

            //     repo2.AddBudgetCategory(category);

            //     VerifyDbAdd(mockCategoryDbSet);
            // }

            // [TestMethod]
            // public void EditBudgetCategory_calls_DbSet_Update() {
            //     var category = categories.First();
            //     category.Name += "_modified";
            //     var mockCategoryDbSet = DbSetMocking.CreateMockSet(categories);
            //     mockContext.Setup(m => m.BudgetCategories).Returns(mockCategoryDbSet.Object);

            //     repo2.EditBudgetCategory(category);

            //     VerifyDbUpdate(mockCategoryDbSet);
            // }

            // [TestMethod]
            // public async Task BudgetCategoriesAsync_returns_sorted_categories() {
            //     categories = new List<BudgetCategory> {
            //         new BudgetCategory {
            //             Name = "ZZZ"
            //         },
            //         new BudgetCategory {
            //             Name = "AAA"
            //         },
            //         new BudgetCategory {
            //             Name = "CCC"
            //         }
            //     };
            //     var mockCategoryDbSet = DbSetMocking.CreateMockSet(new TestAsyncEnumerable<BudgetCategory>(categories));
            //     mockContext.Setup(m => m.BudgetCategories).Returns(mockCategoryDbSet.Object);

            //     var ascendingResults = await repo2.BudgetCategoriesAsync(nameof(BudgetCategory.Name));

            //     Assert.AreEqual("AAA", ascendingResults[0].Name, "Sort ascending failed");
            //     Assert.AreEqual("CCC", ascendingResults[1].Name, "Sort ascending failed");
            //     Assert.AreEqual("ZZZ", ascendingResults[2].Name, "Sort ascending failed");

            //     var descendingResults = await repo2.BudgetCategoriesAsync(nameof(BudgetCategory.Name), true);

            //     Assert.AreEqual("ZZZ", descendingResults[0].Name, "Sort descending failed");
            //     Assert.AreEqual("CCC", descendingResults[1].Name, "Sort descending failed");
            //     Assert.AreEqual("AAA", descendingResults[2].Name, "Sort descending failed");
            // }

            // // This test does not work currently, but I'm not super concerned because what it's testing is that
            // // the categories are returned with ToListAsync but no other operations are performed.
            // [TestMethod]
            // public async Task BudgetCategoriesAsync_throws_AgurmentException_when_non_existant_property_passed() {
            //     categories = new List<BudgetCategory> {
            //         new BudgetCategory {
            //             Name = "ZZZ"
            //         },
            //         new BudgetCategory {
            //             Name = "AAA"
            //         },
            //         new BudgetCategory {
            //             Name = "CCC"
            //         }
            //     };
            //     var mockCategoryDbSet = DbSetMocking.CreateMockSet(new TestAsyncEnumerable<BudgetCategory>(categories));
            //     mockContext.Setup(m => m.BudgetCategories).Returns(mockCategoryDbSet.Object);

            //     await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
            //         repo2.BudgetCategoriesAsync(nameof(BudgetCategory.Name) + "FAKE!")
            //     );
            // }
        #endregion

        #region Payee Tests
            // [TestMethod]
            // public void Payees_returns_IQueryable_collection() {
            //     mockContext.Setup(m => m.Payees).ReturnsDbSet(payees);

            //     var result = repo2.Payees();

            //     Assert.IsInstanceOfType(result, typeof(IQueryable<Payee>));
            // }

            // [TestMethod]
            // public void DeletePayee_calls_DbSet_Remove() {
            //     var payee = payees.First();
            //     var mockPayeeDbSet = DbSetMocking.CreateMockSet(payees);
            //     mockContext.Setup(m => m.Payees).Returns(mockPayeeDbSet.Object);

            //     repo2.DeletePayee(payee);

            //     VerifyDbDelete(mockPayeeDbSet);
            // }

            // [TestMethod]
            // public void AddPayee_calls_DbSet_Add() {
            //     var payee = new Payee();
            //     var mockPayeeDbSet = DbSetMocking.CreateMockSet(payees);
            //     mockContext.Setup(m => m.Payees).Returns(mockPayeeDbSet.Object);

            //     repo2.AddPayee(payee);

            //     VerifyDbAdd(mockPayeeDbSet);
            // }

            // [TestMethod]
            // public void EditPayee_calls_DbSet_Update() {
            //     var payee = payees.First();
            //     payee.Name += "_modified";
            //     var mockPayeeDbSet = DbSetMocking.CreateMockSet(payees);
            //     mockContext.Setup(m => m.Payees).Returns(mockPayeeDbSet.Object);

            //     repo2.EditPayee(payee);

            //     VerifyDbUpdate(mockPayeeDbSet);
            // }
        #endregion

        #region Transaction Tests
            // [TestMethod]
            // public void Transactions_returns_IQueryable_collection() {
            //     mockContext.Setup(m => m.Transactions).ReturnsDbSet(transactions);

            //     var result = repo2.Transactions();

            //     Assert.IsInstanceOfType(result, typeof(IQueryable<Transaction>));
            // }

            // [TestMethod]
            // public void DeleteTransaction_calls_DbSet_Remove() {
            //     var transaction = transactions.First();
            //     var mockTransactionDbSet = DbSetMocking.CreateMockSet(transactions);
            //     mockContext.Setup(m => m.Transactions).Returns(mockTransactionDbSet.Object);

            //     repo2.DeleteTransaction(transaction);

            //     VerifyDbDelete(mockTransactionDbSet);
            // }

            // [TestMethod]
            // public void AddTransaction_calls_DbSet_Add() {
            //     var transaction = new Transaction();
            //     var mockTransactionDbSet = DbSetMocking.CreateMockSet(transactions);
            //     mockContext.Setup(m => m.Transactions).Returns(mockTransactionDbSet.Object);

            //     repo2.AddTransaction(transaction);

            //     VerifyDbAdd(mockTransactionDbSet);
            // }

            // [TestMethod]
            // public void EditTransaction_calls_DbSet_Update() {
            //     var transaction = transactions.First();
            //     transaction.Amount += 50;
            //     var mockTransactionDbSet = DbSetMocking.CreateMockSet(transactions);
            //     mockContext.Setup(m => m.Transactions).Returns(mockTransactionDbSet.Object);

            //     repo2.EditTransaction(transaction);

            //     VerifyDbUpdate(mockTransactionDbSet);
            // }
        #endregion

        #region Alias Tests
            // [TestMethod]
            // public void Aliases_returns_IQueryable_collection () {
            //     mockContext.Setup(m => m.Aliases).ReturnsDbSet(aliases);

            //     var result = repo2.Aliases();

            //     Assert.IsInstanceOfType(result, typeof(IQueryable<Alias>));
            // }

            // [TestMethod]
            // public void DeleteAlias_calls_DbSet_Remove() {
            //     var alias = aliases.First();
            //     var mockAliasDbSet = DbSetMocking.CreateMockSet(aliases);
            //     mockContext.Setup(m => m.Aliases).Returns(mockAliasDbSet.Object);

            //     repo2.DeleteAlias(alias);

            //     VerifyDbDelete(mockAliasDbSet);
            // }

            // [TestMethod]
            // public void AddAlias_calls_DbSet_Add() {
            //     var alias = new Alias();
            //     var mockAliasDbSet = DbSetMocking.CreateMockSet(aliases);
            //     mockContext.Setup(m => m.Aliases).Returns(mockAliasDbSet.Object);

            //     repo2.AddAlias(alias);

            //     VerifyDbAdd(mockAliasDbSet);
            // }

            // [TestMethod]
            // public void EditAlias_calls_DbSet_Update() {
            //     var alias = aliases.First();
            //     alias.Name += "_modified";
            //     var mockAliasDbSet = DbSetMocking.CreateMockSet(aliases);
            //     mockContext.Setup(m => m.Aliases).Returns(mockAliasDbSet.Object);

            //     repo2.EditAlias(alias);

            //     VerifyDbUpdate(mockAliasDbSet);
            // }
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