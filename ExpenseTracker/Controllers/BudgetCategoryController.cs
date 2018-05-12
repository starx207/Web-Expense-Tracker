using ExpenseTracker.Exceptions;
using ExpenseTracker.Models;
using ExpenseTracker.Repository.Extensions;
using ExpenseTracker.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Controllers
{
    public class BudgetCategoryController : CRUDController<CategoryCrudVm>
    { 
        #region Constuctors

        /// <summary>
        /// The default Constructor
        /// </summary>
        /// <param name="service">The service to use in the controller</param>
        public BudgetCategoryController(ICategoryManagerService service)
            : base(
                // Setup required functions
                singleAdder: vm => service.AddCategoryAsync(vm.Name, vm.Amount, vm.Type),
                singleDeleter: id => service.RemoveCategoryAsync(id),
                singleEditor: vm => service.UpdateCategoryAsync(vm.NavId, vm.Amount, vm.EffectiveFrom, vm.Type),
                existanceChecker: vm => service.CategoryExists(vm.NavId)
            )  
            {
                CollectionGetter = () => GetViewModelCollection(service);
                ViewModelCreator = id => CreateViewModel(id, service);
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

        #region Internal Helpers

        internal async Task<IEnumerable<CategoryCrudVm>> GetViewModelCollection(ICategoryManagerService service) {
            return await service.GetCategories()
                .OrderBy(c => c.Name.ToLower())
                .Select(c => new CategoryCrudVm(c))
                .Extension().ToListAsync();
        }

        internal async Task<CategoryCrudVm> CreateViewModel(int? id, ICategoryManagerService service) {
            CategoryCrudVm vm = null;
            if (id != null) {
                vm = new CategoryCrudVm(await service.GetSingleCategoryAsync(id));
            }
            return vm;
        }

        #endregion // Internal Helpers
    }
}
