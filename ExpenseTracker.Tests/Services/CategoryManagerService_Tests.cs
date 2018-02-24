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
                case nameof(UpdateCategoryAsync_edits_category_then_saves):
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
            mockRepo.InSequence(sequence).Setup(m => m.AddBudgetCategory(It.IsAny<BudgetCategory>()));
            mockRepo.InSequence(sequence).Setup(m => m.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            await testService.AddCategoryAsync(category);

            // Assert
            mockRepo.Verify(m => m.AddBudgetCategory(category), Times.Once());
            mockRepo.Verify(m => m.SaveChangesAsync(), Times.Once());
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
        public async Task UpdateCategoryAsync_edits_category_then_saves() {
            // Arrange
            var testID = 1;
            var category = new BudgetCategory { ID = testID };
            var sequence = new MockSequence();
            mockRepo.Setup(m => m.GetCategories()).Returns(new List<BudgetCategory>().AsQueryable());
            mockRepo.InSequence(sequence).Setup(m => m.EditBudgetCategory(It.IsAny<BudgetCategory>()));
            mockRepo.InSequence(sequence).Setup(m => m.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            await testService.UpdateCategoryAsync(testID, category, DateTime.Now);

            // Assert
            mockRepo.Verify(m => m.EditBudgetCategory(category), Times.Once());
            mockRepo.Verify(m => m.SaveChangesAsync(), Times.Once());
        }
        [TestMethod]
        public async Task UpdateCategoryAsync_sets_Begin_and_End_EffectiveDates() {
            // Arrange
            var categoryToEdit = new BudgetCategory {
                ID = 1,
                BeginEffectiveDate = DateTime.Today.AddDays(-5),
                EndEffectiveDate = DateTime.Today.AddDays(-2)
            };
            var effectiveDate = DateTime.Today;
            var result = new BudgetCategory();
            mockRepo.Setup(m => m.EditBudgetCategory(It.IsAny<BudgetCategory>()))
                .Callback<BudgetCategory>(c => result = c);

            // Act
            await testService.UpdateCategoryAsync(1, categoryToEdit, effectiveDate);

            // Assert
            Assert.AreEqual(effectiveDate, result.BeginEffectiveDate, "BeginEffectiveDate not set correctly");
            Assert.IsNull(result.EndEffectiveDate, "EndEffectiveDate should be null");
        }

        [TestMethod]
        public async Task UpdateCategoryAsync_correctly_splits_BudgetCategory_based_on_date() {
            // Arrange
            var categoryName = "Split Category";
            var testCategories = new List<BudgetCategory> {
                new BudgetCategory {
                    ID = 1,
                    Name = categoryName,
                    BeginEffectiveDate = DateTime.Parse("1/1/2000"),
                    EndEffectiveDate = DateTime.Parse("1/1/2001")
                },
                new BudgetCategory {
                    ID = 2,
                    Name = categoryName,
                    BeginEffectiveDate = DateTime.Parse("1/2/2001"),
                    EndEffectiveDate = DateTime.Parse("1/1/2017")
                },
                new BudgetCategory {
                    ID = 3,
                    Name = categoryName,
                    BeginEffectiveDate = DateTime.Parse("1/2/2017"),
                    EndEffectiveDate = null
                }
            }.AsQueryable();
            var categoryToEdit = testCategories.Single(c => c.ID == 3);
            var effectiveDate = DateTime.Parse("11/1/2016");
            var splitCategory = new BudgetCategory();
            mockRepo.Setup(m => m.GetCategories()).Returns(testCategories);
            mockRepo.Setup(m => m.EditBudgetCategory(It.Is<BudgetCategory>(c => c.ID == 2)))
                .Callback<BudgetCategory>(c => splitCategory = c);

            // Act
            await testService.UpdateCategoryAsync(categoryToEdit.ID, categoryToEdit, effectiveDate);
            // Assert
            Assert.AreEqual(DateTime.Parse("10/31/2016"), splitCategory.EndEffectiveDate, "End Effective Date not set correctly");
        }
        
        [TestMethod]
        public async Task UpdateCategoryAsync_correctly_reassigns_payees() {
            // Arrange
            var categoryName = "Split Category";
            var testCategories = new List<BudgetCategory> {
                new BudgetCategory {
                    ID = 1,
                    Name = categoryName,
                    BeginEffectiveDate = DateTime.Parse("1/1/2000"),
                    EndEffectiveDate = DateTime.Parse("1/1/2001")
                },
                new BudgetCategory {
                    ID = 2,
                    Name = categoryName,
                    BeginEffectiveDate = DateTime.Parse("1/2/2001"),
                    EndEffectiveDate = DateTime.Parse("1/1/2017")
                },
                new BudgetCategory {
                    ID = 3,
                    Name = categoryName,
                    BeginEffectiveDate = DateTime.Parse("1/2/2017"),
                    EndEffectiveDate = null
                }
            }.AsQueryable();
            var categoryToEdit = testCategories.Single(c => c.ID == 3);
            var effectiveDate = DateTime.Parse("5/1/2000");
            var testPayees = new List<Payee> {
                new Payee {
                    ID = 1,
                    Name = "Unchanged Payee",
                    BudgetCategoryID = 1,
                    BeginEffectiveDate = DateTime.Parse("3/1/2000")
                },
                new Payee {
                    ID = 2,
                    Name = "Changed Payee 1",
                    BudgetCategoryID = 1,
                    BeginEffectiveDate = DateTime.Parse("12/1/2000")
                },
                new Payee {
                    ID = 3,
                    Name = "Changed Payee 2",
                    BudgetCategoryID = 2,
                    BeginEffectiveDate = DateTime.Parse("11/5/2016")
                }
            }.AsQueryable();
            var reassignedPayees = new List<Payee>();
            mockRepo.Setup(m => m.GetCategories()).Returns(testCategories);
            mockRepo.Setup(m => m.GetPayees(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(testPayees);
            mockRepo.Setup(m => m.EditPayee(It.IsAny<Payee>()))
                .Callback<Payee>(p => reassignedPayees.Add(p));

            // Act
            await testService.UpdateCategoryAsync(categoryToEdit.ID, categoryToEdit, effectiveDate);

            // Assert
            Assert.AreEqual(2, reassignedPayees.Count);
            foreach (var payee in reassignedPayees) {
                Assert.AreEqual(3, payee.BudgetCategoryID);
            }
        }

        [TestMethod]
        public async Task UpdateCategoryAsync_correctly_reassigns_transactions() {
            // Arrange
            var categoryName = "Split Category";
            var testCategories = new List<BudgetCategory> {
                new BudgetCategory {
                    ID = 1,
                    Name = categoryName,
                    BeginEffectiveDate = DateTime.Parse("1/1/2000"),
                    EndEffectiveDate = DateTime.Parse("1/1/2001")
                },
                new BudgetCategory {
                    ID = 2,
                    Name = categoryName,
                    BeginEffectiveDate = DateTime.Parse("1/2/2001"),
                    EndEffectiveDate = DateTime.Parse("1/1/2017")
                },
                new BudgetCategory {
                    ID = 3,
                    Name = categoryName,
                    BeginEffectiveDate = DateTime.Parse("1/2/2017"),
                    EndEffectiveDate = null
                }
            }.AsQueryable();
            var categoryToEdit = testCategories.Single(c => c.ID == 3);
            var effectiveDate = DateTime.Parse("12/1/2000");
            var testTransactions = new List<Transaction> {
                new Transaction {
                    ID = 1,
                    Amount = 100,
                    Date = DateTime.Parse("10/31/2000"),
                    OverrideCategoryID = 1
                },
                new Transaction {
                    ID = 2,
                    Amount = 200,
                    Date = DateTime.Parse("12/25/2000"),
                    OverrideCategoryID = 1
                },
                new Transaction {
                    ID = 3,
                    Amount = 300,
                    Date = DateTime.Parse("12/25/2010"),
                    OverrideCategoryID = 2
                }
            }.AsQueryable();
            var reassignedTransactions = new List<Transaction>();
            mockRepo.Setup(m => m.GetCategories()).Returns(testCategories);
            mockRepo.Setup(m => m.GetTransactions(It.IsAny<bool>(), It.IsAny<bool>())).Returns(testTransactions);
            mockRepo.Setup(m => m.EditTransaction(It.IsAny<Transaction>()))
                .Callback<Transaction>(t => reassignedTransactions.Add(t));

            // Act
            await testService.UpdateCategoryAsync(categoryToEdit.ID, categoryToEdit, effectiveDate);

            // Assert
            Assert.AreEqual(2, reassignedTransactions.Count);
            foreach (var tran in reassignedTransactions) {
                Assert.AreEqual(3, tran.OverrideCategoryID);
            }
        }

        [TestMethod]
        public async Task UpdateCategoryAsync_throws_IdMismatchException_when_id_doesnt_match_CategoryID() {
            // Arrange
            var testID = 1;
            var category = new BudgetCategory { ID = testID + 1 };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<IdMismatchException>(() =>
                testService.UpdateCategoryAsync(testID, category, DateTime.Now)
            , $"No exception thrown for Id = {testID} and BudgetCategory.ID = {category.ID}");
        }

        [TestMethod]
        public async Task UpdateCategoryAsync_throws_InvalidDateException_when_effectiveDateFrom_is_future_date() {
            // Arrange
            var testID = 1;
            var category = new BudgetCategory { ID = testID };
            var futureDate = new DateTime(DateTime.Today.Year + 1, 1, 1);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidDateExpection>(() =>
                testService.UpdateCategoryAsync(testID, category, futureDate)
            , $"No exception thrown for effective date = '{futureDate.ToString()}'");
        }
    }
}