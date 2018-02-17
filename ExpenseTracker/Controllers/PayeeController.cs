using ExpenseTracker.Exceptions;
using ExpenseTracker.Models;
using ExpenseTracker.Repository;
using ExpenseTracker.Repository.Extensions;
using ExpenseTracker.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Controllers
{
    public class PayeeController : Controller
    {
        private readonly IPayeeManagerService _service;

        public PayeeController(IPayeeManagerService service) => _service = service;

        // GET: Payee
        public async Task<IActionResult> Index() {
            return View(nameof(Index), await _service.GetPayees(true, true).OrderBy(p => p.Name).Extension().ToListAsync());
        }

        // GET: Payee/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            Payee payee;
            try {
                payee = await _service.GetSinglePayeeAsync(id, true);
            } catch (ExpenseTrackerException) {
                return NotFound();
            }
            return View(nameof(Details), payee);
        }

        // GET: Payee/Create
        public IActionResult Create() {
            CreateCategorySelectList();
            return View(nameof(Create));
        }

        // POST: Payee/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,BeginEffectiveDate,EndEffectiveDate,BudgetCategoryID")] Payee payee) {
            if (ModelState.IsValid) {
                await _service.AddPayeeAsync(payee);
                return RedirectToAction(nameof(Index));
            }
            CreateCategorySelectList(payee);
            return View(nameof(Create), payee);
        }

        // GET: Payee/Edit/5
        public async Task<IActionResult> Edit(int? id) {
            Payee payee;
            try {
                payee = await _service.GetSinglePayeeAsync(id);
            } catch (ExpenseTrackerException) {
                return NotFound();
            }
            CreateCategorySelectList(payee);
            return View(nameof(Edit), payee);
        }

        // POST: Payee/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,BeginEffectiveDate,EndEffectiveDate,BudgetCategoryID")] Payee payee) {
            if (ModelState.IsValid) {
                try {
                    await _service.UpdatePayeeAsync(id, payee);
                } catch (ExpenseTrackerException ex) {
                    if (ex is ConcurrencyException && _service.PayeeExists(payee.ID)) {
                        throw;
                    }
                    return NotFound();
                }
                return RedirectToAction(nameof(Index));
            }
            CreateCategorySelectList(payee);
            return View(nameof(Edit), payee);
        }

        // GET: Payee/Delete/5
        public async Task<IActionResult> Delete(int? id) {
            Payee payee;
            try {
                payee = await _service.GetSinglePayeeAsync(id, true);
            } catch (ExpenseTrackerException) {
                return NotFound();
            }
            return View(nameof(Delete), payee);
        }

        // POST: Payee/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) {
            await _service.RemovePayeeAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private void CreateCategorySelectList(Payee payeeToSelect = null) {
            ViewData["CategoryList"] = new SelectList(_service.GetCategories().OrderBy(c => c.Name), "ID", "Name", payeeToSelect == null ? null : payeeToSelect.BudgetCategoryID);
        }
    }
}
