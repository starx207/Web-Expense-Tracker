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
    public class BudgetCategoryController : CRUDController<CategoryCrudVm, BudgetCategory>
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
                singleAdder: category => service.AddCategoryAsync(category.Name, category.Amount, category.Type),
                singleDeleter: id => service.RemoveCategoryAsync(id),
                viewModelCreator: category => new CategoryCrudVm(category))  {

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
        public override async Task<IActionResult> Create(CategoryCrudVm budgetCategory) {
            if (ModelState.IsValid)
            {
                try {
                    await _serviceRO.AddCategoryAsync(budgetCategory.Name, budgetCategory.Amount, budgetCategory.Type);
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
        public override async Task<IActionResult> Edit(CategoryCrudVm budgetCategory) {
            if (ModelState.IsValid) {
                int id = GetRoutedId();
                try {
                    await _serviceRO.UpdateCategoryAsync(id, budgetCategory.Amount, budgetCategory.EffectiveFrom, budgetCategory.Type);
                    return RedirectToAction(nameof(Index));
                } catch (InvalidDateExpection dteex) {
                    ModelState.AddModelError(nameof(BudgetCategory.EffectiveFrom), dteex.Message);
                } catch (ExpenseTrackerException ex) {
                    if (ex is ConcurrencyException && (_serviceRO.CategoryExists(id))) {
                        throw;
                    }
                    return NotFound();
                }
            }
            return View(nameof(Edit), budgetCategory);
        }

        #endregion // Public Methods
    }
}
