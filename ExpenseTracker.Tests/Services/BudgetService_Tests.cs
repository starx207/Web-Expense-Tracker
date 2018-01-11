// //using ExpenseTracker.Repository;
// using ExpenseTracker.Services;
// using ExpenseTracker.Models;
// using ExpenseTracker.Data;
// using Microsoft.VisualStudio.TestTools.UnitTesting;
// using Moq;
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;

// namespace ExpenseTracker.Tests.Services
// {
//     [TestClass]
//     public class BudgetService_Tests
//     {
//         private BudgetContext repo;
//         private Mock<BudgetContext> mockRepo;
//         private IBudgetService budget;
//         private Dictionary<int, string> categoryRef;
//         private Dictionary<int, string> payeeRef;
//         private Dictionary<int, string> aliasRef;
//         private Dictionary<int, double> transactionRef;
//         private int categoryCount, payeeCount, aliasCount, transactionCount;

//         [TestInitialize]
//         public void InitializeTestData() {
//             // Create in-memory BudgetCategories
//             List<BudgetCategory> categories = TestInitializer.CreateTestCategories();
//             // Add categories and ids to dictionary for verification
//             categoryRef = new Dictionary<int, string>();
//             foreach (var category in categories) {
//                 categoryRef.Add(category.ID, category.Name);
//             }
//             // Record the number of categories at the start of the test
//             categoryCount = categories.Count;

//             // Create in-memory Payees
//             List<Payee> payees = TestInitializer.CreateTestPayees(categories.AsQueryable());
//             // Add payees and ids to dictionary for verification
//             payeeRef = new Dictionary<int, string>();
//             foreach (var payee in payees) {
//                 payeeRef.Add(payee.ID, payee.Name);
//             }
//             // Record the number of payees at the start of the test
//             payeeCount = payees.Count;

//             // Create in-memory Aliases
//             List<Alias> aliases = TestInitializer.CreateTestAliases(payees.AsQueryable());
//             aliasRef = new Dictionary<int, string>();
//             foreach (var alias in aliases) {
//                 aliasRef.Add(alias.ID, alias.Name);
//             }
//             aliasCount = aliases.Count;

//             // Create in-memory Transactions
//             List<Transaction> transactions = TestInitializer.CreateTestTransactions(categories.AsQueryable(), payees.AsQueryable());
//             transactionRef = new Dictionary<int, double>();
//             foreach (var trans in transactions) {
//                 transactionRef.Add(trans.ID, trans.Amount);
//             }
//             transactionCount = transactions.Count;

//             // Iniitlize the IBudgetAccess repo with in-memory data
//             mockRepo = new Mock<BudgetContext>();
//             mockRepo.Setup(r => r.BudgetCategories).ReturnsDbSet(categories);
//             mockRepo.Setup(r => r.Payees).ReturnsDbSet(payees);
//             mockRepo.Setup(r => r.Transactions).ReturnsDbSet(transactions);
//             mockRepo.Setup(r => r.Aliases).ReturnsDbSet(aliases);

//             repo = mockRepo.Object;
//             //repo = new MockDataAccess(transactions, payees, categories, aliases);
//         }

//         [TestMethod]
//         public void BudgetImplementsIBudget()
//         {
//             try {
//                 budget = new BudgetService(repo);
//             } catch {
//                 Assert.Fail("Budget class does not implement IBudget");
//             }
//             // if no error, Budget implements IBudget. Test passes
//             Assert.IsTrue(true);
//         }

//         #region "BudgetCategory Tests"
//             [TestMethod]
//             public void GetAllBudgetCategoriesReturnsCorrectCount() {
//                 budget = new BudgetService(repo);
//                 IQueryable<BudgetCategory> allCategories;

//                 allCategories = budget.GetCategories();

//                 Assert.AreEqual(categoryCount, allCategories.Count(), "GetCategories() returned wrong number of BudgetCategories");
//             }

