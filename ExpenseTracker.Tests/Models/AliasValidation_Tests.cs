using ExpenseTracker.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Tests.Models
{
    [TestClass]
    public class AliasValidation_Tests
    {
        #region Private Members

        private Alias _model;

        #endregion // Private Members

        #region Test Initialization

        [TestInitialize]
        public void CreateValidAlias() => _model = new Alias {
            ID = 1,
            Name = "Valid alias name",
            PayeeID = 4
        };

        #endregion // Test Initialization

        #region Tests

        [TestMethod]
        public void NamePropertyRequired() {
            _model.Name = null;

            bool isModelStateValid = ValidateModel(_model);

            Assert.IsFalse(isModelStateValid, "Alias Name should not be nullable");
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
                errorMsg = "Alias name cannot be greater than 100 characters";
            } else if (name.Length == 0) {
                errorMsg = "Alias name cannot be empty";
            } else {
                errorMsg = $"Alias name cannot be '{name}'";
            }

            Assert.IsFalse(isModelStateValid, errorMsg);
        }

        private bool ValidateModel(Alias model) {
            var context = new ValidationContext(model, null, null);
            var results = new List<ValidationResult>();

            return Validator.TryValidateObject(model, context, results, true);
        }

        #endregion // Tests
    }
}