// using ExpenseTracker.Controllers;
// using ExpenseTracker.Services;
// using ExpenseTracker.Models;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.VisualStudio.TestTools.UnitTesting;
// using Moq;
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;

// namespace ExpenseTracker.Tests.Controllers
// {
//     [TestClass]
//     public class AliasController_Tests : TestCommon
//     {
//         private IBudgetService budget;
//         private Dictionary<int, string> aliasReference;
//         private AliasController controller;
//         private readonly string payeeSelectListKey = "PayeeList";

//         private Mock<IBudgetService> mockBudget;

//         [TestInitialize]
//         public void InitializeTestData() {
//             List<BudgetCategory> categories = TestInitializer.CreateTestCategories();
//             List<Payee> payees = TestInitializer.CreateTestPayees(categories.AsQueryable());
//             List<Alias> aliases = TestInitializer.CreateTestAliases(payees.AsQueryable());
//             mockBudget = new Mock<IBudgetService>();
//             mockBudget.Setup(m => m.AddAlias(It.IsAny<Alias>()));
//             mockBudget.Setup(m => m.GetAliases()).Returns(new TestAsyncEnumerable<Alias>(aliases));
//             mockBudget.Setup(m => m.GetPayees()).Returns(payees.AsQueryable());
//             budget = mockBudget.Object;

//             aliasReference = new Dictionary<int, string>();
//             foreach (var alias in budget.GetAliases()) {
//                 aliasReference.Add(alias.ID, alias.Name);
//             }

//             controller = new AliasController(budget);
//         }

//         #region Create Tests
//             [TestMethod]
//             public void CreateGETReturnsView() {
//                 IActionResult actionResult = controller.Create();
//                 var result = actionResult as ViewResult;

//                 Assert.AreEqual("Create", result.ViewName, $"Create method returned '{result.ViewName}' instead of 'Create'");
//             }

//             [TestMethod]
//             public void CreateGETCreatesSelectListInViewData() {
//                 IActionResult actionResult = controller.Create();
//                 var result = actionResult as ViewResult;

//                 // Check that ViewData is not null
//                 AssertThatViewDataIsSelectList(result.ViewData, payeeSelectListKey, budget.GetPayees().Select(i => i.ID.ToString()));
//             }

//             [TestMethod]
//             public async Task CreatePOSTWithAValidModelState() {
//                 int testID = budget.GetAliases().OrderByDescending(p => p.ID).Select(p => p.ID).First() + 1;
//                 Payee assignedPayee = budget.GetPayees().First();
//                 Alias newAlias = new Alias {
//                     ID = testID,
//                     Name = "New Test Alias",
//                     PayeeID = assignedPayee.ID,
//                     AliasForPayee = assignedPayee
//                 };

//                 IActionResult actionResult = await controller.Create(newAlias);
//                 var result = actionResult as RedirectToActionResult;
                
//                 Assert.AreEqual("Index", result.ActionName, "Create should redirect to Index after successful create");
//                 Assert.AreEqual(nameof(Payee), result.ControllerName, "Create should redirect to PayeeController Index");

//                 mockBudget.Verify(m => m.AddAlias(It.IsAny<Alias>()), Times.Once());
//             }

//             [TestMethod]
//             public async Task CreatePOSTWithInvalidModelState() {
//                 int testID = budget.GetAliases().OrderByDescending(p => p.ID).Select(p => p.ID).First() + 1;
//                 Payee assignedPayee = budget.GetPayees().First();
//                 Alias newAlias = new Alias {
//                     ID = testID,
//                     Name = "New Test Alias",
//                     PayeeID = assignedPayee.ID,
//                     AliasForPayee = assignedPayee
//                 };

//                 controller.ModelState.AddModelError("test", "test");

//                 IActionResult actionResult = await controller.Create(newAlias);
//                 var viewResult = actionResult as ViewResult;

//                 Assert.AreEqual("Create", viewResult.ViewName, "Create should return to itself if ModelState is invalid");

//                 Alias model = (Alias)viewResult.Model;

//                 Assert.AreEqual(testID, model.ID, "The Alias was not sent back to the view");
//             }

//             [TestMethod]
//             public async Task CreatePOSTWithInvalidModelStatePopulatesPayeeSelectWithCorrectDefault() {
//                 int testID = budget.GetAliases().OrderByDescending(p => p.ID).First().ID + 1;
//                 Payee testPayee = budget.GetPayees().First();
//                 Alias newAlias = new Alias {
//                     ID = testID,
//                     Name = "New Test Alias",
//                     PayeeID = testPayee.ID,
//                     AliasForPayee = testPayee
//                 };