//             [DataTestMethod]
//             [DataRow(1), DataRow(2), DataRow(3), DataRow(4)]
//             public void GetAllBudgetCategoriesReturnsCorrectCategories(int id) {
//                 budget = new BudgetService(repo);
//                 BudgetCategory category;
//                 IQueryable<BudgetCategory> allCategories;
//                 string expectedName = categoryRef[id];

//                 allCategories = budget.GetCategories();
//                 category = allCategories.Where(c => c.ID == id).First();

//                 Assert.AreEqual(expectedName, category.Name, $"BudgetCategory with ID = {id} should have Name = {expectedName}");
//             }

//             [TestMethod]
//             public async Task AddANewBudgetCategory() {
//                 budget = new BudgetService(repo);
//                 int testID = repo.BudgetCategories().OrderByDescending(c => c.ID).Select(c => c.ID).First() + 1;
//                 string testName = "Insurance";
//                 BudgetCategory newCategory = new BudgetCategory {
//                     ID = testID,
//                     Name = testName,
//                     Amount = 280.94,
//                     BeginEffectiveDate = new DateTime(2016, 09, 01),
//                     EndEffectiveDate = null,
//                     Type = BudgetType.Expense
//                 };
//                 int newCount;

//                 await budget.AddBudgetCategoryAsync(newCategory);
//                 newCount = budget.GetCategories().Count();

//                 mockRepo.Verify(r => r.AddBudgetCategory(It.IsAny<BudgetCategory>()), Times.Once());
//             }

//             [TestMethod]
//             public async Task RemoveABudgetCategory() {
//                 budget = new BudgetServiceII(repo);
//                 int testID = repo.BudgetCategories().Select(c => c.ID).First();
//                 BudgetCategory remove = budget.GetCategories().Where(c => c.ID == testID).First();
//                 int newCount;

//                 await budget.RemoveBudgetCategoryAsync(remove.ID);
//                 newCount = budget.GetCategories().Count();

//                 mockRepo.Verify(r => r.DeleteBudgetCategory(It.IsAny<BudgetCategory>()), Times.Once());
//             }

//             [TestMethod]
//             public async Task EditABudgetCategory() {
//                 budget = new BudgetServiceII(repo);
//                 BudgetCategory categoryToEdit = budget.GetCategories().First();
//                 int testID = categoryToEdit.ID;
//                 string originalName = categoryToEdit.Name;
//                 string newName = originalName + "_modified";

//                 categoryToEdit.Name = newName;
//                 await budget.UpdateBudgetCategoryAsync(categoryToEdit.ID, categoryToEdit);

//                 BudgetCategory editedCategory = budget.GetCategories().First(c => c.ID == testID);

//                 Assert.AreEqual(newName, editedCategory.Name, "The Budget Category name was not updated");
//             }
//         #endregion
    
//         #region "Payee Tests"

//             [TestMethod]
//             public void GetAllPayeesReturnsCorrectCount() {
//                 budget = new BudgetServiceII(repo);
//                 IQueryable<Payee> allPayees;

//                 allPayees = budget.GetPayees();

//                 Assert.AreEqual(payeeCount, allPayees.Count(), "The wrong number of Payees was returned");
//             }

//             [DataTestMethod]
//             [DataRow(1), DataRow(2), DataRow(3), DataRow(4)]
//             public void GetPayeesReturnsCorrectPayees(int id) {
//                 budget = new BudgetServiceII(repo);
//                 Payee payee;
//                 IQueryable<Payee> allPayees;
//                 string expectedName = payeeRef[id];

//                 allPayees = budget.GetPayees();
//                 payee = allPayees.Where(p => p.ID == id).First();

//                 Assert.AreEqual(expectedName, payee.Name, $"Id = {id} should return '{expectedName}'");
//             }

