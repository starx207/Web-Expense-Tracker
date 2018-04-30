using ExpenseTracker.Exceptions;
using ExpenseTracker.Models;
using ExpenseTracker.Repository;
using ExpenseTracker.Repository.Extensions;
using ExpenseTracker.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Controllers
{
    public class BudgetCategoryController : Controller
    { 
        #region Private Members

        private readonly ICategoryManagerService _serviceRO;

        #endregion // Private Members

        #region Constuctors

        /// <summary>
        /// The default Constructor
        /// </summary>
        /// <param name="service">The service to use in the controller</param>
        public BudgetCategoryController(ICategoryManagerService service) => _serviceRO = service;

        #endregion // Constuctors

        #region Public Methods
 
        /// <summary>
        /// Returns the Index view for <see cref="BudgetCategory"/>
        /// GET: BudgetCategory
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index() {
            return View(nameof(Index), await _serviceRO.GetCategories().OrderBy(c => c.Name).Extension().ToListAsync());
        }

        /// <summary>
        /// Returns the Details view for a <see cref="BudgetCategory"/>
        /// GET: BudgetCategory/Details/5
        /// </summary>
        /// <param name="id">The Id of the <see cref="BudgetCategory"/> to show</param>
        /// <returns></returns>
        public async Task<IActionResult> Details(int? id) {
            BudgetCategory budgetCategory;
            try {
                budgetCategory = await _serviceRO.GetSingleCategoryAsync(id);
            } catch (Exception ex) {
                if (ex is IdNotFoundException || ex is NullIdException) {
                    return NotFound();
                }
                throw;
            }
            return View(nameof(Details), budgetCategory);
        }

        /// <summary>
        /// Returns the Create view for the <see cref="BudgetCategory"/> class
        /// GET: BudgetCategory/Create
        /// </summary>
        /// <returns></returns>
        public IActionResult Create() => View(nameof(Create));

        /// <summary>
        /// Attempts to create the <see cref="BudgetCategory"/> specified in the view. 
        /// If the model is not valid, returns to the view
        /// POST: BudgetCategory/Create
        /// </summary>
        /// <param name="budgetCategory">The <see cref="BudgetCategory"/> to add</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,Amount,EffectiveFrom,Type")] BudgetCategory budgetCategory) {
            if (ModelState.IsValid)
            {
                try {
                    await _serviceRO.AddCategoryAsync(budgetCategory);
                    return RedirectToAction(nameof(Index));
                } catch (UniqueConstraintViolationException) {
                    ModelState.AddModelError(nameof(BudgetCategory.Name), "Name already in use by another Budget Category");
                }
                
            }
            return View(nameof(Create), budgetCategory);
        }

        /// <summary>
        /// Returns the Edit view for a <see cref="BudgetCategory"/>
        /// GET: BudgetCategory/Edit/5
        /// </summary>
        /// <param name="id">The Id of the <see cref="BudgetCategory"/> to show/edit</param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(int? id) {
            BudgetCategory budgetCategory;
            try {
                budgetCategory = await _serviceRO.GetSingleCategoryAsync(id);
            } catch (ExpenseTrackerException) {
                return NotFound();
            }
            return View(nameof(Edit) ,budgetCategory);
        }

        /// <summary>
        /// Attempts to edit the <see cref="BudgetCategory"/> specified in the view. 
        /// If the model is not valid, returns to the view
        /// POST: BudgetCategory/Edit/5
        /// </summary>
        /// <param name="id">The id of the <see cref="BudgetCategory"/> to edit</param>
        /// <param name="budgetCategory">The <see cref="BudgetCategory"/> to edit</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,Amount,EffectiveFrom,Type")] BudgetCategory budgetCategory) {
            if (ModelState.IsValid) {
                try {
                    await _serviceRO.UpdateCategoryAsync(id, budgetCategory);
                    return RedirectToAction(nameof(Index));
                } catch (InvalidDateExpection dteex) {
                    ModelState.AddModelError(nameof(BudgetCategory.EffectiveFrom), dteex.Message);
                } catch (ExpenseTrackerException ex) {
                    if (ex is ConcurrencyException && (_serviceRO.CategoryExists(budgetCategory.ID))) {
                        throw;
                    }
                    return NotFound();
                }
            }
            return View(nameof(Edit), budgetCategory);
        }

        /// <summary>
        /// Returns the Delete view for a <see cref="BudgetCategory"/>
        /// GET: BudgetCategory/Delete/5
        /// </summary>
        /// <param name="id">The Id of the <see cref="BudgetCategory"/> to show/delete</param>
        /// <returns></returns>
        public async Task<IActionResult> Delete(int? id) {
            BudgetCategory budgetCategory;
            try {
                budgetCategory = await _serviceRO.GetSingleCategoryAsync(id);
            } catch (Exception ex) {
                if (ex is IdNotFoundException || ex is NullIdException) {
                    return NotFound();
                }
                throw;
            }

            return View(nameof(Delete), budgetCategory);
        }

        /// <summary>
        /// Attempts to delete the <see cref="BudgetCategory"/> specified in the view, 
        /// then redirects to <see cref="Index"/>
        /// POST: BudgetCategory/Delete/5
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) {
            await _serviceRO.RemoveCategoryAsync(id);
            return RedirectToAction(nameof(Index));
        }

        #endregion // Public Methods
    }
}
