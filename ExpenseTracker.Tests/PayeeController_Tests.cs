using ExpenseTracker.Controllers;
using ExpenseTracker.Data.Repository;
using ExpenseTracker.Models;
using ExpenseTracker.Tests.Mock;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Tests
{
    [TestClass]
    public class PayeeController_Tests
    {
        private IBudget budget;
        private Dictionary<int, string> payeeReference;
        private PayeeController controller;

        [TestInitialize]
        public void InitializeTestData() {
            List<BudgetCategory> categories = TestInitializer.CreateTestCategories();
            List<Payee> payees = TestInitializer.CreateTestPayees(categories.AsQueryable());
            budget = new MockBudget(new TestAsyncEnumerable<BudgetCategory>(categories), 
                                    new TestAsyncEnumerable<Payee>(payees));

            payeeReference = new Dictionary<int, string>();
            foreach (var payee in budget.GetPayees()) {
                payeeReference.Add(payee.ID, payee.Name);
            }

            controller = new PayeeController(budget);
        }
    }
}