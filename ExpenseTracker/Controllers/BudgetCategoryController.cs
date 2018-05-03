using ExpenseTracker.Exceptions;
using ExpenseTracker.Models;
using ExpenseTracker.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace ExpenseTracker.Controllers
{
    public class BudgetCategoryController : CRUDController<CategoryCrudVm, BudgetCategory>
    { 
        #region Constuctors

        /// <summary>
        /// The default Constructor
        /// </summary>
        /// <param name="service">The service to use in the controller</param>
        public BudgetCategoryController(ICategoryManagerService service)
            : base(
                // Setup required functions
                collectionGetter: () => service.GetCategories(),
                singleGetter: id => service.GetSingleCategoryAsync(id),
                singleAdder: vm => service.AddCategoryAsync(vm.Name, vm.Amount, vm.Type),
                singleDeleter: id => service.RemoveCategoryAsync(id),
                singleEditor: (vm) => service.UpdateCategoryAsync(vm.NavId, vm.Amount, vm.EffectiveFrom, vm.Type),
                viewModelCreator: category => new CategoryCrudVm(category),
                existanceChecker: vm => service.CategoryExists(vm.NavId)
            )  
            {
                // Specify collection ordering
                CollectionOrderFunc = category => category.Name;
                ExceptionHandling = new Dictionary<Type, Func<Exception, IActionResult>> {
                    {typeof(InvalidDateExpection), ex => {
                        ModelState.AddModelError(nameof(BudgetCategory.EffectiveFrom), ex.Message);
                        return null;
                    }},
                    {typeof(ExpenseTrackerException), ex => {
                        return NotFound();
                    }}
                };
            } 

        #endregion // Constuctors
    }
}
