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
    public class BudgetCategoryController : Controller
    {
        private readonly IBudget _context;

        public BudgetCategoryController(IBudget context)
        {
            _context = context;
        }

        // GET: BudgetCategory
        public async Task<IActionResult> Index()
        {
            return View(await _context.GetCategories().ToListAsync());
        }

        // GET: BudgetCategory/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var budgetCategory = await _context.GetCategories()
                .SingleOrDefaultAsync(m => m.ID == id);
            if (budgetCategory == null)
            {
                return NotFound();
            }

            return View(budgetCategory);
        }

        // GET: BudgetCategory/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BudgetCategory/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,Amount,BeginEffectiveDate,EndEffectiveDate,Type")] BudgetCategory budgetCategory)
        {
            if (ModelState.IsValid)
            {
                //_context.Add(budgetCategory);
                _context.AddBudgetCategory(budgetCategory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(budgetCategory);
        }

        // GET: BudgetCategory/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var budgetCategory = await _context.GetCategories().SingleOrDefaultAsync(m => m.ID == id);
            if (budgetCategory == null)
            {
                return NotFound();
            }
            return View(budgetCategory);
        }

        // POST: BudgetCategory/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,Amount,BeginEffectiveDate,EndEffectiveDate,Type")] BudgetCategory budgetCategory)
        {
            throw new NotImplementedException();
            // if (id != budgetCategory.ID)
            // {
            //     return NotFound();
            // }

            // if (ModelState.IsValid)
            // {
            //     try
            //     {
            //         _context.Update(budgetCategory);
            //         await _context.SaveChangesAsync();
            //     }
            //     catch (DbUpdateConcurrencyException)
            //     {
            //         if (!BudgetCategoryExists(budgetCategory.ID))
            //         {
            //             return NotFound();
            //         }
            //         else
            //         {
            //             throw;
            //         }
            //     }
            //     return RedirectToAction(nameof(Index));
            // }
            //return View(budgetCategory);
        }

        // GET: BudgetCategory/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var budgetCategory = await _context.GetCategories()
                .SingleOrDefaultAsync(m => m.ID == id);
            if (budgetCategory == null)
            {
                return NotFound();
            }

            return View(budgetCategory);
        }

        // POST: BudgetCategory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var budgetCategory = await _context.GetCategories().SingleOrDefaultAsync(m => m.ID == id);
            _context.RemoveBudgetCategory(budgetCategory);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BudgetCategoryExists(int id)
        {
            return _context.GetCategories().Any(e => e.ID == id);
        }
    }
}
