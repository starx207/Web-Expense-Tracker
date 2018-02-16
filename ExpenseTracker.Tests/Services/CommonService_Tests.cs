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
    public class CommonService_Tests {
        private ICommonService testService;
        private Mock<IBudgetRepo> mockRepo;

        [TestInitialize]
        public void Initialize_test_objects() {
            mockRepo = new Mock<IBudgetRepo>();
            testService = new CommonService(mockRepo.Object);
        }

        [TestMethod]
        public void GetCategories_calls_repo_GetCategories() {
            // Act
            var result = testService.GetCategories();

            // Assert
            mockRepo.Verify(m => m.GetCategories(), Times.Once());
        }

        [TestMethod]
        public void GetPayees_calls_repo_GetPayees() {
            // Act
            var result = testService.GetPayees();

            // Assert
            mockRepo.Verify(m => m.GetPayees(false, false, false), Times.Once());
        }
    }
}