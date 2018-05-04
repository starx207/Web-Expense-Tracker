using ExpenseTracker.Exceptions;
using ExpenseTracker.Models;
using ExpenseTracker.Repository;
using ExpenseTracker.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Controllers
{
  public class AliasController : BaseController
    {
        #region Private Members

        private readonly IAliasManagerService _serviceRO;
        private readonly string _payeeIndexRO = "Index";

        #endregion // Private Members

        #region Contructors

        /// <summary>
        /// The default Constructor
        /// </summary>
        /// <param name="service">The service to use in the controller</param>
        public AliasController(IAliasManagerService service) => _serviceRO = service;

        #endregion // Constructors

        #region Public Actions

        /// <summary>
        /// Returns Create view for <see cref="Alias"/>
        /// GET: Alias/Create
        /// </summary>
        /// <param name="payeeID">The Id of the <see cref="Payee"/> the alias belongs to</param>
        /// <returns></returns>
        public IActionResult Create() {
            var aliasVm = new AliasCrudVm(_serviceRO);
            if (int.TryParse(GetRequestParameter("payeeID"), out int fetchedId)) {
                aliasVm.PayeeID = fetchedId;
            }
            return View(nameof(Create), aliasVm);
        }

        /// <summary>
        /// Attempts to create the <see cref="Alias"/> specified in the view.
        /// If the model is not valid, returns to the view
        /// POST: Alias/Create
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AliasCrudVm alias) {
            if (ModelState.IsValid) {
                try {
                    await _serviceRO.AddAliasAsync(alias.Name, (int)alias.PayeeID);
                    return RedirectToAction(_payeeIndexRO, nameof(Payee));
                } catch (UniqueConstraintViolationException) {
                    ModelState.AddModelError(nameof(Alias.Name), "Name already in use by another Alias");
                }
            }
            return View(nameof(Create), alias);
        }

        /// <summary>
        /// Returns the Edit view for <see cref="Alias"/>
        /// GET: Alias/Edit/5
        /// </summary>
        /// <param name="id">The Id of the <see cref="Alias"/> to edit</param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(int? id) {
            AliasCrudVm alias;
            try {
                alias = new AliasCrudVm(await _serviceRO.GetSingleAliasAsync(id), _serviceRO);
            } catch (ExpenseTrackerException) {
                return NotFound();
            }
            return View(nameof(Edit), alias);
        }

        /// <summary>
        /// Attempts to edit the <see cref="Alias"/> specified in the view.
        /// If the model is not valid, returns to the view
        /// POST: Alias/Edit/5
        /// </summary>
        /// <param name="id">The Id of the <see cref="Alias"/> to edit</param>
        /// <param name="alias">The <see cref="Alias"/> to edit</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AliasCrudVm alias) {
            if (ModelState.IsValid) {
                try {
                    await _serviceRO.UpdateAliasAsync(alias.NavId, alias.Name, (int)alias.PayeeID);
                    return RedirectToAction(_payeeIndexRO, nameof(Payee));
                } catch (UniqueConstraintViolationException uex) {
                    ModelState.AddModelError(uex.PropertyName, uex.Message);
                } catch (ExpenseTrackerException ex) {
                    if (ex is ConcurrencyException && _serviceRO.AliasExists(alias.NavId)) {
                        throw;
                    }
                    return NotFound();
                }
            }
            return View(nameof(Edit), alias);
        }

        /// <summary>
        /// Returns the Delete view for <see cref="Alias"/>
        /// GET: Alias/Delete/5
        /// </summary>
        /// <param name="id">The Id of the <see cref="Alias"/> to show/delete</param>
        /// <returns></returns>
        public async Task<IActionResult> Delete(int? id) {
            AliasCrudVm alias;
            try {
                alias = new AliasCrudVm(await _serviceRO.GetSingleAliasAsync(id, true), _serviceRO);
            } catch (ExpenseTrackerException) {
                return NotFound();
            }
            return View(nameof(Delete), alias);
        }

        /// <summary>
        /// Attempts to remove the specified <see cref="Alias"/>, 
        /// then returns to the <see cref="Payee"/> Index view
        /// POST: Alias/Delete/5
        /// </summary>
        /// <param name="id">The Id of the <see cref="Alias"/> to delete</param>
        /// <returns></returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) {
            await _serviceRO.RemoveAliasAsync(id);
            return RedirectToAction(_payeeIndexRO, nameof(Payee));
        }

        #endregion // Public Actions

        // #region Helper Functions

        // /// <summary>
        // /// Creates a <see cref="SelectList"/> of <see cref="Payee"/> objects and assigns it to ViewData[PayeeList]
        // /// </summary>
        // /// <param name="idToSelect">The Id of the <see cref="Payee"/> to be pre-selected</param>
        // private void CreatePayeeSelectList(int? idToSelect = null) {
        //     ViewData["PayeeList"] = new SelectList(_serviceRO.GetPayees().OrderBy(p => p.Name), "ID", "Name", idToSelect);
        // }

        // #endregion // Helper Functions
    }
}