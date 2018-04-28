using ExpenseTracker.Exceptions;
using ExpenseTracker.Models;
using ExpenseTracker.Repository;
using ExpenseTracker.Repository.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Services.Tests
{
    [TestClass]
    public class CategoryManagerService_Tests
    {
        private Mock<IBudgetRepo> mockRepo;
        private ICategoryManagerService testService;
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Initialize_test_objects() {
            
            switch (TestContext.TestName) {
                case nameof(AddCategoryAsync_adds_category_then_saves):
                case nameof(RemoveCategoryAsync_removes_category_then_saves):
                case nameof(UpdateCategoryAsync_addsCategory_then_saves_when_EffectiveFrom_moved_later):
                case nameof(UpdateCategoryAsync_editsCategory_then_saves_when_EffectiveFrom_unchanged_or_earlier):
                    mockRepo = new Mock<IBudgetRepo>(MockBehavior.Strict);
                    break;
                default:
                    mockRepo = new Mock<IBudgetRepo>();
                    break;
            }
            testService = new CategoryManagerService(mockRepo.Object);
        }

        [DataTestMethod]
        [DataRow(true), DataRow(false)]
        public void HasCategories_indicates_if_any_categories_exist(bool expectedResult) {
            // Arrange
            var temp = new List<BudgetCategory>();
            if (expectedResult) {
                temp.Add(new BudgetCategory());
            }
            var queryable = temp.AsQueryable();
            mockRepo.Setup(m => m.GetCategories()).Returns(queryable);

            // Act
            var result = testService.HasCategories();

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        public async Task GetSingleCategoryAsync_returns_category() {
            // Arrange
            var category = new BudgetCategory { ID = 3 };
            var mockCategoryExt = new Mock<ICategoryExtMask>();
            mockCategoryExt.Setup(m => m.SingleOrDefaultAsync(It.IsAny<int>())).ReturnsAsync(category);
            ExtensionFactory.CategoryExtFactory = ext => mockCategoryExt.Object;

            // Act
            var result = await testService.GetSingleCategoryAsync(3);

            // Assert
            mockCategoryExt.Verify(m => m.SingleOrDefaultAsync(3), Times.Once());
            Assert.AreEqual(3, result.ID);
        }

        [TestMethod]
        public async Task GetSingleCategoryAsync_throws_NullIdException_when_null_is_passed() {
            // Act & Assert
            await Assert.ThrowsExceptionAsync<NullIdException>(() =>
                testService.GetSingleCategoryAsync(null)
            , "No exception was thrown");
        }

        [TestMethod]
        public async Task GetSingleCategoryAsync_throws_IdNotFoundException_when_category_doesnt_exist() {
            // Arrange
            var mockCategoryExt = new Mock<ICategoryExtMask>();
            mockCategoryExt.Setup(m => m.SingleOrDefaultAsync(It.IsAny<int>())).ReturnsAsync((BudgetCategory)null);
            ExtensionFactory.CategoryExtFactory = ext => mockCategoryExt.Object;

            // Act & Assert
            await Assert.ThrowsExceptionAsync<IdNotFoundException>(() =>
                testService.GetSingleCategoryAsync(40)
            , "IdNotFoundException should have been thrown");
        }

        [TestMethod]
        public async Task AddCategoryAsync_adds_category_then_saves() {
            // Arrange
            var category = new BudgetCategory { Name = "Test" };
            var sequence = new MockSequence();
            mockRepo.Setup(m => m.GetCategories()).Returns(new List<BudgetCategory>().AsQueryable());
            mockRepo.InSequence(sequence).Setup(m => m.AddBudgetCategory(It.IsAny<BudgetCategory>()));
            mockRepo.InSequence(sequence).Setup(m => m.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            await testService.AddCategoryAsync(category);

            // Assert
            mockRepo.Verify(m => m.AddBudgetCategory(category), Times.Once());
            mockRepo.Verify(m => m.SaveChangesAsync(), Times.Once());
        }

        [TestMethod]
        public async Task AddCategoryAsync_throw_UniqueConstraintViolationException_when_duplicate_name() {
            // Arrange
            var categories = new List<BudgetCategory> {
                new BudgetCategory { Name = "test" }
            }.AsQueryable();
            var category = new BudgetCategory { Name = "test" };
            mockRepo.Setup(m => m.GetCategories()).Returns(categories);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<UniqueConstraintViolationException>(() =>
                testService.AddCategoryAsync(category) );
        }

        [TestMethod]
        public async Task RemoveCategoryAsync_removes_category_then_saves() {
            // Arrange
            var category = new BudgetCategory { ID = 1 };
            var categories = new List<BudgetCategory> { category }.AsQueryable();
            var sequence = new MockSequence();
            mockRepo.Setup(m => m.GetCategories()).Returns(categories);
            mockRepo.InSequence(sequence).Setup(m => m.DeleteBudgetCategory(It.IsAny<BudgetCategory>()));
            mockRepo.InSequence(sequence).Setup(m => m.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            await testService.RemoveCategoryAsync(1);

            // Assert
            mockRepo.Verify(m => m.DeleteBudgetCategory(category), Times.Once());
            mockRepo.Verify(m => m.SaveChangesAsync(), Times.Once());
        }

        [TestMethod]
        public async Task RemoveCategoryAsync_skips_delete_if_id_not_present() {
            // Arrange
            var categories = new List<BudgetCategory>().AsQueryable();
            mockRepo.Setup(m => m.GetCategories()).Returns(categories);

            // Act
            await testService.RemoveCategoryAsync(20);

            // Assert
            mockRepo.Verify(m => m.DeleteBudgetCategory(It.IsAny<BudgetCategory>()), Times.Never());
        }

        [DataTestMethod]
        [DataRow(1, true), DataRow(40, false)]
        public void CategoryExists_tells_if_an_id_exists(int testId, bool expectedResult) {
            // Arrange
            var categories = new List<BudgetCategory> {
                new BudgetCategory { ID = 1 }
            }.AsQueryable();
            mockRepo.Setup(m => m.GetCategories()).Returns(categories);

            // Act
            var result = testService.CategoryExists(testId);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        public async Task UpdateCategoryAsync_addsCategory_then_saves_when_EffectiveFrom_moved_later() {
            // Arrange
            var testID = 1;
            var originalCategory = new BudgetCategory { 
                ID = testID,
                EffectiveFrom = DateTime.Parse("1/1/2017") 
            };
            var editedCategory = new BudgetCategory { 
                ID = testID, 
                EffectiveFrom = originalCategory.EffectiveFrom.AddDays(5) 
            };
            var categories = new List<BudgetCategory> { originalCategory }.AsQueryable();
            var sequence = new MockSequence();
            mockRepo.Setup(m => m.GetCategories()).Returns(categories);
            mockRepo.Setup(m => m.GetTransactions(It.IsAny<bool>(), It.IsAny<bool>())).Returns(new List<Transaction>().AsQueryable());
            mockRepo.Setup(m => m.EditPayee(It.IsAny<Payee>()));
            mockRepo.Setup(m => m.EditTransaction(It.IsAny<Transaction>()));
            mockRepo.InSequence(sequence).Setup(m => m.AddBudgetCategory(It.IsAny<BudgetCategory>()));
            mockRepo.InSequence(sequence).Setup(m => m.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await testService.UpdateCategoryAsync(testID, editedCategory);

            // Assert
            mockRepo.Verify(m => m.AddBudgetCategory(It.IsAny<BudgetCategory>()), Times.Once());
            mockRepo.Verify(m => m.SaveChangesAsync(), Times.Once());
        }

        [DataTestMethod]
        [DataRow("2/15/2017", 0)]
        [DataRow("2/1/2017", 1)]
        [DataRow("1/19/2017", 2)]
        [DataRow("1/3/2017", 3)]
        public async Task UpdateCategoryAsync_reassigns_payees_correctly_when_EffectiveFrom_moved_later(string newDateString, int expectedReassignments) {
            // Arrange
            var reassignedPayees = new List<Payee>();
            var payees = new List<Payee> {
                new Payee {
                    ID = 1,
                    BudgetCategoryID = 1,
                    EffectiveFrom = DateTime.Parse("1/5/2017")
                },
                new Payee {
                    ID = 2,
                    BudgetCategoryID = 1,
                    EffectiveFrom = DateTime.Parse("1/30/2017")
                },
                new Payee {
                    ID = 3,
                    BudgetCategoryID = 1,
                    EffectiveFrom = DateTime.Parse("2/10/2017")
                }
            }.AsQueryable();
            var category = new BudgetCategory {
                ID = 1,
                Name = "test",
                EffectiveFrom = DateTime.Parse(newDateString),
                Payees = payees.Where(p => p.BudgetCategoryID == 1).ToList()
            };
            var categories = new List<BudgetCategory> {
                new BudgetCategory {
                    ID = 1,
                    Name = "test",
                    EffectiveFrom = DateTime.Parse("1/1/2017"),
                    Payees = payees.Where(p => p.BudgetCategoryID == 1).ToList()
                }
            }.AsQueryable();
            mockRepo.Setup(m => m.GetCategories()).Returns(categories);
            mockRepo.Setup(m => m.EditPayee(It.IsAny<Payee>())).Callback<Payee>(p => reassignedPayees.Add(p));

            // Act
            var result = await testService.UpdateCategoryAsync(1, category);

            // Assert
            Assert.AreEqual(expectedReassignments, reassignedPayees.Count, "Incorrect number of payees reassigned");
            foreach (var reassignment in reassignedPayees) {
                Assert.AreNotEqual(1, reassignment.BudgetCategoryID, "BudgetCategoryID was not changed");
            }
        }

        [DataTestMethod]
        [DataRow("2/15/2017", 0)]
        [DataRow("2/1/2017", 1)]
        [DataRow("1/19/2017", 2)]
        [DataRow("1/3/2017", 3)]
        public async Task UpdateCategoryAsync_reassigns_transactions_correctly_when_EffectiveFrom_moved_later(string newDateString, int expectedReassignments) {
            // Arrange
            var reassignedTransactions = new List<Transaction>();
            var transactions = new List<Transaction> {
                new Transaction {
                    ID = 1,
                    OverrideCategoryID = 1,
                    Date = DateTime.Parse("1/5/2017")
                },
                new Transaction {
                    ID = 2,
                    OverrideCategoryID = 1,
                    Date = DateTime.Parse("1/30/2017")
                },
                new Transaction {
                    ID = 3,
                    OverrideCategoryID = 1,
                    Date = DateTime.Parse("2/10/2017")
                }
            }.AsQueryable();
            var category = new BudgetCategory {
                ID = 1,
                Name = "test",
                EffectiveFrom = DateTime.Parse(newDateString)
            };
            var categories = new List<BudgetCategory> {
                new BudgetCategory {
                    ID = 1,
                    Name = "test",
                    EffectiveFrom = DateTime.Parse("1/1/2017")
                }
            }.AsQueryable();
            mockRepo.Setup(m => m.GetCategories()).Returns(categories);
            mockRepo.Setup(m => m.GetTransactions(It.IsAny<bool>(), It.IsAny<bool>())).Returns(transactions);
            mockRepo.Setup(m => m.EditTransaction(It.IsAny<Transaction>())).Callback<Transaction>(t => reassignedTransactions.Add(t));

            // Act
            var result = await testService.UpdateCategoryAsync(1, category);

            // Assert
            Assert.AreEqual(expectedReassignments, reassignedTransactions.Count, "Incorrect number of payees reassigned");
            foreach (var reassignment in reassignedTransactions) {
                Assert.AreNotEqual(1, reassignment.OverrideCategoryID, "BudgetCategoryID was not changed");
            }
        }

        [DataTestMethod]
        [DataRow("1/1/2017", "1/1/2017")]
        [DataRow("1/1/2017", "12/1/2016")]
        public async Task UpdateCategoryAsync_editsCategory_then_saves_when_EffectiveFrom_unchanged_or_earlier(string originalDateString, string editedDateString) {
            // Arrange
            var testID = 1;
            var originalCategory = new BudgetCategory { 
                ID = testID,
                EffectiveFrom = DateTime.Parse(originalDateString) 
            };
            var editedCategory = new BudgetCategory { 
                ID = testID, 
                EffectiveFrom = DateTime.Parse(editedDateString)
            };
            var categories = new List<BudgetCategory> { originalCategory }.AsQueryable();
            var sequence = new MockSequence();
            mockRepo.Setup(m => m.GetCategories()).Returns(categories);
            mockRepo.Setup(m => m.GetTransactions(It.IsAny<bool>(), It.IsAny<bool>())).Returns(new List<Transaction>().AsQueryable());
            mockRepo.Setup(m => m.DeleteBudgetCategory(It.IsAny<BudgetCategory>()));
            mockRepo.InSequence(sequence).Setup(m => m.EditBudgetCategory(It.IsAny<BudgetCategory>()));
            mockRepo.InSequence(sequence).Setup(m => m.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await testService.UpdateCategoryAsync(testID, editedCategory);

            // Assert
            mockRepo.Verify(m => m.EditBudgetCategory(editedCategory), Times.Once());
            mockRepo.Verify(m => m.SaveChangesAsync(), Times.Once());
        }

        [DataTestMethod]
        [DataRow("12/30/2016", 0)]
        [DataRow("12/21/2016", 1)]
        [DataRow("12/1/2016", 2)]
        [DataRow("11/3/2016", 3)]
        public async Task UpdateCategoryAsync_reassigns_payees_correctly_when_EffectiveFrom_moved_earlier(string newDateString, int expectedReassignments) {
            // Arrange
            var reassignedPayees = new List<Payee>();
            var payees = new List<Payee> {
                new Payee {
                    ID = 2,
                    BudgetCategoryID = 2,
                    EffectiveFrom = DateTime.Parse("12/15/2016")
                },
                new Payee {
                    ID = 1,
                    BudgetCategoryID = 3,
                    EffectiveFrom = DateTime.Parse("11/25/2016")
                },
                new Payee {
                    ID = 3,
                    BudgetCategoryID = 2,
                    EffectiveFrom = DateTime.Parse("12/25/2016")
                }
            }.AsQueryable();
            var category = new BudgetCategory {
                ID = 1,
                Name = "test",
                EffectiveFrom = DateTime.Parse(newDateString),
                Payees = payees.Where(p => p.BudgetCategoryID == 1).ToList()
            };
            var categories = new List<BudgetCategory> {
                new BudgetCategory {
                    ID = 1,
                    Name = "test",
                    EffectiveFrom = DateTime.Parse("1/1/2017"),
                    Payees = payees.Where(p => p.BudgetCategoryID == 1).ToList()
                },
                new BudgetCategory {
                    ID = 2,
                    Name = "test",
                    EffectiveFrom = DateTime.Parse("12/1/2016"),
                    Payees = payees.Where(p => p.BudgetCategoryID == 2).ToList()
                },
                new BudgetCategory {
                    ID = 3,
                    Name = "test",
                    EffectiveFrom = DateTime.Parse("11/1/2016"),
                    Payees = payees.Where(p => p.BudgetCategoryID == 3).ToList()
                }
            }.AsQueryable();
            mockRepo.Setup(m => m.GetCategories()).Returns(categories);
            mockRepo.Setup(m => m.EditPayee(It.IsAny<Payee>())).Callback<Payee>(p => reassignedPayees.Add(p));

            // Act
            var result = await testService.UpdateCategoryAsync(1, category);

            // Assert
            Assert.AreEqual(expectedReassignments, reassignedPayees.Count, "Incorrect number of payees reassigned");
            foreach (var reassignment in reassignedPayees) {
                Assert.AreEqual(1, reassignment.BudgetCategoryID, "BudgetCategoryID was not changed");
            }
        }

        [DataTestMethod]
        [DataRow("12/30/2016", 0)]
        [DataRow("12/21/2016", 1)]
        [DataRow("12/1/2016", 2)]
        [DataRow("11/3/2016", 3)]
        public async Task UpdateCategoryAsync_reassigns_transactions_correctly_when_EffectiveFrom_moved_earlier(string newDateString, int expectedReassignments) {
            // Arrange
            var reassignedTransactions = new List<Transaction>();
            var transactions = new List<Transaction> {
                new Transaction {
                    ID = 2,
                    OverrideCategoryID = 2,
                    Date = DateTime.Parse("12/15/2016")
                },
                new Transaction {
                    ID = 1,
                    OverrideCategoryID = 3,
                    Date = DateTime.Parse("11/25/2016")
                },
                new Transaction {
                    ID = 3,
                    OverrideCategoryID = 2,
                    Date = DateTime.Parse("12/25/2016")
                }
            }.AsQueryable();
            var category = new BudgetCategory {
                ID = 1,
                Name = "test",
                EffectiveFrom = DateTime.Parse(newDateString)
            };
            var categories = new List<BudgetCategory> {
                new BudgetCategory {
                    ID = 1,
                    Name = "test",
                    EffectiveFrom = DateTime.Parse("1/1/2017")
                },
                new BudgetCategory {
                    ID = 2,
                    Name = "test",
                    EffectiveFrom = DateTime.Parse("12/1/2016")
                },
                new BudgetCategory {
                    ID = 3,
                    Name = "test",
                    EffectiveFrom = DateTime.Parse("11/1/2016")
                }
            }.AsQueryable();
            mockRepo.Setup(m => m.GetCategories()).Returns(categories);
            mockRepo.Setup(m => m.GetTransactions(It.IsAny<bool>(), It.IsAny<bool>())).Returns(transactions);
            mockRepo.Setup(m => m.EditTransaction(It.IsAny<Transaction>())).Callback<Transaction>(t => reassignedTransactions.Add(t));

            // Act
            var result = await testService.UpdateCategoryAsync(1, category);

            // Assert
            Assert.AreEqual(expectedReassignments, reassignedTransactions.Count, "Incorrect number of transactions reassigned");
            foreach (var reassignment in reassignedTransactions) {
                Assert.AreEqual(1, reassignment.OverrideCategoryID, "BudgetCategoryID was not changed");
            }
        }

        [TestMethod]
        public async Task UpdateCategoryAsync_steals_from_correct_category_when_EffectiveFrom_moved_earlier() {
            // Arrange
            var allPayees = new List<Payee> {
                new Payee {
                    ID = 1,
                    Name = "aaa",
                    EffectiveFrom = DateTime.Parse("8/12/2017"),
                    BudgetCategoryID = 3
                },
                new Payee {
                    ID = 2,
                    Name = "bbb",
                    EffectiveFrom = DateTime.Parse("7/12/2017"),
                    BudgetCategoryID = 2
                },
                new Payee {
                    ID = 3,
                    Name = "ccc",
                    EffectiveFrom = DateTime.Parse("8/1/2017"),
                    BudgetCategoryID = 3
                },
                new Payee {
                    ID = 4,
                    Name = "ddd",
                    EffectiveFrom = DateTime.Parse("9/12/2017"),
                    BudgetCategoryID = 3
                }
            }.AsQueryable();
            var editedCategory = new BudgetCategory {
                ID = 1,
                Name = "test",
                EffectiveFrom = DateTime.Parse("8/8/2017"),
                Payees = new List<Payee>()
            };
            var allCategories = new List<BudgetCategory> {
                new BudgetCategory {
                    ID = 1,
                    Name = "test",
                    EffectiveFrom = editedCategory.EffectiveFrom.AddMonths(2),
                    Payees = new List<Payee>()
                },
                new BudgetCategory {
                    ID = 2,
                    Name = "test",
                    EffectiveFrom = DateTime.Parse("3/1/2017"),
                    Payees = allPayees.Where(p => p.BudgetCategoryID == 2).ToList()
                },
                new BudgetCategory {
                    ID = 3,
                    Name = "test",
                    EffectiveFrom = DateTime.Parse("8/1/2017"),
                    Payees = allPayees.Where(p => p.BudgetCategoryID == 3).ToList()
                }
            }.AsQueryable();
            mockRepo.Setup(m => m.GetCategories()).Returns(allCategories);

            // Act
            var result = await testService.UpdateCategoryAsync(editedCategory.ID, editedCategory);

            // Redistribute payees based on what has changed
            allCategories.Single(c => c.ID == 1).Payees = allPayees.Where(p => p.BudgetCategoryID == 1).ToList();
            allCategories.Single(c => c.ID == 2).Payees = allPayees.Where(p => p.BudgetCategoryID == 2).ToList();
            allCategories.Single(c => c.ID == 3).Payees = allPayees.Where(p => p.BudgetCategoryID == 3).ToList();

            // Assert
            Assert.AreEqual(2, allCategories.Single(c => c.ID == 1).Payees.Count);
            Assert.AreEqual(1, allCategories.Single(c => c.ID == 2).Payees.Count);
            Assert.AreEqual(1, allCategories.Single(c => c.ID == 3).Payees.Count);
        }

        [TestMethod]
        public async Task UpdateCategoryAsync_deletes_unneeded_categories_when_EffectiveFrom_moved_earlier() {
            // Arrange
            var deletedCategories = new List<BudgetCategory>();
            var categories = new List<BudgetCategory> {
                new BudgetCategory {
                    ID = 2,
                    Name = "test",
                    EffectiveFrom = DateTime.Parse("12/1/2016")
                },
                new BudgetCategory {
                    ID = 1,
                    Name = "test",
                    EffectiveFrom = DateTime.Parse("1/1/2017")
                },
                new BudgetCategory {
                    ID = 3,
                    Name = "test",
                    EffectiveFrom = DateTime.Parse("11/1/2016")
                }
            }.AsQueryable();
            var editedCategory = new BudgetCategory {
                ID = 1,
                Name = "test",
                EffectiveFrom = DateTime.Parse("11/20/2016")
            };
            mockRepo.Setup(m => m.GetCategories()).Returns(categories);
            mockRepo.Setup(m => m.DeleteBudgetCategory(It.IsAny<BudgetCategory>())).Callback<BudgetCategory>(c => deletedCategories.Add(c));

            // Act
            var result = await testService.UpdateCategoryAsync(1, editedCategory);

            // Assert
            Assert.AreEqual(1, deletedCategories.Count, "Wrong number of categories deleted");
            foreach (var deleted in deletedCategories) {
                Assert.AreEqual(2, deleted.ID, "Only BudgetCategory with ID = 2 should have been deleted");
            }
        } 

        [TestMethod]
        public async Task UpdateCategoryAsync_throws_IdMismatchException_when_id_doesnt_match_CategoryID() {
            // Arrange
            var testID = 1;
            var category = new BudgetCategory { ID = testID + 1 };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<IdMismatchException>(() =>
                testService.UpdateCategoryAsync(testID, category)
            , $"No exception thrown for Id = {testID} and BudgetCategory.ID = {category.ID}");
        }

        [TestMethod]
        public async Task UpdateCategoryAsync_throws_InvalidDateException_when_effectiveFrom_is_future_date() {
            // Arrange
            var testID = 1;
            var futureDate = new DateTime(DateTime.Today.Year + 1, 1, 1);
            var category = new BudgetCategory { ID = testID, EffectiveFrom = futureDate };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidDateExpection>(() =>
                testService.UpdateCategoryAsync(testID, category)
            , $"No exception thrown for effective date = '{futureDate.ToString()}'");
        }
    }
}