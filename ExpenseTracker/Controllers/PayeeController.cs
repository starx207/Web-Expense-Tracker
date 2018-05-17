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
    public class PayeeController : CRUDController<PayeeCrudVm>
    {
        #region Private Members

        private List<string> _allCategoryNames;

        #endregion // Private Members

        #region Constructors

        /// <summary>
        /// The default constructor
        /// </summary>
        /// <param name="service">The service to use in the controller</param>
        public PayeeController(IPayeeManagerService service) : base (
            singleAdder: payee => service.AddPayeeAsync(payee),
            singleEditor: payee => service.UpdatePayeeAsync(payee),
            existanceChecker: payee => service.PayeeExists(payee.NavId),
            singleDeleter: id => service.RemovePayeeAsync(id)
        ) 
        { 
            // Setup category name collection
            _allCategoryNames = service.GetCategories().Select(c => c.Name).ToList();

            // Setup CRUDController functions
            CollectionGetter = () => GetViewModelCollection(service);
            ViewModelCreator = id => GetViewModel(id, service);
            FailedPostRebinder = payee => payee.CategoryOptions = _allCategoryNames;

            // Setup error handling
            ExceptionHandling = new Dictionary<Type, Func<Exception, IActionResult>> {
                {typeof(ExpenseTrackerException), ex => NotFound()},
                {typeof(NullModelException), ex => {
                    ModelState.AddModelError("", ex.Message);
                    return null;
                }}
            };
        }

        #endregion // Constructors

        #region Internal Helpers

        internal async Task<IEnumerable<PayeeCrudVm>> GetViewModelCollection(ICommonService service) {
            return await service.GetPayees(true, true)
                .OrderBy(p => p.Name.ToLower())
                .Select(p => new PayeeCrudVm(p, _allCategoryNames))
                .Extension().ToListAsync();
        }

        internal async Task<PayeeCrudVm> GetViewModel(int? id, IPayeeManagerService service) {
            if (GetRoutedAction() == nameof(Create)) {
                return new PayeeCrudVm(null, _allCategoryNames);
            }
            return new PayeeCrudVm(await service.GetSinglePayeeAsync(id, true), _allCategoryNames);
        }

        #endregion // Internal Helpers
    }
}