//             [TestMethod]
//             public async Task AddAPayee() {
//                 budget = new BudgetServiceII(repo);
//                 int testID = budget.GetPayees().OrderByDescending(p => p.ID).Select(p => p.ID).First() + 1;
//                 string payeeName = "Sweetwater";
//                 BudgetCategory category = budget.GetCategories().First();
//                 Payee payee = new Payee {
//                     ID = testID,
//                     Name = payeeName,
//                     BeginEffectiveDate = new DateTime(2017, 3, 25),
//                     EndEffectiveDate = null,
//                     BudgetCategoryID = category.ID,
//                     Category = category
//                 };
//                 int newCount;

//                 await budget.AddPayeeAsync(payee);
//                 newCount = budget.GetPayees().Count();

//                 mockRepo.Verify(r => r.AddPayee(It.IsAny<Payee>()), Times.Once());
//             }

//             [TestMethod]
//             public async Task DeleteAPayee() {
//                 budget = new BudgetServiceII(repo);
//                 Payee payeeToRemove = budget.GetPayees().First();
//                 int testID = payeeToRemove.ID;
//                 int newCount;

//                 await budget.RemovePayeeAsync(payeeToRemove.ID);
//                 newCount = budget.GetPayees().Count();

//                 mockRepo.Verify(r => r.DeletePayee(It.IsAny<Payee>()), Times.Once());
//             }

//             [TestMethod]
//             public async Task EditAPayee() {
//                 budget = new BudgetServiceII(repo);
//                 Payee payeeToEdit = budget.GetPayees().First();
//                 int testID = payeeToEdit.ID;
//                 string originalName = payeeToEdit.Name;

//                 string newName = originalName + " plus something extra";

//                 payeeToEdit.Name = newName;
//                 await budget.UpdatePayeeAsync(payeeToEdit.ID, payeeToEdit);

//                 Payee editedPayee = budget.GetPayees().Where(p => p.ID == testID).First();

//                 Assert.AreEqual(newName, editedPayee.Name, $"Payee name was not updated");

//                 BudgetCategory newCategory = budget.GetCategories().Where(c => c.ID != editedPayee.BudgetCategoryID).First();

//                 testID = editedPayee.ID;
//                 editedPayee.Category = newCategory;
//                 editedPayee.BudgetCategoryID = newCategory.ID;

//                 await budget.UpdatePayeeAsync(editedPayee.ID, editedPayee);

//                 payeeToEdit = budget.GetPayees().Where(p => p.ID == testID).First();

//                 Assert.AreEqual(newCategory.Name, payeeToEdit.Category.Name, "The payee was not correctly reassigned to the new category");
//                 Assert.AreEqual(newCategory.ID, payeeToEdit.BudgetCategoryID, "The payee was not correctly reassinged to the new category");
//             }

//         #endregion

//         #region Alias Tests
//             [TestMethod]
//             public void GetAliasesReturnsCorrectCount() {
//                 budget = new BudgetServiceII(repo);
//                 IQueryable<Alias> allAliases;

//                 allAliases = budget.GetAliases();

//                 Assert.AreEqual(aliasCount, allAliases.Count(), "The wrong number of Aliases was returned");
//             }

//             [DataTestMethod]
//             [DataRow(1), DataRow(2), DataRow(3)]
//             public void GetAliasesReturnsCorrectAliases(int id) {
//                 budget = new BudgetServiceII(repo);
//                 Alias alias;
//                 IQueryable<Alias> allAliases;
//                 string expectedName = aliasRef[id];

//                 allAliases = budget.GetAliases();
//                 alias = allAliases.Where(a => a.ID == id).First();

//                 Assert.AreEqual(expectedName, alias.Name, $"Id = {id}, should return '{expectedName}'");
//             }

//             [TestMethod]
//             public void AddAnAlias() {
//                 budget = new BudgetServiceII(repo);
//                 int testID = budget.GetAliases().OrderByDescending(a => a.ID).First().ID + 1;
//                 string aliasName = "Yet Another Walmart Alias";
//                 Payee payee = budget.GetPayees().First();
//                 Alias newAlias = new Alias {
//                     ID = testID,
//                     Name = aliasName,
//                     PayeeID = payee.ID,
//                     AliasForPayee = payee
//                 };
//                 int newCount;

