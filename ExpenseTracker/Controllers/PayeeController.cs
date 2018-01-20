using ExpenseTracker.Exceptions;
using ExpenseTracker.Models;
using ExpenseTracker.Repository;
using ExpenseTracker.Repository.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Threading.Tasks;

namespace ExpenseTracker.Controllers
{
    public class PayeeController : Controller
    {
        private readonly IPayeeRepo _context;

        public PayeeController(IDataRepo context) => _context = context;

        // GET: Payee
        public async Task<IActionResult> Index() {
            return View(nameof(Index), await _context.GetOrderedPayeeQueryable(orderBy: nameof(Payee.Name), includeAll: true).Extension().ToListAsync());
        }

        // GET: Payee/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var payee = await _context.GetSinglePayeeAsync(id, true);
            if (payee == null) {
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
                await _context.AddPayeeAsync(payee);
                return RedirectToAction(nameof(Index));
            }
            CreateCategorySelectList(payee);
            return View(nameof(Create), payee);
        }

        // GET: Payee/Edit/5
        public async Task<IActionResult> Edit(int? id) {
            var payee = await _context.GetSinglePayeeAsync(id);
            if (payee == null) {
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
                    await _context.UpdatePayeeAsync(id, payee);
                }
                catch (Exception ex) {
                    if (ex is IdMismatchException || (ex is ConcurrencyException && (!_context.PayeeExists(payee.ID)))) {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            CreateCategorySelectList(payee);
            return View(nameof(Edit), payee);
        }

        // GET: Payee/Delete/5
        public async Task<IActionResult> Delete(int? id) {
            var payee = await _context.GetSinglePayeeAsync(id, true);
            if (payee == null) {
                return NotFound();
            }
            return View(nameof(Delete), payee);
        }

        // POST: Payee/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) {
            await _context.RemovePayeeAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private void CreateCategorySelectList(Payee payeeToSelect = null) {
            ViewData["CategoryList"] = new SelectList(_context.GetOrderedCategoryQueryable(nameof(BudgetCategory.Name)), "ID", "Name", payeeToSelect == null ? null : payeeToSelect.BudgetCategoryID);
        }
    }
}
