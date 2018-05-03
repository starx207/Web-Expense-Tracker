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
                singleAdder: category => service.AddCategoryAsync(category.Name, category.Amount, category.Type),
                singleDeleter: id => service.RemoveCategoryAsync(id),
                singleEditor: (category, id) => service.UpdateCategoryAsync(id, category.Amount, category.EffectiveFrom, category.Type),
                viewModelCreator: category => new CategoryCrudVm(category),
                existanceChecker: id => service.CategoryExists(id)
            )  
            {
                // Specify collection ordering
                CollectionOrderFunc = category => category.Name;
            } 

        #endregion // Constuctors
    }
}
