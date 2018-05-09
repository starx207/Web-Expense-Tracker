using ExpenseTracker.Exceptions;
using ExpenseTracker.Models;
using ExpenseTracker.Repository;
using ExpenseTracker.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Controllers
{
  public class AliasController : CRUDController<AliasCrudVm>
    {
        #region Contructors

        public AliasController(IAliasManagerService service) : base(
            singleAdder: alias => service.AddAliasAsync(alias.Name, (int)alias.PayeeID),
            singleDeleter: id => service.RemoveAliasAsync(id),
            singleEditor: alias => service.UpdateAliasAsync(alias.NavId, alias.Name, (int)alias.PayeeID),
            existanceChecker: alias => service.AliasExists(alias.NavId)
        ) 
        {
            // Define how a view model should be created
            ViewModelCreatorFunc = async id => {
                if (id == null && GetRoutedAction() != nameof(Create)) {
                    return null;
                }
                AliasCrudVm vm = null;
                if (id == null) {
                    vm = new AliasCrudVm(null, service);
                } else {
                    vm = new AliasCrudVm(await service.GetSingleAliasAsync(id, true), service);
                }
                if (GetRoutedAction() == nameof(Create) &&
                    int.TryParse(GetRequestParameter("payeeID"), out int fetchedId)) {
                        vm.PayeeID = fetchedId;
                }
                return vm;
            };

            ExceptionHandling = new Dictionary<Type, Func<Exception, IActionResult>> {
                {typeof(ExpenseTrackerException), ex => NotFound()}
            };
        }

        #endregion // Constructors

        #region Public Actions

        // Alias has no views for these actions
        public override async Task<IActionResult> Index() => await Task.FromResult(RedirectToAction(nameof(Index), nameof(Payee)));
        public override async Task<IActionResult> Details(int? id) => await Task.FromResult(NotFound());

        #endregion // Public Actions
    }
}