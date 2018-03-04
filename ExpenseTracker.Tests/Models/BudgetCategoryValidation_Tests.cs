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
        private BudgetCategory _model;

        [TestInitialize]
        public void CreateValidBudgetCategory() {
            _model = new BudgetCategory {
                ID = 1,
                Name = "Valid category name",
                Amount = 200,
                Type = BudgetType.Income,
                EffectiveFrom = new DateTime(2017, 12, 12)
            };
        }

        [TestMethod]
        public void NamePropertyRequired() {
            _model.Name = null;

            bool isModelStateValid = ValidateModel(_model);

            Assert.IsFalse(isModelStateValid, "Budget Category Name should not be nullable");
        }

        [DataTestMethod]
        [DataRow("qwertyuiopasdfghjklzxcvbnmqwertyuiopasdfghjklzxcvbnmqwertyuiopqwertyuiopasdfghjklzxcvbnmqwertyuiopasdfghjklzxcvbnmqwertyuiop")]
        [DataRow("")]
        [DataRow("   ")]
        public void NamePropertyValidated(string name) {
            _model.Name = name;
            string errorMsg = "";
            bool isModelStateValid = ValidateModel(_model);

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