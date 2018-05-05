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
    public class PayeeController : Controller
    {
        #region Private Members

        private readonly IPayeeManagerService _serviceRO;

        #endregion // Private Members

        #region Constructors

        /// <summary>
        /// The default constructor
        /// </summary>
        /// <param name="service">The service to use in the controller</param>
        public PayeeController(IPayeeManagerService service) => _serviceRO = service;

        #endregion // Constructors

        #region Public Methods

        /// <summary>
        /// Returns the Index view for <see cref="Payee"/>
        /// GET: Payee
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index() {
            return View(nameof(Index), await _serviceRO.GetPayees(true, true).OrderBy(p => p.Name).Extension().ToListAsync());
        }

        /// <summary>
        /// Returns the Details view for a <see cref="Payee"/>
        /// GET: Payee/Details/5
        /// </summary>
        /// <param name="id">The Id of the <see cref="Payee"/> to show</param>
        /// <returns></returns>
        public async Task<IActionResult> Details(int? id) {
            Payee payee;
            try {
                payee = await _serviceRO.GetSinglePayeeAsync(id, true);
            } catch (ExpenseTrackerException) {
                return NotFound();
            }
            return View(nameof(Details), payee);
        }

        // GET: Payee/Create
        /// <summary>
        /// Return the Create view for a <see cref="Payee"/>
        /// GET: Payee/Create
        /// </summary>
        /// <returns></returns>
        public IActionResult Create() {
            CreateCategorySelectList();
            return View(nameof(Create));
        }

        /// <summary>
        /// Attempts to create the <see cref="Payee"/> specified in the view
        /// If the model is not valid, returns to the view
        /// POST: Payee/Create
        /// </summary>
        /// <param name="payee">The <see cref="Payee"/> to create</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,EffectiveFrom,BudgetCategoryID")] Payee payee) {
            if (ModelState.IsValid) {
                try {
                    await _serviceRO.AddPayeeAsync(payee);
                    return RedirectToAction(nameof(Index));
                } catch (ModelValidationException) {
                    ModelState.AddModelError(nameof(BudgetCategory.Name), "Name already in use by another Payee");
                }
            }
            CreateCategorySelectList(payee);
            return View(nameof(Create), payee);
        }

        /// <summary>
        /// Returns the Edit view for a <see cref="Payee"/>
        /// GET: Payee/Edit/5
        /// </summary>
        /// <param name="id">The Id of the <see cref="Payee"/> to show/edit</param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(int? id) {
            Payee payee;
            try {
                payee = await _serviceRO.GetSinglePayeeAsync(id);
            } catch (ExpenseTrackerException) {
                return NotFound();
            }
            CreateCategorySelectList(payee);
            return View(nameof(Edit), payee);
        }

        /// <summary>
        /// Attempts to edit the <see cref="Payee"/> specified in the view
        /// If the model is not valid, returns to the view
        /// POST: Payee/Edit/5
        /// </summary>
        /// <param name="id">The Id of the <see cref="Payee"/> to edit</param>
        /// <param name="payee">The <see cref="Payee"/> to edit</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,EffectiveFrom,BudgetCategoryID")] Payee payee) {
            if (ModelState.IsValid) {
                try {
                    await _serviceRO.UpdatePayeeAsync(id, payee);
                } catch (ExpenseTrackerException ex) {
                    if (ex is ConcurrencyException && _serviceRO.PayeeExists(payee.ID)) {
                        throw;
                    }
                    return NotFound();
                }
                return RedirectToAction(nameof(Index));
            }
            CreateCategorySelectList(payee);
            return View(nameof(Edit), payee);
        }

        /// <summary>
        /// Returns the Delete view for a <see cref="Payee"/>
        /// GET: Payee/Delete/5
        /// </summary>
        /// <param name="id">The Id of the <see cref="Payee"/> to show/delete</param>
        /// <returns></returns>
        public async Task<IActionResult> Delete(int? id) {
            Payee payee;
            try {
                payee = await _serviceRO.GetSinglePayeeAsync(id, true);
            } catch (ExpenseTrackerException) {
                return NotFound();
            }
            return View(nameof(Delete), payee);
        }

        /// <summary>
        /// Attempts to delete the <see cref="Payee"/> specified in the view,
        /// then redirects to the <see cref="Index"/> action
        /// POST: Payee/Delete/5
        /// </summary>
        /// <param name="id">The Id of the <see cref="Payee"/> to delete</param>
        /// <returns></returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) {
            await _serviceRO.RemovePayeeAsync(id);
            return RedirectToAction(nameof(Index));
        }

        #endregion // Public Methods

        #region Helper Functions

        /// <summary>
        /// Creates a <see cref="SelectList"/> of <see cref="BudgetCategory"/> objects and adds them to ViewData[CategoryList]
        /// </summary>
        /// <param name="payeeToSelect">The <see cref="Payee"/> whose <see cref="BudgetCategory"/> should be pre-selected</param>
        private void CreateCategorySelectList(Payee payeeToSelect = null) {
            ViewData["CategoryList"] = new SelectList(_serviceRO.GetCategories().OrderBy(c => c.Name), "ID", "Name", payeeToSelect == null ? null : payeeToSelect.BudgetCategoryID);
        }

        #endregion // Helper Functions
    }
}
