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
    public class CategoryManagerService_Tests
    {
        #region Private Members

        private Mock<IBudgetRepo> _mockRepo;
        private ICategoryManagerService _testService;

        #endregion // Private Members

        #region Public Properties

        public TestContext TestContext { get; set; }

        #endregion // Public Properties

        #region Test Initialization

        [TestInitialize]
        public void Initialize_test_objects() {
            
            switch (TestContext.TestName) {
                case nameof(AddCategoryAsync_adds_category_then_saves):
                case nameof(RemoveCategoryAsync_removes_category_then_saves):
                case nameof(UpdateCategoryAsync_addsCategory_then_saves_when_EffectiveFrom_moved_later):
                case nameof(UpdateCategoryAsync_editsCategory_then_saves_when_EffectiveFrom_unchanged_or_earlier):
                    _mockRepo = new Mock<IBudgetRepo>(MockBehavior.Strict);
                    break;
                default:
                    _mockRepo = new Mock<IBudgetRepo>();
                    break;
            }
            _testService = new CategoryManagerService(_mockRepo.Object);
        }

        #endregion // Test Initialization

        #region Tests

        #region HasCategories Tests

        [DataTestMethod]
        [DataRow(true), DataRow(false)]
        public void HasCategories_indicates_if_any_categories_exist(bool expectedResult) {
            // Arrange
            var temp = new List<BudgetCategory>();
            if (expectedResult) {
                temp.Add(new BudgetCategory());
            }
            var queryable = temp.AsQueryable();
            _mockRepo.Setup(m => m.GetCategories()).Returns(queryable);

            // Act
            var result = _testService.HasCategories();

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        #endregion // HasCategories Tests

        #region GetSingleCategoryAsync Tests

        [TestMethod]
        public async Task GetSingleCategoryAsync_returns_category() {
            // Arrange
            var category = new BudgetCategory { ID = 3 };
            var mockCategoryExt = new Mock<CategoryExt>();
            mockCategoryExt.Setup(m => m.SingleOrDefaultAsync(It.IsAny<Expression<Func<BudgetCategory, bool>>>())).ReturnsAsync(category);
            ExtensionFactory.CategoryExtFactory = ext => mockCategoryExt.Object;

            // Act
            var result = await _testService.GetSingleCategoryAsync(3);

            // Assert
            Assert.AreEqual(3, result.ID);
        }

        [TestMethod]
        public async Task GetSingleCategoryAsync_throws_NullIdException_when_null_is_passed() {
            // Act & Assert
            await Assert.ThrowsExceptionAsync<NullIdException>(() =>
                _testService.GetSingleCategoryAsync(null)
            , "No exception was thrown");
        }

        [TestMethod]
        public async Task GetSingleCategoryAsync_throws_IdNotFoundException_when_category_doesnt_exist() {
            // Arrange
            var mockCategoryExt = new Mock<CategoryExt>();
            mockCategoryExt.Setup(m => m.SingleOrDefaultAsync(It.IsAny<Expression<Func<BudgetCategory, bool>>>())).ReturnsAsync((BudgetCategory)null);
            ExtensionFactory.CategoryExtFactory = ext => mockCategoryExt.Object;

            // Act & Assert
            await Assert.ThrowsExceptionAsync<IdNotFoundException>(() =>
                _testService.GetSingleCategoryAsync(40)
            , "IdNotFoundException should have been thrown");
        }

        #endregion // GetSingleCategoryAsync Tests

        #region AddCategoryAsync Tests

        [TestMethod]
        public async Task AddCategoryAsync_adds_category_then_saves() {
            // Arrange
            var category = new BudgetCategory { Name = "Test" };
            var sequence = new MockSequence();
            _mockRepo.Setup(m => m.GetCategories()).Returns(new List<BudgetCategory>().AsQueryable());
            _mockRepo.InSequence(sequence).Setup(m => m.AddBudgetCategory(It.IsAny<BudgetCategory>()));
            _mockRepo.InSequence(sequence).Setup(m => m.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            await _testService.AddCategoryAsync(category);

            // Assert
            _mockRepo.Verify(m => m.AddBudgetCategory(category), Times.Once());
            _mockRepo.Verify(m => m.SaveChangesAsync(), Times.Once());
        }

        [TestMethod]
        public async Task AddCategoryAsync_throw_UniqueConstraintViolationException_when_duplicate_name() {
            // Arrange
            var categories = new List<BudgetCategory> {
                new BudgetCategory { Name = "test" }
            }.AsQueryable();
            var category = new BudgetCategory { Name = "test" };
            _mockRepo.Setup(m => m.GetCategories()).Returns(categories);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<UniqueConstraintViolationException>(() =>
                _testService.AddCategoryAsync(category) );
        }

        #endregion // AddCategoryAsync Tests

        #region RemoveCategoryAsync Tests

        [TestMethod]
        public async Task RemoveCategoryAsync_removes_category_then_saves() {
            // Arrange
            var category = new BudgetCategory { ID = 1 };
            var categories = new List<BudgetCategory> { category }.AsQueryable();
            var sequence = new MockSequence();
            _mockRepo.Setup(m => m.GetCategories()).Returns(categories);
            _mockRepo.InSequence(sequence).Setup(m => m.DeleteBudgetCategory(It.IsAny<BudgetCategory>()));
            _mockRepo.InSequence(sequence).Setup(m => m.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            await _testService.RemoveCategoryAsync(1);

            // Assert
            _mockRepo.Verify(m => m.DeleteBudgetCategory(category), Times.Once());
            _mockRepo.Verify(m => m.SaveChangesAsync(), Times.Once());
        }

        [TestMethod]
        public async Task RemoveCategoryAsync_skips_delete_if_id_not_present() {
            // Arrange
            var categories = new List<BudgetCategory>().AsQueryable();
            _mockRepo.Setup(m => m.GetCategories()).Returns(categories);

            // Act
            await _testService.RemoveCategoryAsync(20);

            // Assert
            _mockRepo.Verify(m => m.DeleteBudgetCategory(It.IsAny<BudgetCategory>()), Times.Never());
        }

        #endregion // RemoveCategoryAsync Tests

        #region CategoryExists Tests

        [DataTestMethod]
        [DataRow(1, true), DataRow(40, false)]
        public void CategoryExists_tells_if_an_id_exists(int testId, bool expectedResult) {
            // Arrange
            var categories = new List<BudgetCategory> {
                new BudgetCategory { ID = 1 }
            }.AsQueryable();
            _mockRepo.Setup(m => m.GetCategories()).Returns(categories);

            // Act
            var result = _testService.CategoryExists(testId);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        #endregion // CategoryExists Tests

        #region UpdateCategoryAsync Tests

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
            _mockRepo.Setup(m => m.GetCategories()).Returns(categories);
            _mockRepo.Setup(m => m.GetTransactions(It.IsAny<bool>(), It.IsAny<bool>())).Returns(new List<Transaction>().AsQueryable());
            _mockRepo.Setup(m => m.EditPayee(It.IsAny<Payee>()));
            _mockRepo.Setup(m => m.EditTransaction(It.IsAny<Transaction>()));
            _mockRepo.InSequence(sequence).Setup(m => m.AddBudgetCategory(It.IsAny<BudgetCategory>()));
            _mockRepo.InSequence(sequence).Setup(m => m.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _testService.UpdateCategoryAsync(testID, 0, editedCategory.EffectiveFrom, BudgetType.Expense);

            // Assert
            _mockRepo.Verify(m => m.AddBudgetCategory(It.IsAny<BudgetCategory>()), Times.Once());
            _mockRepo.Verify(m => m.SaveChangesAsync(), Times.Once());
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
            _mockRepo.Setup(m => m.GetCategories()).Returns(categories);
            _mockRepo.Setup(m => m.EditPayee(It.IsAny<Payee>())).Callback<Payee>(p => reassignedPayees.Add(p));

            // Act
            var result = await _testService.UpdateCategoryAsync(1, 0, category.EffectiveFrom, BudgetType.Expense);

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
            _mockRepo.Setup(m => m.GetCategories()).Returns(categories);
            _mockRepo.Setup(m => m.GetTransactions(It.IsAny<bool>(), It.IsAny<bool>())).Returns(transactions);
            _mockRepo.Setup(m => m.EditTransaction(It.IsAny<Transaction>())).Callback<Transaction>(t => reassignedTransactions.Add(t));

            // Act
            var result = await _testService.UpdateCategoryAsync(1, 0, category.EffectiveFrom, BudgetType.Expense);

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
            _mockRepo.Setup(m => m.GetCategories()).Returns(categories);
            _mockRepo.Setup(m => m.GetTransactions(It.IsAny<bool>(), It.IsAny<bool>())).Returns(new List<Transaction>().AsQueryable());
            _mockRepo.Setup(m => m.DeleteBudgetCategory(It.IsAny<BudgetCategory>()));
            _mockRepo.InSequence(sequence).Setup(m => m.EditBudgetCategory(It.IsAny<BudgetCategory>()));
            _mockRepo.InSequence(sequence).Setup(m => m.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _testService.UpdateCategoryAsync(testID, 0, editedCategory.EffectiveFrom, BudgetType.Expense);

            // Assert
            _mockRepo.Verify(m => m.EditBudgetCategory(It.Is<BudgetCategory>(c => c.ID == testID)), Times.Once());
            _mockRepo.Verify(m => m.SaveChangesAsync(), Times.Once());
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
            _mockRepo.Setup(m => m.GetCategories()).Returns(categories);
            _mockRepo.Setup(m => m.EditPayee(It.IsAny<Payee>())).Callback<Payee>(p => reassignedPayees.Add(p));

            // Act
            var result = await _testService.UpdateCategoryAsync(1, 0, category.EffectiveFrom, BudgetType.Expense);

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
            _mockRepo.Setup(m => m.GetCategories()).Returns(categories);
            _mockRepo.Setup(m => m.GetTransactions(It.IsAny<bool>(), It.IsAny<bool>())).Returns(transactions);
            _mockRepo.Setup(m => m.EditTransaction(It.IsAny<Transaction>())).Callback<Transaction>(t => reassignedTransactions.Add(t));

            // Act
            var result = await _testService.UpdateCategoryAsync(1, 0, category.EffectiveFrom, BudgetType.Expense);

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
            _mockRepo.Setup(m => m.GetCategories()).Returns(allCategories);

            // Act
            var result = await _testService.UpdateCategoryAsync(editedCategory.ID, 0, editedCategory.EffectiveFrom, BudgetType.Expense);

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
            _mockRepo.Setup(m => m.GetCategories()).Returns(categories);
            _mockRepo.Setup(m => m.DeleteBudgetCategory(It.IsAny<BudgetCategory>())).Callback<BudgetCategory>(c => deletedCategories.Add(c));

            // Act
            var result = await _testService.UpdateCategoryAsync(1, 0, editedCategory.EffectiveFrom, BudgetType.Expense);

            // Assert
            Assert.AreEqual(1, deletedCategories.Count, "Wrong number of categories deleted");
            foreach (var deleted in deletedCategories) {
                Assert.AreEqual(2, deleted.ID, "Only BudgetCategory with ID = 2 should have been deleted");
            }
        } 

        [TestMethod]
        public async Task UpdateCategoryAsync_throws_IdNotFoundException_when_id_not_found_in_existing_Categories() {
            // Arrange
            var testID = 1;
            var category = new BudgetCategory { ID = testID + 1 };
            _mockRepo.Setup(m => m.GetCategories()).Returns(new List<BudgetCategory> { category }.AsQueryable());

            // Act & Assert
            await Assert.ThrowsExceptionAsync<IdNotFoundException>(() =>
                _testService.UpdateCategoryAsync(testID, 0, DateTime.Now.AddDays(-1), BudgetType.Expense)
            , $"No exception thrown for Id = {testID} and BudgetCategory.ID = {category.ID}");
        }

        [TestMethod]
        public async Task UpdateCategoryAsync_throws_InvalidDateException_when_effectiveFrom_is_future_date() {
            // Arrange
            var testID = 1;
            var futureDate = new DateTime(DateTime.Today.Year + 1, 1, 1);
            var category = new BudgetCategory { ID = testID, EffectiveFrom = futureDate };
            _mockRepo.Setup(m => m.GetCategories()).Returns(new List<BudgetCategory> { category }.AsQueryable());

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidDateExpection>(() =>
                _testService.UpdateCategoryAsync(testID, 0, category.EffectiveFrom, BudgetType.Expense)
            , $"No exception thrown for effective date = '{futureDate.ToString()}'");
        }

        #endregion // UpdateCategoryAsync Tests

        #endregion // Tests
    }
}