//                 controller.ModelState.AddModelError("test", "test");

//                 IActionResult actionResult = await controller.Create(newAlias);
//                 var result = actionResult as ViewResult;
//                 // Check that ViewData is not null
//                 AssertThatViewDataIsSelectList(result.ViewData, payeeSelectListKey, budget.GetPayees().Select(p => p.ID.ToString()), newAlias.PayeeID.ToString());
//             }
//         #endregion

//         #region "Delete Method Tests"
//             [TestMethod]
//             public async Task DeleteGETReturnsView() {
//                 int id = budget.GetAliases().Select(p => p.ID).First();

//                 IActionResult actionResult = await controller.Delete(id);
//                 var result = actionResult as ViewResult;
                
//                 Assert.IsNotNull(result);
//                 Assert.AreEqual("Delete", result.ViewName, $"Delete method returned '{result.ViewName}' instead of 'Delete'");
//             }

//             [DataTestMethod]
//             [DataRow(1), DataRow(2), DataRow(3), DataRow(-1), DataRow(300)]
//             public async Task DeleteGETReturnsCorrectAlias(int id) {
//                 IActionResult actionResult = await controller.Delete(id);

//                 if (!aliasReference.ContainsKey(id)) {
//                     Assert.IsInstanceOfType(actionResult, typeof(NotFoundResult), $"The id ({id}) doesn't exist. 404 Not Found should have been called");
//                 } else {
//                     string aliasName = aliasReference[id];

//                     var result = actionResult as ViewResult;
//                     Alias model = (Alias)result.ViewData.Model;

//                     Assert.AreEqual(aliasName, model.Name, $"The wrong Alias was returned by for ID = {id}");
//                 }
//             }

//             [TestMethod]
//             public async Task DeleteGETReturnsNotFoundForNullIndex() {
//                 IActionResult actionResult = await controller.Delete(null);

//                 Assert.IsInstanceOfType(actionResult, typeof(NotFoundResult), "A NULL id should result in 404 Not Found");
//             }

//             [TestMethod]
//             public async Task DeletePOSTRemoveExistingAlias() {
//                 int testID = budget.GetAliases().Select(p => p.ID).First();

//                 IActionResult actionResult = await controller.DeleteConfirmed(testID);
//                 var result = actionResult as RedirectToActionResult;

//                 Assert.AreEqual("Index", result.ActionName, "DeletePOST should redirect to Index");
//                 Assert.AreEqual(nameof(Payee), result.ControllerName, "DeletePOST should redirect to Payee Index");

//                 mockBudget.Verify(m => m.RemoveAlias(It.IsAny<Alias>()), Times.Once);
//             }

//             [TestMethod]
//             public async Task DeletePOSTRemoveNonExistantAlias() {
//                 int testID = budget.GetAliases().OrderByDescending(p => p.ID).Select(p => p.ID).First() + 10;
//                 int preCount = budget.GetAliases().Count();

//                 IActionResult actionResult = await controller.DeleteConfirmed(testID);
//                 var result = actionResult as RedirectToActionResult;

//                 Assert.AreEqual("Index", result.ActionName, "DeletePOST should redirect to Index");
//                 Assert.AreEqual(nameof(Payee), result.ControllerName, "DeletePOST should redirect to Payee Index");

//                 Assert.AreEqual(preCount, budget.GetAliases().Count(), "No alias should have been removed");
//             }
//         #endregion

//         #region Edit Method Tests
//             [TestMethod]
//             public async Task EditGETReturnsView() {
//                 int testID = budget.GetAliases().First().ID;

//                 IActionResult actionResult = await controller.Edit(testID);
//                 var result = actionResult as ViewResult;

//                 Assert.IsNotNull(result);
//                 Assert.AreEqual("Edit", result.ViewName, $"Edit method returned '{result.ViewName}' instead of 'Edit'");
//             }

//             [DataTestMethod]
//             [DataRow(1), DataRow(2), DataRow(3), DataRow(-1), DataRow(300)]
//             public async Task EditGETReturnsCorrectAlias(int id) {
//                 IActionResult actionResult = await controller.Edit(id);

//                 if (!aliasReference.ContainsKey(id)) {
//                     Assert.IsInstanceOfType(actionResult, typeof(NotFoundResult), $"The id ({id}) doesn't exist. 404 Not Found should have been called");
//                 } else {
//                     string aliasName = aliasReference[id];
//                     var result = actionResult as ViewResult;
//                     Alias model = (Alias)result.Model;