//                 budget.AddAlias(newAlias);
//                 newCount = budget.GetAliases().Count();

//                 mockRepo.Verify(r => r.AddAlias(It.IsAny<Alias>()), Times.Once());
//             }

//             [TestMethod]
//             public void DeleteAnAlias() {
//                 budget = new BudgetServiceII(repo);
//                 Alias aliasToRemove = budget.GetAliases().First();
//                 int testID = aliasToRemove.ID;
//                 int newCount;

//                 budget.RemoveAlias(aliasToRemove);
//                 newCount = budget.GetAliases().Count();

//                 mockRepo.Verify(r => r.DeleteAlias(It.IsAny<Alias>()), Times.Once());
//             }

//             [TestMethod]
//             public void EditAnAlias() {
//                 budget = new BudgetServiceII(repo);
//                 Alias aliasToEdit = budget.GetAliases().First();
//                 int testID = aliasToEdit.ID;
//                 string originalName = aliasToEdit.Name;
//                 int originalPayeeID = aliasToEdit.PayeeID;
                
//                 string newName = originalName + "_modified";
//                 Payee newPayee = budget.GetPayees().Where(p => p.ID != originalPayeeID).First();

//                 aliasToEdit.Name = newName;
//                 budget.UpdateAlias(aliasToEdit);

//                 Alias editedAlias = budget.GetAliases().Where(a => a.ID == testID).First();

//                 Assert.AreEqual(newName, editedAlias.Name, "Alias name was not updated");

//                 editedAlias.PayeeID = newPayee.ID;
//                 editedAlias.AliasForPayee = newPayee;
//                 budget.UpdateAlias(editedAlias);

//                 aliasToEdit = budget.GetAliases().Where(a => a.ID == testID).First();

//                 Assert.AreEqual(newPayee.ID, aliasToEdit.PayeeID, "The alias was not correctly reassigned to a new payee");
//             }
//         #endregion

//         #region Transaction Tests
//             [TestMethod]
//             public void GetAllTransactionsReturnsCorrectCount() {
//                 budget = new BudgetServiceII(repo);
//                 IQueryable<Transaction> allTransactions;

//                 allTransactions = budget.GetTransactions();

//                 Assert.AreEqual(transactionCount, allTransactions.Count(), "The wrong number of Transactions was returned");
//             }

//             [DataTestMethod]
//             [DataRow(1), DataRow(2), DataRow(3), DataRow(4)]
//             public void GetTransactionsReturnsCorrectTransaction(int id) {
//                 budget = new BudgetServiceII(repo);
//                 Transaction transaction;
//                 IQueryable<Transaction> allTransactions;
//                 double expectedAmount = transactionRef[id];

//                 allTransactions = budget.GetTransactions();
//                 transaction = allTransactions.Where(p => p.ID == id).First();

//                 Assert.AreEqual(expectedAmount, transaction.Amount, $"Id = {id} should return '{expectedAmount}'");
//             }

//             [TestMethod]
//             public void AddATransaction() {
//                 budget = new BudgetServiceII(repo);
//                 int testID = budget.GetTransactions().OrderByDescending(p => p.ID).Select(p => p.ID).First() + 1;
//                 double transactionAmount = 123123;
//                 BudgetCategory category = budget.GetCategories().First();
//                 Payee payee = budget.GetPayees().First();
//                 Transaction trans = new Transaction {
//                     ID = testID,
//                     Date = DateTime.Now,
//                     Amount = transactionAmount,
//                     PayeeID = payee.ID,
//                     PayableTo = payee,
//                     OverrideCategoryID = category.ID,
//                     OverrideCategory = category
//                 };
//                 int newCount;

//                 budget.AddTransaction(trans);
//                 newCount = budget.GetTransactions().Count();

//                 mockRepo.Verify(r => r.AddTransaction(It.IsAny<Transaction>()), Times.Once());
//             }

