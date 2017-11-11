using ExpenseTracker.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Tests.Models
{
    [TestClass]
    public class BudgetCategoryValidation_Tests
    {
        private BudgetCategory category;

        [TestInitialize]
        public void CreateValidBudgetCategory() {
            category = new BudgetCategory {
                ID = 1,
                Name = "Valid category name",
                Amount = 200,
                Type = BudgetType.Income,
                BeginEffectiveDate = new DateTime(2017, 12, 12),
                EndEffectiveDate = new DateTime(2017, 12, 30)
            };
        }

        [TestMethod]
        public void NamePropertyRequired() {
            category.Name = null;

            bool isModelStateValid = ValidateModel(category);

            Assert.IsFalse(isModelStateValid, "Budget Category Name should not be nullable");
        }

        [DataTestMethod]
        [DataRow("qwertyuiopasdfghjklzxcvbnmqwertyuiopasdfghjklzxcvbnmqwertyuiopqwertyuiopasdfghjklzxcvbnmqwertyuiopasdfghjklzxcvbnmqwertyuiop")]
        [DataRow("")]
        [DataRow("   ")]
        public void NamePropertyValidated(string name) {
            category.Name = name;
            string errorMsg = "";
            bool isModelStateValid = ValidateModel(category);

            if (name.Length > 100) {
                errorMsg = "Budget Category name cannot be greater than 100 characters";
            } else if (name.Length == 0) {
                errorMsg = "Budget Category name cannot be empty";
            } else {
                errorMsg = $"Budget Category name cannot be '{name}'";
            }

            Assert.IsFalse(isModelStateValid, errorMsg);
        }

        private bool ValidateModel(BudgetCategory model) {
            var context = new ValidationContext(model, null, null);
            var results = new List<ValidationResult>();

            return Validator.TryValidateObject(model, context, results, true);
        }
    }
}