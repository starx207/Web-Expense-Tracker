using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Data;
using ExpenseTracker.Models;
using ExpenseTracker.Data.Repository;

namespace ExpenseTracker.Controllers
{
    public class TransactionController : Controller
    {
        private readonly IBudget _context;

        public TransactionController(IBudget context)
        {
            _context = context;
        }

        // GET: Transaction
        public async Task<IActionResult> Index()
        {
            var budgetContext = _context.GetTransactions().Include(t => t.OverrideCategory).Include(t => t.PayableTo);
            return View(await budgetContext.ToListAsync());
        }

        // GET: Transaction/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.GetTransactions()
                .Include(t => t.OverrideCategory)
                .Include(t => t.PayableTo)
                .SingleOrDefaultAsync(m => m.ID == id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // GET: Transaction/Create
        public IActionResult Create()
        {
            ViewData["OverrideCategoryID"] = new SelectList(_context.GetCategories(), "ID", "Name");
            ViewData["PayeeID"] = new SelectList(_context.GetPayees(), "ID", "Name");
            return View();
        }

        // POST: Transaction/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Date,Amount,OverrideCategoryID,PayeeID")] Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                _context.AddTransaction(transaction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["OverrideCategoryID"] = new SelectList(_context.GetCategories(), "ID", "Name", transaction.OverrideCategoryID);
            ViewData["PayeeID"] = new SelectList(_context.GetPayees(), "ID", "Name", transaction.PayeeID);
            return View(transaction);
        }

        // GET: Transaction/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.GetTransactions().SingleOrDefaultAsync(m => m.ID == id);
            if (transaction == null)
            {
                return NotFound();
            }
            ViewData["OverrideCategoryID"] = new SelectList(_context.GetCategories(), "ID", "Name", transaction.OverrideCategoryID);
            ViewData["PayeeID"] = new SelectList(_context.GetPayees(), "ID", "Name", transaction.PayeeID);
            return View(transaction);
        }

        // POST: Transaction/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Date,Amount,OverrideCategoryID,PayeeID")] Transaction transaction)
        {
            if (id != transaction.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.UpdateTransaction(transaction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionExists(transaction.ID))
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
            ViewData["OverrideCategoryID"] = new SelectList(_context.GetCategories(), "ID", "Name", transaction.OverrideCategoryID);
            ViewData["PayeeID"] = new SelectList(_context.GetPayees(), "ID", "Name", transaction.PayeeID);
            return View(transaction);
        }

        // GET: Transaction/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.GetTransactions()
                .Include(t => t.OverrideCategory)
                .Include(t => t.PayableTo)
                .SingleOrDefaultAsync(m => m.ID == id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // POST: Transaction/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transaction = await _context.GetTransactions().SingleOrDefaultAsync(m => m.ID == id);
            _context.RemoveTransaction(transaction);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TransactionExists(int id)
        {
            return _context.GetTransactions().Any(e => e.ID == id);
        }
    }
}
