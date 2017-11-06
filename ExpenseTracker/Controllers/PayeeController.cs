using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Data.Repository;
using ExpenseTracker.Models;

namespace ExpenseTracker.Controllers
{
    public class PayeeController : Controller
    {
        private readonly IBudget _context;

        public PayeeController(IBudget context)
        {
            _context = context;
        }

        // GET: Payee
        public async Task<IActionResult> Index()
        {
            var budgetContext = _context.GetPayees().Include(p => p.Category);
            return View(nameof(Index), await budgetContext.ToListAsync());
        }

        // GET: Payee/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payee = await _context.GetPayees()
                .Include(p => p.Category)
                .SingleOrDefaultAsync(m => m.ID == id);
            if (payee == null)
            {
                return NotFound();
            }

            return View(nameof(Details), payee);
        }

        // GET: Payee/Create
        public IActionResult Create()
        {
            ViewData["BudgetCategoryID"] = new SelectList(_context.GetCategories(), "ID", "Name");
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
                _context.AddPayee(payee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BudgetCategoryID"] = new SelectList(_context.GetCategories(), "ID", "Name", payee.BudgetCategoryID);
            return View(nameof(Create), payee);
        }

        // GET: Payee/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payee = await _context.GetPayees().SingleOrDefaultAsync(m => m.ID == id);
            if (payee == null)
            {
                return NotFound();
            }
            ViewData["BudgetCategoryID"] = new SelectList(_context.GetCategories(), "ID", "Name", payee.BudgetCategoryID);
            return View(nameof(Edit), payee);
        }

        // POST: Payee/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,BeginEffectiveDate,EndEffectiveDate,BudgetCategoryID")] Payee payee)
        {
            if (id != payee.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.UpdatePayee(payee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PayeeExists(payee.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["BudgetCategoryID"] = new SelectList(_context.GetCategories(), "ID", "Name", payee.BudgetCategoryID);
            return View(nameof(Edit), payee);
        }

        // GET: Payee/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payee = await _context.GetPayees()
                .Include(p => p.Category)
                .SingleOrDefaultAsync(m => m.ID == id);
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
            var payee = await _context.GetPayees().SingleOrDefaultAsync(m => m.ID == id);
            _context.RemovePayee(payee);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PayeeExists(int id)
        {
            return _context.GetPayees().Any(e => e.ID == id);
        }
    }
}
