using ExpenseTracker.Controllers;
using ExpenseTracker.Exceptions;
using ExpenseTracker.Repository;
using ExpenseTracker.Services;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Tests.Controllers
{
    [TestClass]
    public class PayeeController_Tests : TestCommon
    {
        private IBudgetService budget;
        private Dictionary<int, string> payeeReference;
        private PayeeController controller;
        private readonly string categorySelectListKey = "CategoryList";
        private Mock<IBudgetService> mockBudget;

        [TestInitialize]
        public void InitializeTestData() {
            List<BudgetCategory> categories = TestInitializer.CreateTestCategories();
            List<Payee> payees = TestInitializer.CreateTestPayees(categories.AsQueryable());
            mockBudget = new Mock<IBudgetService>();
            mockBudget.Setup(m => m.GetPayees()).Returns(new TestAsyncEnumerable<Payee>(payees));
            mockBudget.Setup(m => m.GetCategories()).Returns(new TestAsyncEnumerable<BudgetCategory>(categories));

            budget = mockBudget.Object;

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
                AssertThatViewDataIsSelectList(result.ViewData, categorySelectListKey, budget.GetCategories().Select(c => c.ID.ToString()));
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

                mockBudget.Verify(m => m.AddPayeeAsync(It.IsAny<Payee>()), Times.Once());
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

            [TestMethod]
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
                AssertThatViewDataIsSelectList(result.ViewData, 
                    categorySelectListKey, 
                    budget.GetCategories().Select(c => c.ID.ToString()), 
                    newPayee.BudgetCategoryID.ToString());
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

                mockBudget.Verify(m => m.RemovePayeeAsync(It.IsAny<int>()), Times.Once());
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
                Payee testPayee = budget.GetPayees().First();
                IActionResult actionResult = await controller.Edit(testPayee.ID);
                var result = actionResult as ViewResult;

                // Check that ViewData is not null
                AssertThatViewDataIsSelectList(result.ViewData, categorySelectListKey, budget.GetCategories().Select(c => c.ID.ToString()), testPayee.BudgetCategoryID.ToString());
            }

            [TestMethod]
            public async Task EditPOSTWithValidModelState() {
                Payee payeeToEdit = budget.GetPayees().First();
                int testID = payeeToEdit.ID;
                string newName = payeeToEdit.Name + "_modified";
                payeeToEdit.Name = newName;
                IActionResult actionResult = await controller.Edit(payeeToEdit.ID, payeeToEdit);
                var result = actionResult as RedirectToActionResult;

                Assert.IsNotNull(result, "Edit POST did not return a RedirectToActionResult");
                Assert.AreEqual("Index", result.ActionName, $"Edit POST should redirect to 'Index', not {result.ActionName}");

                Payee editedPayee = budget.GetPayees().Where(p => p.ID == testID).First();

                Assert.AreEqual(newName, editedPayee.Name, "The changes for the payee were not saved");
            }

            [TestMethod]
            public async Task EditPOSTWithMismatchedIDReturnsNotFound() {
                mockBudget.Setup(m => m.UpdatePayeeAsync(It.IsAny<int>(), It.IsAny<Payee>())).ThrowsAsync(new IdMismatchException());
                
                Payee editedPayee = budget.GetPayees().First();
                editedPayee.Name += "_modified";
                IActionResult actionResult = await controller.Edit(editedPayee.ID + 1, editedPayee);

                Assert.IsInstanceOfType(actionResult, typeof(NotFoundResult), "If the ID doesn't match the ID of the edited Payee, Not Found should be called");
            }

            [TestMethod]
            public async Task EditPOSTWithInvalidModelStateReturnsToEditView() {
                Payee editedPayee = budget.GetPayees().First();
                string newName = editedPayee.Name + "_modified";
                editedPayee.Name = newName;

                controller.ModelState.AddModelError("test", "test");
                IActionResult actionResult = await controller.Edit(editedPayee.ID, editedPayee);

                var result = actionResult as ViewResult;
                Assert.IsNotNull(result, "Edit POST with invalid model state should return a ViewResult");
                Assert.AreEqual("Edit", result.ViewName, $"Edit POST with invalid model state should return to 'Edit' view, not '{result.ViewName}'");

                Payee model = (Payee)result.Model;

                Assert.AreEqual(editedPayee.ID, model.ID, "The wrong payee was passed back to the View");

                Assert.AreEqual(newName, model.Name, "The updated values were not preserved when returning to the View");
            }

            [TestMethod]
            public async Task EditPOSTWithInvalidModelStatePopulatesCategorySelect() {
                Payee editedPayee = budget.GetPayees().Where(p => p.BudgetCategoryID != null).First();
                BudgetCategory newCategory = budget.GetCategories().Where(c => c.ID != editedPayee.BudgetCategoryID).First();
                editedPayee.Name += "_modified";
                editedPayee.Category = newCategory;
                editedPayee.BudgetCategoryID = newCategory.ID;

                controller.ModelState.AddModelError("test", "test");
                IActionResult actionResult = await controller.Edit(editedPayee.ID, editedPayee);
                var result = actionResult as ViewResult;

                // Check that ViewData is not null
                AssertThatViewDataIsSelectList(result.ViewData, categorySelectListKey, budget.GetCategories().Select(c => c.ID.ToString()), newCategory.ID.ToString());
            }

            // TODO: Figure out how to test the DbUpdateConcurrencyException portion of Edit POST
        #endregion
    }
}