using ExpenseTracker.Controllers;
using ExpenseTracker.Data.Repository;
using ExpenseTracker.Models;
using ExpenseTracker.Tests.Mock;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
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
        private readonly string categorySelectListKey = "CategoryList";

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
            public async Task DetailsGETReturnsCorrectPayee(int id) {
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
            public void CreateGETCreatesSelectListInViewData() {
                IActionResult actionResult = controller.Create();
                var result = actionResult as ViewResult;

                // Check that ViewData is not null
                ViewDataDictionary viewData = result.ViewData;
                Assert.IsNotNull(viewData[categorySelectListKey], $"Create View expects data for ViewData['{categorySelectListKey}']");

                // Check that ViewData is a SelectList with correct number of Items
                SelectList list = (SelectList)viewData[categorySelectListKey];
                Assert.AreEqual(budget.GetCategories().Count(), list.Count(), "SelectList count does not match Category Count");

                // Check that all BudgetCategories are included in the SelectList
                string errorMsg = "The following BudgetCategories are missing: ";
                int missingCategories = 0;
                foreach (var Category in budget.GetCategories()) {
                    if (list.Where(i => i.Value == Category.ID.ToString()).FirstOrDefault() == null) {
                        missingCategories += 1;
                        errorMsg += Category.Name + ", ";
                    }
                }
                errorMsg = errorMsg.Substring(0, errorMsg.Length - 2);

                Assert.AreEqual(0, missingCategories, errorMsg);
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

            public async Task CreatePOSTWithInvalidModelStatePopulatesCategorySelectWithCorrectDefault() {
                int testID = budget.GetPayees().OrderByDescending(p => p.ID).First().ID + 1;
                BudgetCategory testCategory = budget.GetCategories().First();
                Payee newPayee = new Payee {
                    ID = testID,
                    Name = "New Test Payee",
                    BeginEffectiveDate = new DateTime(2017,12,12),
                    Category = testCategory,
                    BudgetCategoryID = testCategory.ID
                };

                controller.ModelState.AddModelError("test", "test");

                IActionResult actionResult = await controller.Create(newPayee);
                var result = actionResult as ViewResult;
                // Check that ViewData is not null
                ViewDataDictionary viewData = result.ViewData;
                Assert.IsNotNull(viewData[categorySelectListKey], $"Create View expects data for ViewData['{categorySelectListKey}']");

                // Check that ViewData is a SelectList with correct number of Items
                SelectList list = (SelectList)viewData[categorySelectListKey];
                Assert.AreEqual(budget.GetCategories().Count(), list.Count(), "SelectList count does not match Category Count");

                // Check that all BudgetCategories are included in the SelectList
                string errorMsg = "The following BudgetCategories are missing: ";
                int missingCategories = 0;
                foreach (var Category in budget.GetCategories()) {
                    if (list.Where(i => i.Value == Category.ID.ToString()).FirstOrDefault() == null) {
                        missingCategories += 1;
                        errorMsg += Category.Name + ", ";
                    }
                }
                errorMsg = errorMsg.Substring(0, errorMsg.Length - 2);

                Assert.AreEqual(0, missingCategories, errorMsg);

                bool isSelected = list.Where(i => i.Value == newPayee.BudgetCategoryID.ToString()).FirstOrDefault().Selected;

                Assert.IsTrue(isSelected, $"The category = '{newPayee.Category.Name}' should be pre-selected for payee = '{newPayee.Name}'");
            }
        #endregion

        #region "Delete Method Tests"
            [TestMethod]
            public async Task DeleteGETReturnsView() {
                int id = budget.GetPayees().Select(p => p.ID).First();

                IActionResult actionResult = await controller.Delete(id);
                var result = actionResult as ViewResult;
                
                Assert.IsNotNull(result);
                Assert.AreEqual("Delete", result.ViewName, $"Delete method returned '{result.ViewName}' instead of 'Delete'");
            }

            [DataTestMethod]
            [DataRow(1), DataRow(2), DataRow(3), DataRow(-1), DataRow(300)]
            public async Task DeleteGETReturnsCorrectPayee(int id) {
                IActionResult actionResult = await controller.Delete(id);

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
            public async Task DeleteGETReturnsNotFoundForNullIndex() {
                IActionResult actionResult = await controller.Delete(null);

                Assert.IsInstanceOfType(actionResult, typeof(NotFoundResult), "A NULL id should result in 404 Not Found");
            }

            [TestMethod]
            public async Task DeletePOSTRemoveExistingPayee() {
                int testID = budget.GetPayees().Select(p => p.ID).First();

                IActionResult actionResult = await controller.DeleteConfirmed(testID);
                var result = actionResult as RedirectToActionResult;

                Assert.AreEqual("Index", result.ActionName, "DeletePOST should redirect to Index");

                Payee payeeShouldntBeThere = budget.GetPayees().Where(p => p.ID == testID).SingleOrDefault();

                Assert.IsNull(payeeShouldntBeThere, $"Payee with id = {testID} wasn't removed");
            }

            [TestMethod]
            public async Task DeletePOSTRemoveNonExistantCategory() {
                int testID = budget.GetPayees().OrderByDescending(p => p.ID).Select(p => p.ID).First() + 10;
                int preCount = budget.GetPayees().Count();

                IActionResult actionResult = await controller.DeleteConfirmed(testID);
                var result = actionResult as RedirectToActionResult;

                Assert.AreEqual("Index", result.ActionName, "DeletePOST should redirect to Index");

                Assert.AreEqual(preCount, budget.GetPayees().Count(), "No payee should have been removed");
            }
        #endregion

        #region Edit Method Tests
            [TestMethod]
            public async Task EditGETReturnsView() {
                int testID = budget.GetPayees().First().ID;

                IActionResult actionResult = await controller.Edit(testID);
                var result = actionResult as ViewResult;

                Assert.IsNotNull(result);
                Assert.AreEqual("Edit", result.ViewName, $"Edit method returned '{result.ViewName}' instead of 'Edit'");
            }

            [DataTestMethod]
            [DataRow(1), DataRow(2), DataRow(3), DataRow(-1), DataRow(300)]
            public async Task EditGETReturnsCorrectPayee(int id) {
                IActionResult actionResult = await controller.Edit(id);

                if (!payeeReference.ContainsKey(id)) {
                    Assert.IsInstanceOfType(actionResult, typeof(NotFoundResult), $"The id ({id}) doesn't exist. 404 Not Found should have been called");
                } else {
                    string payeeName = payeeReference[id];
                    var result = actionResult as ViewResult;
                    Payee model = (Payee)result.Model;

                    Assert.AreEqual(payeeName, model.Name, $"The wrong Payee was returned by for ID = {id}");
                }
            }

            [TestMethod]
            public async Task EditGETReturnsNotFoundForNULLId() {
                IActionResult actionResult = await controller.Edit(null);

                Assert.IsInstanceOfType(actionResult, typeof(NotFoundResult), "404 Not Found should be returned for a NULL id");
            }

            [TestMethod]
            public async Task EditGETPopulatesViewDataWithCategories() {
                int testID = budget.GetPayees().First().ID;
                IActionResult actionResult = await controller.Edit(testID);
                var result = actionResult as ViewResult;

                // Check that ViewData is not null
                ViewDataDictionary viewData = result.ViewData;
                Assert.IsNotNull(viewData[categorySelectListKey], $"Edit View expects data for ViewData['{categorySelectListKey}']");

                // Check that ViewData is a SelectList with correct number of Items
                SelectList list = (SelectList)viewData[categorySelectListKey];
                Assert.AreEqual(budget.GetCategories().Count(), list.Count(), "SelectList count does not match Category Count");

                // Check that all BudgetCategories are included in the SelectList
                string errorMsg = "The following BudgetCategories are missing: ";
                int missingCategories = 0;
                foreach (var Category in budget.GetCategories()) {
                    if (list.Where(i => i.Value == Category.ID.ToString()).FirstOrDefault() == null) {
                        missingCategories += 1;
                        errorMsg += Category.Name + ", ";
                    }
                }
                errorMsg = errorMsg.Substring(0, errorMsg.Length - 2);

                Assert.AreEqual(0, missingCategories, errorMsg);
            }

            [TestMethod]
            public async Task EditGETBudgetCategoriesHasCorrectCategoryPreSelected() {
                Payee testPayee = budget.GetPayees().First();
                IActionResult actionResult = await controller.Edit(testPayee.ID);
                var result = actionResult as ViewResult;
                SelectList list = (SelectList)result.ViewData[categorySelectListKey];

                bool isSelected = list.Where(i => i.Value == testPayee.BudgetCategoryID.ToString()).FirstOrDefault().Selected;

                Assert.IsTrue(isSelected, $"The category = '{testPayee.Category.Name}' should be pre-selected for payee = '{testPayee.Name}'");
            }
        #endregion
    }
}