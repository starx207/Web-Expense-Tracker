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

namespace ExpenseTracker.Tests.Controllers
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



        #region Index Tests
            [TestMethod]
            public async Task IndexGETReturnsView() {
                IActionResult actionResult = await controller.Index();
                var result = actionResult as ViewResult;

                Assert.IsNotNull(result);
                Assert.AreEqual("Index", result.ViewName, $"Index method returned '{result.ViewName}' instead of 'Index'");
            }    
        #endregion


        #region Details Tests
            [TestMethod]
            public async Task DetailsGETReturnsView() {
                int id = budget.GetPayees().First().ID;

                IActionResult actionResult = await controller.Details(id);
                var result = actionResult as ViewResult;
                
                Assert.IsNotNull(result);
                Assert.AreEqual("Details", result.ViewName, $"Details method returned '{result.ViewName}' instead of 'Details'");
            }

            [DataTestMethod]
            [DataRow(1), DataRow(2), DataRow(3), DataRow(-1), DataRow(300)]
            public async Task DetailsGETReturnsCorrectBudgetCategory(int id) {
                IActionResult actionResult = await controller.Details(id);

                if (!payeeReference.ContainsKey(id)) {
                    Assert.IsInstanceOfType(actionResult, typeof(NotFoundResult), $"The id ({id}) doesn't exist. 404 Not Found should have been called");
                } else {
                    string payeeName = payeeReference[id];

                    var result = actionResult as ViewResult;
                    Payee model = (Payee)result.ViewData.Model;

                    Assert.AreEqual(payeeName, model.Name, $"The wrong Payee was returned by for ID = {id}");
                }
            }

            [TestMethod]
            public async Task DetailsGETReturnsNotFoundForNullIndex() {
                IActionResult actionResult = await controller.Details(null);

                Assert.IsInstanceOfType(actionResult, typeof(NotFoundResult), "A NULL id should result in 404 Not Found");
            }
        #endregion

        #region Create Tests
            [TestMethod]
            public void CreateGETReturnsView() {
                IActionResult actionResult = controller.Create();
                var result = actionResult as ViewResult;

                Assert.AreEqual("Create", result.ViewName, $"Create method returned '{result.ViewName}' instead of 'Create'");
            }

            [TestMethod]
            public async Task CreatePOSTWithAValidModelState() {
                int testID = budget.GetPayees().OrderByDescending(p => p.ID).Select(p => p.ID).First() + 1;
                Payee newPayee = new Payee {
                    ID = testID,
                    Name = "New Test Payee",
                    BeginEffectiveDate = new DateTime(2016, 09, 01)
                };

                IActionResult actionResult = await controller.Create(newPayee);
                var result = actionResult as RedirectToActionResult;
                
                Assert.AreEqual("Index", result.ActionName, "Create should redirect to Index after successful create");

                Payee createdPayee = budget.GetPayees().Where(p => p.ID == testID).First();
                Assert.AreEqual(newPayee.Name, createdPayee.Name, "New payee was not properly added");
            }

            [TestMethod]
            public async Task CreatePOSTWithInvalidModelState() {
                int testID = budget.GetPayees().OrderByDescending(p => p.ID).Select(p => p.ID).First() + 1;
                Payee newPayee = new Payee {
                    ID = testID,
                    Name = "New Test Payee",
                    BeginEffectiveDate = new DateTime(2016, 09, 01)
                };

                controller.ModelState.AddModelError("test", "test");

                IActionResult actionResult = await controller.Create(newPayee);
                var viewResult = actionResult as ViewResult;

                Assert.AreEqual("Create", viewResult.ViewName, "Create should return to itself if ModelState is invalid");

                Payee model = (Payee)viewResult.Model;

                Assert.AreEqual(testID, model.ID, "The Payee was not sent back to the view");
            }
        #endregion
    }
}