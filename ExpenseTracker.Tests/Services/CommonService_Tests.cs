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

        [DataTestMethod]
        [DataRow(true), DataRow(false)]
        public void GetOrderedCategories_orders_by_specified_property(bool descending) {
            // Arrange
            string property = nameof(BudgetCategory.Name);
            var categories = new List<BudgetCategory> {
                new BudgetCategory { Name = "BBB" },
                new BudgetCategory { Name = "CCC" },
                new BudgetCategory { Name = "AAA" }
            }.AsQueryable();
            int posA = descending ? 2 : 0;
            int posC = descending ? 0 : 2;
            mockRepo.Setup(m => m.GetCategories()).Returns(categories);

            // Act
            var result = testService.GetOrderedCategories(property, descending).ToArray();

            // Assert
            Assert.AreEqual("AAA", result[posA].Name, "Categories not ordered correctly");
            Assert.AreEqual("BBB", result[1].Name, "Categories not ordered correctly");
            Assert.AreEqual("CCC", result[posC].Name, "Categories not ordered correctly");
        }

        [TestMethod]
        public void GetOrderedCategories_throws_ArgumentException_when_ordering_by_non_existant_property() {
            // Arrange
            string property = nameof(BudgetCategory.Name) + "FAKE";
            var categories = new List<BudgetCategory>().AsQueryable();
            mockRepo.Setup(m => m.GetCategories()).Returns(categories);

            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() =>
                testService.GetOrderedCategories(property)
            , $"ArgumentException should be thrown when attempting to sort by BudgetCategory.{property}");
        }

        [DataTestMethod]
        [DataRow(true), DataRow(false)]
        public void GetOrderedPayees_orders_by_specified_property(bool descending) {
            // Arrange
            string property = nameof(Payee.Name);
            var payees = new List<Payee> {
                new Payee { Name = "BBB" },
                new Payee { Name = "CCC" },
                new Payee { Name = "AAA" }
            }.AsQueryable();
            int posA = descending ? 2 : 0;
            int posC = descending ? 0 : 2;
            mockRepo.Setup(m => m.GetPayees(It.IsAny<bool>(), It.IsAny<bool>())).Returns(payees);

            // Act
            var result = testService.GetOrderedPayees(property, descending).ToArray();

            // Assert
            Assert.AreEqual("AAA", result[posA].Name, "Payees not ordered correctly");
            Assert.AreEqual("BBB", result[1].Name, "Payees not ordered correctly");
            Assert.AreEqual("CCC", result[posC].Name, "Payees not ordered correctly");
        }

        [TestMethod]
        public void GetOrderedPayees_throws_ArgumentException_when_ordering_by_non_existant_property() {
            // Arrange
            string property = nameof(Payee.Name) + "FAKE";
            var payees = new List<Payee>().AsQueryable();

            mockRepo.Setup(m => m.GetPayees(It.IsAny<bool>(), It.IsAny<bool>())).Returns(payees);
            
            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() =>
                testService.GetOrderedPayees(property)
            , $"ArgumentException should be thrown when attempting to sort by Payee.{property}");
        }
    }
}