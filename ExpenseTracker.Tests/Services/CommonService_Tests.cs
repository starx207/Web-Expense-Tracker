using ExpenseTracker.Models;
using ExpenseTracker.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Services.Tests
{
    [TestClass]
    public class CommonService_Tests 
    {
        #region Private Members

        private ICommonService _testService;
        private Mock<IBudgetRepo> _mockRepo;

        #endregion // Private Members

        #region Test Initialization

        [TestInitialize]
        public void Initialize_test_objects() {
            _mockRepo = new Mock<IBudgetRepo>();
            _testService = new CommonService(_mockRepo.Object);
        }

        #endregion // Test Initialization

        #region Tests

        #region GetCategories Tests

        [DataTestMethod]
        [DataRow(true), DataRow(false)]
        public void GetCategories_calls_repo_GetCategories(bool onlyCurrentCategories) {
            // Act
            var result = _testService.GetCategories(currentOnly: onlyCurrentCategories);

            // Assert
            _mockRepo.Verify(m => m.GetCategories(), Times.Once());
        }

        [DataTestMethod]
        [DataRow(true), DataRow(false)]
        public void GetCategories_only_gets_current_categories_when_currentOnly_true(bool onlyCurrentCategories) {
            // Arrange
            int expectedCount = 1;
            if (!onlyCurrentCategories) {
                expectedCount = 2;
            }
            var categories = new List<BudgetCategory> {
                new BudgetCategory {
                    Name = "test",
                    EffectiveFrom = DateTime.Parse("1/1/2016")
                },
                new BudgetCategory {
                    Name = "test",
                    EffectiveFrom = DateTime.Parse("1/1/2017")
                }
            }.AsQueryable();
            _mockRepo.Setup(m => m.GetCategories()).Returns(categories);

            // Act
            var results = _testService.GetCategories(currentOnly: onlyCurrentCategories);

            // Assert
            Assert.AreEqual(expectedCount, results.Count(), "The wrong number of categories were returned");
        }

        #endregion // GetCategories Tests

        #region GetPayees Tests

        [DataTestMethod]
        [DataRow(true), DataRow(false)]
        public void GetPayees_calls_repo_GetPayees(bool onlyCurrentPayees) {
            // Act
            var result = _testService.GetPayees(currentOnly: onlyCurrentPayees);

            // Assert
            _mockRepo.Verify(m => m.GetPayees(false, false, false), Times.Once());
        }

        [DataTestMethod]
        [DataRow(true), DataRow(false)]
        public void GetPayees_only_gets_current_payees_when_currentOnly_true(bool onlyCurrentPayees) {
            // Arrange
            int expectedCount = 1;
            if (!onlyCurrentPayees) {
                expectedCount = 2;
            }
            var payees = new List<Payee> {
                new Payee {
                    Name = "test",
                    EffectiveFrom = DateTime.Parse("1/1/2016")
                },
                new Payee {
                    Name = "test",
                    EffectiveFrom = DateTime.Parse("1/1/2017")
                }
            }.AsQueryable();
            _mockRepo.Setup(m => m.GetPayees(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(payees);

            // Act
            var results = _testService.GetPayees(currentOnly: onlyCurrentPayees);

            // Assert
            Assert.AreEqual(expectedCount, results.Count(), "The wrong number of payees were returned");
        }

        #endregion // GetPayees Tests

        #endregion // Tests
    }
}