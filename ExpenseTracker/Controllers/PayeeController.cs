using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Repository;
using ExpenseTracker.Services;
using ExpenseTracker.Models;
using ExpenseTracker.Exceptions;

namespace ExpenseTracker.Controllers
{
    public class PayeeController : Controller
    {
        private readonly IBudgetService _context;

        public PayeeController(IBudgetService context)
        {
            _context = context;
        }

        // GET: Payee
        public async Task<IActionResult> Index()
        {
            var budgetContext = _context.GetPayees()
                .Include(p => p.Category)
                .Include(p => p.Aliases)
                .OrderBy(p => p.Name);
            return View(nameof(Index), await budgetContext.ToListAsync());
        }

        // GET: Payee/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var payee = await GetPayeeById(id, true);
            if (payee == null)
            {
                return NotFound();
            }

            return View(nameof(Details), payee);
        }

        // GET: Payee/Create
        public IActionResult Create()
        {
            CreateCategorySelectList();
            return View(nameof(Create));
        }

        // POST: Payee/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,BeginEffectiveDate,EndEffectiveDate,BudgetCategoryID")] Payee payee)
        {
            if (ModelState.IsValid)
            {
                await _context.AddPayeeAsync(payee);
                return RedirectToAction(nameof(Index));
            }
            CreateCategorySelectList(payee);
            return View(nameof(Create), payee);
        }

        // GET: Payee/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var payee = await GetPayeeById(id);
            if (payee == null)
            {
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
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,BeginEffectiveDate,EndEffectiveDate,BudgetCategoryID")] Payee payee)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _context.UpdatePayeeAsync(id, payee);
                }
                catch (Exception ex) {
                    if (ex is IdMismatchException || (ex is DbUpdateConcurrencyException && (!PayeeExists(payee.ID)))) {
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
        public async Task<IActionResult> Delete(int? id)
        {
            var payee = await GetPayeeById(id, true);
            if (payee == null)
            {
                return NotFound();
            }

            return View(nameof(Delete), payee);
        }

        // POST: Payee/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _context.RemovePayeeAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private bool PayeeExists(int id)
        {
            return _context.GetPayees().Any(e => e.ID == id);
        }

        private async Task<Payee> GetPayeeById(int? id, bool includeCategory = false) {
            if (id == null) { return null; }

            var payee = _context.GetPayees().Where(p => p.ID == id);
            
            if (includeCategory) {
                payee = payee.Include(p => p.Category);
            }

            return await payee.SingleOrDefaultAsync();
        }

        private void CreateCategorySelectList(Payee payeeToSelect = null) {
            ViewData["CategoryList"] = new SelectList(_context.GetCategories().OrderBy(c => c.Name), "ID", "Name", payeeToSelect == null ? null : payeeToSelect.BudgetCategoryID);
        }
    }
}