//                     Assert.AreEqual(aliasName, model.Name, $"The wrong Alias was returned by for ID = {id}");
//                 }
//             }

//             [TestMethod]
//             public async Task EditGETReturnsNotFoundForNULLId() {
//                 IActionResult actionResult = await controller.Edit(null);

//                 Assert.IsInstanceOfType(actionResult, typeof(NotFoundResult), "404 Not Found should be returned for a NULL id");
//             }

//             [TestMethod]
//             public async Task EditGETPopulatesViewDataWithPayees() {
//                 Alias testAlias = budget.GetAliases().First();
//                 IActionResult actionResult = await controller.Edit(testAlias.ID);
//                 var result = actionResult as ViewResult;

//                 // Check that ViewData is not null
//                 AssertThatViewDataIsSelectList(result.ViewData, payeeSelectListKey, budget.GetPayees().Select(p => p.ID.ToString()), testAlias.PayeeID.ToString());
//             }

//             [TestMethod]
//             public async Task EditPOSTWithValidModelState() {
//                 Alias aliasToEdit = budget.GetAliases().First();
//                 int testID = aliasToEdit.ID;
//                 string newName = aliasToEdit.Name + "_modified";
//                 aliasToEdit.Name = newName;
//                 IActionResult actionResult = await controller.Edit(aliasToEdit.ID, aliasToEdit);
//                 var result = actionResult as RedirectToActionResult;

//                 Assert.IsNotNull(result, "Edit POST did not return a RedirectToActionResult");
//                 Assert.AreEqual("Index", result.ActionName, $"Edit POST should redirect to 'Index', not {result.ActionName}");
//                 Assert.AreEqual(nameof(Payee), result.ControllerName, "Edit POST shoud redirect to Payee Index");

//                 Alias editedAlias = budget.GetAliases().Where(p => p.ID == testID).First();

//                 Assert.AreEqual(newName, editedAlias.Name, "The changes for the alias were not saved");
//             }

//             [TestMethod]
//             public async Task EditPOSTWithMismatchedIDReturnsNotFound() {
//                 Alias editedAlias = budget.GetAliases().First();
//                 editedAlias.Name += "_modified";
//                 IActionResult actionResult = await controller.Edit(editedAlias.ID + 1, editedAlias);

//                 Assert.IsInstanceOfType(actionResult, typeof(NotFoundResult), "If the ID doesn't match the ID of the edited Alias, Not Found should be called");
//             }

//             [TestMethod]
//             public async Task EditPOSTWithInvalidModelStateReturnsToEditView() {
//                 Alias editedAlias = budget.GetAliases().First();
//                 string newName = editedAlias.Name + "_modified";
//                 editedAlias.Name = newName;

//                 controller.ModelState.AddModelError("test", "test");
//                 IActionResult actionResult = await controller.Edit(editedAlias.ID, editedAlias);

//                 var result = actionResult as ViewResult;
//                 Assert.IsNotNull(result, "Edit POST with invalid model state should return a ViewResult");
//                 Assert.AreEqual("Edit", result.ViewName, $"Edit POST with invalid model state should return to 'Edit' view, not '{result.ViewName}'");

//                 Alias model = (Alias)result.Model;

//                 Assert.AreEqual(editedAlias.ID, model.ID, "The wrong alias was passed back to the View");

//                 Assert.AreEqual(newName, model.Name, "The updated values were not preserved when returning to the View");
//             }

//             [TestMethod]
//             public async Task EditPOSTWithInvalidModelStatePopulatesPayeeSelect() {
//                 Alias editedAlias = budget.GetAliases().First();
//                 Payee newPayee = budget.GetPayees().Where(p => p.ID != editedAlias.PayeeID).First();
//                 editedAlias.Name += "_modified";
//                 editedAlias.AliasForPayee = newPayee;
//                 editedAlias.PayeeID = newPayee.ID;

//                 controller.ModelState.AddModelError("test", "test");
//                 IActionResult actionResult = await controller.Edit(editedAlias.ID, editedAlias);
//                 var result = actionResult as ViewResult;

//                 // Check that ViewData is not null
//                 AssertThatViewDataIsSelectList(result.ViewData, payeeSelectListKey, budget.GetPayees().Select(p => p.ID.ToString()), newPayee.ID.ToString());
//             }

//             // TODO: Figure out how to test the DbUpdateConcurrencyException portion of Edit POST
//         #endregion
//     }
// }