//             [TestMethod]
//             public void DeleteATransaction() {
//                 budget = new BudgetServiceII(repo);
//                 Transaction transToRemove = budget.GetTransactions().First();
//                 int testID = transToRemove.ID;
//                 int newCount;

//                 budget.RemoveTransaction(transToRemove);
//                 newCount = budget.GetTransactions().Count();

//                 mockRepo.Verify(r => r.DeleteTransaction(It.IsAny<Transaction>()), Times.Once());
//             }

//             [TestMethod]
//             public void EditATransaction() {
//                 budget = new BudgetServiceII(repo);
//                 Transaction transToEdit = budget.GetTransactions().First();
//                 int testID = transToEdit.ID;
//                 double originalAmount = transToEdit.Amount;

//                 double newAmount = originalAmount + 50;

//                 transToEdit.Amount = newAmount;
//                 budget.UpdateTransaction(transToEdit);

//                 Transaction editedTrans = budget.GetTransactions().Where(p => p.ID == testID).First();

//                 Assert.AreEqual(newAmount, editedTrans.Amount, $"Transaction amount was not updated");

//                 BudgetCategory newCategory = budget.GetCategories().Where(c => c.ID != editedTrans.OverrideCategoryID).First();

//                 testID = editedTrans.ID;
//                 editedTrans.OverrideCategory = newCategory;
//                 editedTrans.OverrideCategoryID = newCategory.ID;

//                 budget.UpdateTransaction(editedTrans);

//                 transToEdit = budget.GetTransactions().Where(p => p.ID == testID).First();

//                 Assert.AreEqual(newCategory.Name, transToEdit.OverrideCategory.Name, "The transaction was not correctly reassigned to the new category");
//                 Assert.AreEqual(newCategory.ID, transToEdit.OverrideCategoryID, "The transaction was not correctly reassinged to the new category");
//             }

//             [DataTestMethod]
//             [DataRow(100.1254), DataRow(10.1234), DataRow(0.1), DataRow(-75.0349)]
//             public void RoundAmountToNearestCentOnAdd(double testAmount) {
//                 Transaction retrievedTrans = new Transaction();
//                 mockRepo.Setup(m => m.AddTransaction(It.IsAny<Transaction>())).Callback<Transaction>(t => retrievedTrans = t);

//                 budget = new BudgetServiceII(repo);
//                 int testID = budget.GetTransactions().OrderByDescending(t => t.ID).First().ID + 1;
//                 double roundedAmount = Math.Round(testAmount, 2, MidpointRounding.AwayFromZero);
//                 Transaction testTrans = new Transaction {
//                     ID = testID,
//                     Date = DateTime.Parse("10/16/2017"),
//                     Amount = testAmount
//                 };

//                 budget.AddTransaction(testTrans);

//                 //Transaction retrievedTrans = budget.GetTransactions().Where(t => t.ID == testID).First();

//                 Assert.AreEqual(roundedAmount, retrievedTrans.Amount, $"Amount = {testAmount} should be rounded to {roundedAmount} when transaction is added");
//             }

//             [DataTestMethod]
//             [DataRow(100.1254), DataRow(10.1234), DataRow(0.1), DataRow(-75.0349)]
//             public void RoundAmountToNearestCentOnUpdate(double testAmount) {
//                 budget = new BudgetServiceII(repo);
//                 Transaction testTrans = budget.GetTransactions().First();
//                 double roundedAmount = Math.Round(testAmount, 2, MidpointRounding.AwayFromZero);
//                 testTrans.Amount = testAmount;

//                 budget.UpdateTransaction(testTrans);

//                 Transaction retrievedTrans = budget.GetTransactions().Where(t => t.ID == testTrans.ID).First();

//                 Assert.AreEqual(roundedAmount, retrievedTrans.Amount, $"Amount = {testAmount} should be rounded to {roundedAmount} when transaction is edited");
//             }
//         #endregion
//     }
// }
