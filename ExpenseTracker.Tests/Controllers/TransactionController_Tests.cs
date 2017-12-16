using ExpenseTracker.Controllers;
using ExpenseTracker.Repository;
using ExpenseTracker.Models;
using ExpenseTracker.Tests.Mock;
using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Mvc.Rendering;
// using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Tests.Controllers
{
    [TestClass]
    public class TransactionController_Tests : TestCommon
    {
        private IBudget budget;
        private Dictionary<int, double> transactionReference;
        private TransactionController controller;
        private readonly string categorySelectListKey = "CategoryList";
        private readonly string payeeSelectListKey = "PayeeList";

        [TestInitialize]
        public void InitializeTestData() {
            List<BudgetCategory> categories = TestInitializer.CreateTestCategories();
            List<Payee> payees = TestInitializer.CreateTestPayees(categories.AsQueryable());
            List<Transaction> transactions = TestInitializer.CreateTestTransactions(categories.AsQueryable(), payees.AsQueryable());
            budget = new MockBudget(new TestAsyncEnumerable<BudgetCategory>(categories), 
                                    new TestAsyncEnumerable<Payee>(payees),
                                    null, new TestAsyncEnumerable<Transaction>(transactions));

            transactionReference = new Dictionary<int, double>();
            foreach (var transaction in budget.GetTransactions()) {
                transactionReference.Add(transaction.ID, transaction.Amount);
            }

            controller = new TransactionController(budget);
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
                int id = budget.GetTransactions().First().ID;

                IActionResult actionResult = await controller.Details(id);
                var result = actionResult as ViewResult;
                
                Assert.IsNotNull(result);
                Assert.AreEqual("Details", result.ViewName, $"Details method returned '{result.ViewName}' instead of 'Details'");
            }

            [DataTestMethod]
            [DataRow(1), DataRow(2), DataRow(3), DataRow(-1), DataRow(300)]
            public async Task DetailsGETReturnsCorrectPayee(int id) {
                IActionResult actionResult = await controller.Details(id);

                if (!transactionReference.ContainsKey(id)) {
                    Assert.IsInstanceOfType(actionResult, typeof(NotFoundResult), $"The id ({id}) doesn't exist. 404 Not Found should have been called");
                } else {
                    double transAmount = transactionReference[id];

                    var result = actionResult as ViewResult;
                    Transaction model = (Transaction)result.ViewData.Model;

                    Assert.AreEqual(transAmount, model.Amount, $"The wrong Payee was returned by for ID = {id}");
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
                AssertThatViewDataIsSelectList(result.ViewData, payeeSelectListKey, budget.GetPayees().Select(c => c.ID.ToString()));
            }

            [TestMethod]
            public async Task CreatePOSTWithAValidModelState() {
                int testID = budget.GetTransactions().OrderByDescending(p => p.ID).Select(p => p.ID).First() + 1;
                Transaction newTransaction = new Transaction {
                    ID = testID,
                    Date = DateTime.Parse("10/16/2016"),
                    Amount = 23.12
                };

                IActionResult actionResult = await controller.Create(newTransaction);
                var result = actionResult as RedirectToActionResult;
                
                Assert.AreEqual("Index", result.ActionName, "Create should redirect to Index after successful create");

                Transaction createdTrans = budget.GetTransactions().Where(p => p.ID == testID).First();
                Assert.AreEqual(newTransaction.Amount, createdTrans.Amount, "New transaction was not properly added");
            }

            [TestMethod]
            public async Task CreatePOSTWithInvalidModelState() {
                int testID = budget.GetTransactions().OrderByDescending(p => p.ID).Select(p => p.ID).First() + 1;
                Transaction newTransaction = new Transaction {
                    ID = testID,
                    Date = DateTime.Parse("10/16/2016"),
                    Amount = 23.12
                };

                controller.ModelState.AddModelError("test", "test");

                IActionResult actionResult = await controller.Create(newTransaction);
                var viewResult = actionResult as ViewResult;

                Assert.AreEqual("Create", viewResult.ViewName, "Create should return to itself if ModelState is invalid");

                Transaction model = (Transaction)viewResult.Model;

                Assert.AreEqual(testID, model.ID, "The Transaction was not sent back to the view");
            }

            [TestMethod]
            public async Task CreatePOSTWithInvalidModelStatePopulatesSelectListsWithCorrectDefaults() {
                int testID = budget.GetTransactions().OrderByDescending(p => p.ID).First().ID + 1;
                BudgetCategory testCategory = budget.GetCategories().First();
                Payee testPayee = budget.GetPayees().Where(x => x.BudgetCategoryID != testCategory.ID).First();
                Transaction newTransaction = new Transaction {
                    ID = testID,
                    Date = DateTime.Parse("10/16/2017"),
                    Amount = 2001.2,
                    OverrideCategory = testCategory,
                    OverrideCategoryID = testCategory.ID,
                    PayableTo = testPayee,
                    PayeeID = testPayee.ID
                };

                controller.ModelState.AddModelError("test", "test");

                IActionResult actionResult = await controller.Create(newTransaction);
                var result = actionResult as ViewResult;

                AssertThatViewDataIsSelectList(result.ViewData, categorySelectListKey, budget.GetCategories().Select(x => x.ID.ToString()), testCategory.ID.ToString());
                AssertThatViewDataIsSelectList(result.ViewData, payeeSelectListKey, budget.GetPayees().Select(x => x.ID.ToString()), testPayee.ID.ToString());
            }
        #endregion

        #region "Delete Method Tests"
            [TestMethod]
            public async Task DeleteGETReturnsView() {
                int id = budget.GetTransactions().Select(p => p.ID).First();

                IActionResult actionResult = await controller.Delete(id);
                var result = actionResult as ViewResult;
                
                Assert.IsNotNull(result);
                Assert.AreEqual("Delete", result.ViewName, $"Delete method returned '{result.ViewName}' instead of 'Delete'");
            }

            [DataTestMethod]
            [DataRow(1), DataRow(2), DataRow(3), DataRow(-1), DataRow(300)]
            public async Task DeleteGETReturnsCorrectTransaction(int id) {
                IActionResult actionResult = await controller.Delete(id);

                if (!transactionReference.ContainsKey(id)) {
                    Assert.IsInstanceOfType(actionResult, typeof(NotFoundResult), $"The id ({id}) doesn't exist. 404 Not Found should have been called");
                } else {
                    double transactionAmount = transactionReference[id];

                    var result = actionResult as ViewResult;
                    Transaction model = (Transaction)result.ViewData.Model;

                    Assert.AreEqual(transactionAmount, model.Amount, $"The wrong Transaction was returned by for ID = {id}");
                }
            }

            [TestMethod]
            public async Task DeleteGETReturnsNotFoundForNullIndex() {
                IActionResult actionResult = await controller.Delete(null);

                Assert.IsInstanceOfType(actionResult, typeof(NotFoundResult), "A NULL id should result in 404 Not Found");
            }

            [TestMethod]
            public async Task DeletePOSTRemoveExistingTransaction() {
                int testID = budget.GetTransactions().Select(p => p.ID).First();

                IActionResult actionResult = await controller.DeleteConfirmed(testID);
                var result = actionResult as RedirectToActionResult;

                Assert.AreEqual("Index", result.ActionName, "DeletePOST should redirect to Index");

                Transaction transShouldntBeThere = budget.GetTransactions().Where(p => p.ID == testID).SingleOrDefault();

                Assert.IsNull(transShouldntBeThere, $"Transaction with id = {testID} wasn't removed");
            }

            [TestMethod]
            public async Task DeletePOSTRemoveNonExistantTransaction() {
                int testID = budget.GetTransactions().OrderByDescending(p => p.ID).Select(p => p.ID).First() + 10;
                int preCount = budget.GetTransactions().Count();

                IActionResult actionResult = await controller.DeleteConfirmed(testID);
                var result = actionResult as RedirectToActionResult;

                Assert.AreEqual("Index", result.ActionName, "DeletePOST should redirect to Index");

                Assert.AreEqual(preCount, budget.GetTransactions().Count(), "No transaction should have been removed");
            }
        #endregion

        #region Edit Method Tests
            [TestMethod]
            public async Task EditGETReturnsView() {
                int testID = budget.GetTransactions().First().ID;

                IActionResult actionResult = await controller.Edit(testID);
                var result = actionResult as ViewResult;

                Assert.IsNotNull(result);
                Assert.AreEqual("Edit", result.ViewName, $"Edit method returned '{result.ViewName}' instead of 'Edit'");
            }

            [DataTestMethod]
            [DataRow(1), DataRow(2), DataRow(3), DataRow(-1), DataRow(300)]
            public async Task EditGETReturnsCorrectTransaction(int id) {
                IActionResult actionResult = await controller.Edit(id);

                if (!transactionReference.ContainsKey(id)) {
                    Assert.IsInstanceOfType(actionResult, typeof(NotFoundResult), $"The id ({id}) doesn't exist. 404 Not Found should have been called");
                } else {
                    double transAmount = transactionReference[id];
                    var result = actionResult as ViewResult;
                    Transaction model = (Transaction)result.Model;

                    Assert.AreEqual(transAmount, model.Amount, $"The wrong Transaction was returned by for ID = {id}");
                }
            }

            [TestMethod]
            public async Task EditGETReturnsNotFoundForNULLId() {
                IActionResult actionResult = await controller.Edit(null);

                Assert.IsInstanceOfType(actionResult, typeof(NotFoundResult), "404 Not Found should be returned for a NULL id");
            }

            [TestMethod]
            public async Task EditGETPopulatesViewDataSelectLists() {
                Transaction testTransaction = budget.GetTransactions().Where(t => t.OverrideCategoryID != null && t.PayeeID != null).First();
                IActionResult actionResult = await controller.Edit(testTransaction.ID);
                var result = actionResult as ViewResult;

                AssertThatViewDataIsSelectList(result.ViewData, categorySelectListKey, budget.GetCategories().Select(c => c.ID.ToString()), testTransaction.OverrideCategoryID.ToString());
                AssertThatViewDataIsSelectList(result.ViewData, payeeSelectListKey, budget.GetPayees().Select(p => p.ID.ToString()), testTransaction.PayeeID.ToString());
            }

            [TestMethod]
            public async Task EditPOSTWithValidModelState() {
                Transaction transToEdit = budget.GetTransactions().First();
                int testID = transToEdit.ID;
                double newAmount = transToEdit.Amount + 50000;
                transToEdit.Amount = newAmount;
                IActionResult actionResult = await controller.Edit(transToEdit.ID, transToEdit);
                var result = actionResult as RedirectToActionResult;

                Assert.IsNotNull(result, "Edit POST did not return a RedirectToActionResult");
                Assert.AreEqual("Index", result.ActionName, $"Edit POST should redirect to 'Index', not {result.ActionName}");

                Transaction editedTrans = budget.GetTransactions().Where(p => p.ID == testID).First();

                Assert.AreEqual(newAmount, editedTrans.Amount, "The changes for the transaction were not saved");
            }

            [TestMethod]
            public async Task EditPOSTWithMismatchedIDReturnsNotFound() {
                Transaction editedTrans = budget.GetTransactions().First();
                editedTrans.Amount += 50000;
                IActionResult actionResult = await controller.Edit(editedTrans.ID + 1, editedTrans);

                Assert.IsInstanceOfType(actionResult, typeof(NotFoundResult), "If the ID doesn't match the ID of the edited Transaction, Not Found should be called");
            }

            [TestMethod]
            public async Task EditPOSTWithInvalidModelStateReturnsToEditView() {
                Transaction editedTrans = budget.GetTransactions().First();
                double newAmt = editedTrans.Amount + 500000;
                editedTrans.Amount = newAmt;

                controller.ModelState.AddModelError("test", "test");
                IActionResult actionResult = await controller.Edit(editedTrans.ID, editedTrans);

                var result = actionResult as ViewResult;
                Assert.IsNotNull(result, "Edit POST with invalid model state should return a ViewResult");
                Assert.AreEqual("Edit", result.ViewName, $"Edit POST with invalid model state should return to 'Edit' view, not '{result.ViewName}'");

                Transaction model = (Transaction)result.Model;

                Assert.AreEqual(editedTrans.ID, model.ID, "The wrong transaction was passed back to the View");

                Assert.AreEqual(newAmt, model.Amount, "The updated values were not preserved when returning to the View");
            }

            [TestMethod]
            public async Task EditPOSTWithInvalidModelStatePopulatesSelectLists() {
                Transaction editedTrans = budget.GetTransactions().Where(t => t.OverrideCategoryID != null && t.PayeeID != null).First();
                editedTrans.Amount += 50000;

                controller.ModelState.AddModelError("test", "test");
                IActionResult actionResult = await controller.Edit(editedTrans.ID, editedTrans);
                var result = actionResult as ViewResult;

                AssertThatViewDataIsSelectList(result.ViewData, categorySelectListKey, budget.GetCategories().Select(x => x.ID.ToString()), editedTrans.OverrideCategoryID.ToString());
                AssertThatViewDataIsSelectList(result.ViewData, payeeSelectListKey, budget.GetPayees().Select(x => x.ID.ToString()), editedTrans.PayeeID.ToString());
            }

            // TODO: Figure out how to test the DbUpdateConcurrencyException portion of Edit POST
        #endregion
    }
}