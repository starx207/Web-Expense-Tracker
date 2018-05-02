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
    public class BudgetCategoryController : CRUDController<BudgetCategory>
    { 
        #region Private Members

        private readonly ICategoryManagerService _serviceRO;

        #endregion // Private Members

        #region Constuctors

        /// <summary>
        /// The default Constructor
        /// </summary>
        /// <param name="service">The service to use in the controller</param>
        public BudgetCategoryController(ICategoryManagerService service)
            : base(collectionGetter: () => service.GetCategories(),
                singleGetter: id => service.GetSingleCategoryAsync(id),
                singleAdder: category => service.AddCategoryAsync(category),
                singleDeleter: id => service.RemoveCategoryAsync(id))  {
                _serviceRO = service;
                CollectionOrderFunc = category => category.Name;
            } 

        #endregion // Constuctors

        #region Public Methods
 
        /// <summary>
        /// Attempts to create the <see cref="BudgetCategory"/> specified in the view. 
        /// If the model is not valid, returns to the view
        /// POST: BudgetCategory/Create
        /// </summary>
        /// <param name="budgetCategory">The <see cref="BudgetCategory"/> to add</param>
        /// <returns></returns>
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        public override async Task<IActionResult> Create([Bind("ID,Name,Amount,EffectiveFrom,Type")] BudgetCategory budgetCategory) {
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
        /// Attempts to edit the <see cref="BudgetCategory"/> specified in the view. 
        /// If the model is not valid, returns to the view
        /// POST: BudgetCategory/Edit/5
        /// </summary>
        /// <param name="id">The id of the <see cref="BudgetCategory"/> to edit</param>
        /// <param name="budgetCategory">The <see cref="BudgetCategory"/> to edit</param>
        /// <returns></returns>
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        public override async Task<IActionResult> Edit(int id, [Bind("ID,Name,Amount,EffectiveFrom,Type")] BudgetCategory budgetCategory) {
            if (ModelState.IsValid) {
                try {
                    await Task.Delay(10); // _serviceRO.UpdateCategoryAsync(id, budgetCategory);
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

        // /// <summary>
        // /// Attempts to delete the <see cref="BudgetCategory"/> specified in the view, 
        // /// then redirects to <see cref="Index"/>
        // /// POST: BudgetCategory/Delete/5
        // /// </summary>
        // /// <param name="id"></param>
        // /// <returns></returns>
        // [HttpPost, ActionName("Delete")]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> DeleteConfirmed(int id) {
        //     await _serviceRO.RemoveCategoryAsync(id);
        //     return RedirectToAction(nameof(Index));
        // }

        #endregion // Public Methods
    }
}
