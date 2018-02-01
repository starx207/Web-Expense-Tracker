using ExpenseTracker.Exceptions;
using ExpenseTracker.Models;
using ExpenseTracker.Services;
using ExpenseTracker.TestResources;
using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Controllers.Tests
{
    [TestClass]
    public class AliasController_Tests : TestCommon
    {
//         private IBudgetService budget;
//         private Dictionary<int, string> aliasReference;
        private AliasController controller;
        private Mock<IAliasManagerService> mockService;
        private readonly string payeeListKey = "PayeeList";
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
        [TestInitialize]
        public void Initialize_test_objects() {
            mockService = new Mock<IAliasManagerService>();
            controller = new AliasController(mockService.Object);
        }

        #region Create Tests
            [TestMethod]
            public void Create_GET_returns_create_view() {
                // Act
                var result = controller.Create();
                var viewResult = result as ViewResult;

                // Assert
                Assert.IsNotNull(viewResult);
                Assert.AreEqual("Create", viewResult.ViewName);
            }

            [TestMethod]
            public void Create_GET_correctly_populates_payee_select_list() {
                // Arrange
                var payees = new List<Payee> {
                    new Payee { ID = 1 },
                    new Payee { ID = 2 }
                }.AsQueryable();
                int testID = 1;
                mockService.Setup(m => m.GetOrderedPayees(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                    .Returns(payees);
                
                // Act
                var result = (ViewResult)controller.Create(testID);

                // Assert
                AssertThatViewDataIsSelectList(result.ViewData, payeeListKey, payees.Select(p => p.ID.ToString()), testID.ToString());
            }

            [TestMethod]
            public async Task Create_POST_calls_AddAliasAsync_and_redirects_to_payee_Index() {
                // Arrange
                var alias = new Alias();

                // Act
                var result = await controller.Create(alias);
                var redirectResult = result as RedirectToActionResult;

                // Assert
                mockService.Verify(m => m.AddAliasAsync(alias), Times.Once());
                Assert.AreEqual("Payee", redirectResult.ControllerName);
                Assert.AreEqual("Index", redirectResult.ActionName);
            }

            [TestMethod]
            public async Task Create_POST_returns_to_create_view_when_model_state_invalid() {
                // Arrange
                var alias = new Alias();
                controller.ModelState.AddModelError("test", "test");

                // Act
                var result = await controller.Create(alias);
                var viewResult = result as ViewResult;

                // Assert
                Assert.IsNotNull(viewResult);
                Assert.AreEqual("Create", viewResult.ViewName);
            }

            [TestMethod]
            public async Task Create_POST_correctly_populates_select_list_when_invalid_model_state() {
                // Arrange
                var payees = new List<Payee> {
                    new Payee { ID = 1 },
                    new Payee { ID = 2 }
                }.AsQueryable();
                mockService.Setup(m => m.GetOrderedPayees(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(payees);
                var alias = new Alias { PayeeID = 1 };
                controller.ModelState.AddModelError("test", "test");

                // Act
                var result = (ViewResult)(await controller.Create(alias));

                // Assert
                AssertThatViewDataIsSelectList(result.ViewData, payeeListKey, payees.Select(p => p.ID.ToString()), alias.PayeeID.ToString());
            }
        #endregion

        #region "Delete Method Tests"
            [TestMethod]
            public async Task Delete_GET_returns_delete_view_with_correct_model() {
                // Arrange
                var alias = new Alias();
                mockService.Setup(m => m.GetSingleAliasAsync(It.IsAny<int?>(), It.IsAny<bool>())).ReturnsAsync(alias);

                // Act
                var result = await controller.Delete(1);
                var viewResult = result as ViewResult;
                var model = viewResult.Model as Alias;

                // Assert
                mockService.Verify(m => m.GetSingleAliasAsync(1, true), Times.Once());
                Assert.IsNotNull(viewResult);
                Assert.AreEqual("Delete", viewResult.ViewName);
                Assert.AreSame(alias, model);
            }

            [TestMethod]
            public async Task Delete_GET_returns_NotFound_if_NullIdException_thrown() {
                // Arrange
                mockService.Setup(m => m.GetSingleAliasAsync(It.IsAny<int?>(), It.IsAny<bool>())).ThrowsAsync(new NullIdException());

                // Act
                var result = await controller.Delete(null);

                // Assert
                Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            }

            [TestMethod]
            public async Task Delete_GET_returns_NotFound_if_IdNotFoundException_thrown() {
                // Arrange
                mockService.Setup(m => m.GetSingleAliasAsync(It.IsAny<int?>(), It.IsAny<bool>())).ThrowsAsync(new IdNotFoundException());

                // Act
                var result = await controller.Delete(1);

                // Assert
                Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            }

            [TestMethod]
            public async Task Delete_GET_throws_exceptions_not_of_type_NullId_or_IdNotFound() {
                // Arrange
                mockService.Setup(m => m.GetSingleAliasAsync(It.IsAny<int?>(), It.IsAny<bool>())).ThrowsAsync(new Exception());

                // Act & Assert
                await Assert.ThrowsExceptionAsync<Exception>(() =>
                    controller.Delete(1));
            }

            [TestMethod]
            public async Task Delete_POST_calls_RemoveAliasAsync_and_redirects_to_Payee_index() {
                // Act
                var result = await controller.DeleteConfirmed(1);
                var redirectResult = result as RedirectToActionResult;

                // Assert
                mockService.Verify(m => m.RemoveAliasAsync(1), Times.Once());
                Assert.AreEqual("Payee", redirectResult.ControllerName);
                Assert.AreEqual("Index", redirectResult.ActionName);
            }
        #endregion

        #region Edit Method Tests
            [TestMethod]
            public async Task Edit_GET_returns_Edit_view_with_correct_model() {
                // Arrange
                var alias = new Alias();
                mockService.Setup(m => m.GetSingleAliasAsync(It.IsAny<int?>(), It.IsAny<bool>())).ReturnsAsync(alias);

                // Act
                var result = await controller.Edit(1);
                var viewResult = result as ViewResult;
                var model = viewResult.Model as Alias;

                // Assert
                mockService.Verify(m => m.GetSingleAliasAsync(1, false), Times.Once());
                Assert.IsNotNull(viewResult);
                Assert.AreEqual("Edit", viewResult.ViewName);
                Assert.AreSame(alias, model);
            }

            [TestMethod]
            public async Task Edit_GET_correctly_populates_select_list() {
                // Arrange
                var payees = new List<Payee> {
                    new Payee { ID = 1 },
                    new Payee { ID = 2 },
                    new Payee { ID = 3 }
                }.AsQueryable();
                var alias = new Alias { PayeeID = 2 };
                mockService.Setup(m => m.GetOrderedPayees(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(payees);
                mockService.Setup(m => m.GetSingleAliasAsync(It.IsAny<int?>(), It.IsAny<bool>())).ReturnsAsync(alias);

                // Act
                var result = (ViewResult)(await controller.Edit(1));

                // Assert
                AssertThatViewDataIsSelectList(result.ViewData, payeeListKey, payees.Select(p => p.ID.ToString()), alias.PayeeID.ToString());
            }

            [TestMethod]
            public async Task Edit_GET_returns_NotFound_when_NullIdException_thrown() {
                // Arrange
                mockService.Setup(m => m.GetSingleAliasAsync(It.IsAny<int?>(), It.IsAny<bool>())).ThrowsAsync(new NullIdException());

                // Act
                var result = await controller.Edit(null);

                // Assert
                Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            }

            [TestMethod]
            public async Task Edit_GET_returns_NotFound_when_IdNotFoundException_thrown() {
                // Arrange
                mockService.Setup(m => m.GetSingleAliasAsync(It.IsAny<int?>(), It.IsAny<bool>())).ThrowsAsync(new IdNotFoundException());

                // Act
                var result = await controller.Edit(1);

                // Assert
                Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            }

            [TestMethod]
            public async Task Edit_GET_throws_exceptions_not_of_type_NullId_or_IdNotFound() {
                // Arrange
                mockService.Setup(m => m.GetSingleAliasAsync(It.IsAny<int?>(), It.IsAny<bool>())).ThrowsAsync(new Exception());

                // Act & Assert
                await Assert.ThrowsExceptionAsync<Exception>(() =>
                    controller.Edit(1));
            }

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
        #endregion
    }
}