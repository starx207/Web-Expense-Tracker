using ExpenseTracker.Exceptions;
using ExpenseTracker.Models;
using ExpenseTracker.Repository;
using ExpenseTracker.Repository.Extensions;
using ExpenseTracker.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Controllers
{
    public class TransactionController : Controller
    {
        #region Private Members

        private readonly ITransactionManagerService _serviceRO;

        #endregion // Private Members

        #region Constructors

        public TransactionController(ITransactionManagerService service) => _serviceRO = service;

        #endregion // Constructors

        #region Public Methods

        /// <summary>
        /// Returns the Index view for <see cref="Transaction"/>
        /// GET: Transaction
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index() {
            return View(nameof(Index), await _serviceRO.GetTransactions(true, true).OrderByDescending(t => t.Date).Extension().ToListAsync());
        }

        /// <summary>
        /// Returns the Details view for a <see cref="Transaction"/>
        /// GET Transaction/Details/5
        /// </summary>
        /// <param name="id">The Id of the <see cref="Transaction"/> to show</param>
        /// <returns></returns>
        public async Task<IActionResult> Details(int? id) {
            Transaction transaction;
            try {
                transaction = await _serviceRO.GetSingleTransactionAsync(id, true);
            } catch (ExpenseTrackerException) {
                return NotFound();
            }
            return View(nameof(Details), transaction);
        }

        /// <summary>
        /// Returns the Create view for a <see cref="Transaction"/>
        /// GET: Transaction/Create
        /// </summary>
        /// <returns></returns>
        public IActionResult Create() {
            PopulateSelectLists();
            return View(nameof(Create));
        }

        /// <summary>
        /// Attempts to create the <see cref="Transaction"/> specified in the view.
        /// If the model is not valid, returns to the view
        /// POST: Transaction/Create
        /// </summary>
        /// <param name="transaction">The <see cref="Transaction"/> to add</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Date,Amount,OverrideCategoryID,PayeeID")] Transaction transaction) {
            if (ModelState.IsValid) {
                await _serviceRO.AddTransactionAsync(transaction);
                return RedirectToAction(nameof(Index));
            }
            PopulateSelectLists(transaction.OverrideCategoryID, transaction.PayeeID);
            return View(nameof(Create), transaction);
        }

        /// <summary>
        /// Returns the Edit view for a <see cref="Transaction"/>
        /// GET: Transaction/Edit/5
        /// </summary>
        /// <param name="id">The Id of the <see cref="Transaction"/> to edit</param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(int? id) {
            Transaction transaction;
            try {
                transaction = await _serviceRO.GetSingleTransactionAsync(id);
            } catch (ExpenseTrackerException) {
                return NotFound();
            }
            PopulateSelectLists(transaction.OverrideCategoryID, transaction.PayeeID);
            return View(nameof(Edit), transaction);
        }

        /// <summary>
        /// Attempts to edit the <see cref="Transaction"/> specified in the view.
        /// If the model is not valid, returns to the view
        /// POST: Transaction/Edit/5
        /// </summary>
        /// <param name="id">The Id of the <see cref="Transaction"/> to edit</param>
        /// <param name="transaction">The <see cref="Transaction"/> to edit</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Date,Amount,OverrideCategoryID,PayeeID")] Transaction transaction) {
            if (ModelState.IsValid) {
                try {
                    await _serviceRO.UpdateTransactionAsync(id, transaction);
                } catch (ExpenseTrackerException ex) {
                    if (ex is ConcurrencyException && _serviceRO.TransactionExists(transaction.ID)) {
                        throw;
                    }
                    return NotFound();
                }
                return RedirectToAction(nameof(Index));
            }
            PopulateSelectLists(transaction.OverrideCategoryID, transaction.PayeeID);
            return View(nameof(Edit), transaction);
        }

        /// <summary>
        /// Returns the Delete view for a <see cref="Transaction"/>
        /// GET: Transaction/Delete/5
        /// </summary>
        /// <param name="id">The Id of the <see cref="Transaction"/> to show/delete</param>
        /// <returns></returns>
        public async Task<IActionResult> Delete(int? id) {
            Transaction transaction;
            try {
                transaction = await _serviceRO.GetSingleTransactionAsync(id, true);
            } catch (ExpenseTrackerException) {
                return NotFound();
            }
            return View(nameof(Delete), transaction);
        }

        // POST: Transaction/Delete/5
        /// <summary>
        /// Attempts to delete the <see cref="Transaction"/> specified in the view,
        /// then returns to the Index view
        /// </summary>
        /// <param name="id">The Id of the <see cref="Transaction"/> to delete</param>
        /// <returns></returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) {
            await _serviceRO.RemoveTransactionAsync(id);
            return RedirectToAction(nameof(Index));
        }

        #endregion // Public Methods

        #region Helper Functions

        /// <summary>
        /// Creates a <see cref="SelectList"/> of <see cref="Payee"/> objects in ViewData[PayeeList] and
        /// a <see cref="SelectList"/> of <see cref="BudgetCategory"/> objects in ViewData[CategoryList]
        /// </summary>
        /// <param name="selectedCategoryID">The Id of the <see cref="BudgetCategory"/> to default to</param>
        /// <param name="selectedPayeeID">The Id of the <see cref="Payee"/> to default to</param>
        private void PopulateSelectLists(int? selectedCategoryID = null, int? selectedPayeeID = null) {
            ViewData["CategoryList"] = new SelectList(_serviceRO.GetCategories().OrderBy(c => c.Name), "ID", "Name", selectedCategoryID);
            ViewData["PayeeList"] = new SelectList(_serviceRO.GetPayees().OrderBy(p => p.Name), "ID", "Name", selectedPayeeID);
        }

        #endregion // Helper Functions
    }
}
