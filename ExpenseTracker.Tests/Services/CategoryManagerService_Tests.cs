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
    }
}