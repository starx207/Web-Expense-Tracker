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
  public class AliasController : Controller
    {
        private readonly IAliasManagerService _service;
        private readonly string payeeIndex = "Index";

        public AliasController(IAliasManagerService service) => _service = service;

        // GET: Alias/Create
        public IActionResult Create(int? payeeID = null) {
            CreatePayeeSelectList(payeeID);
            return View(nameof(Create));
        }

        //POST: Alias/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,PayeeID")] Alias alias) {
            if (ModelState.IsValid) {
                try {
                    await _service.AddAliasAsync(alias);
                    return RedirectToAction(payeeIndex, nameof(Payee));
                } catch (UniqueConstraintViolationException) {
                    ModelState.AddModelError(nameof(Alias.Name), "Name already in use by another Alias");
                }
            }
            CreatePayeeSelectList(alias.PayeeID);
            return View(nameof(Create), alias);
        }

        // GET: Alias/Edit/5
        public async Task<IActionResult> Edit(int? id) {
            Alias alias;
            try {
                alias = await _service.GetSingleAliasAsync(id);
            } catch (ExpenseTrackerException) {
                return NotFound();
            }
            CreatePayeeSelectList(alias.PayeeID);
            return View(nameof(Edit), alias);
        }

        // POST: Payee/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,PayeeID")] Alias alias) {
            if (ModelState.IsValid) {
                try {
                    await _service.UpdateAliasAsync(id, alias);
                    return RedirectToAction(payeeIndex, nameof(Payee));
                } catch (UniqueConstraintViolationException) {
                    ModelState.AddModelError(nameof(Alias.Name), "Name already in use by another Alias");
                } catch (ExpenseTrackerException ex) {
                    if (ex is ConcurrencyException && _service.AliasExists(alias.ID)) {
                        throw;
                    }
                    return NotFound();
                }
            }
            CreatePayeeSelectList(alias.PayeeID);
            return View(nameof(Edit), alias);
        }

        // GET: Alias/Delete/5
        public async Task<IActionResult> Delete(int? id) {
            Alias alias;
            try {
                alias = await _service.GetSingleAliasAsync(id, true);
            } catch (ExpenseTrackerException) {
                return NotFound();
            }
            return View(nameof(Delete), alias);
        }

        // POST: Alias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) {
            await _service.RemoveAliasAsync(id);
            return RedirectToAction(payeeIndex, nameof(Payee));
        }

        private void CreatePayeeSelectList(int? idToSelect = null) {
            ViewData["PayeeList"] = new SelectList(_service.GetPayees().OrderBy(p => p.Name), "ID", "Name", idToSelect);
        }
    }
}