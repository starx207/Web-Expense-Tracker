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
            return View(nameof(Index), await _context.GetCategories().ToListAsync());
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

            return View(nameof(Details), budgetCategory);
        }

        // GET: BudgetCategory/Create
        public IActionResult Create()
        {
            return View(nameof(Create));
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
                _context.AddBudgetCategory(budgetCategory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(nameof(Create) ,budgetCategory);
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
            return View(nameof(Edit) ,budgetCategory);
        }

        // POST: BudgetCategory/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,Amount,BeginEffectiveDate,EndEffectiveDate,Type")] BudgetCategory budgetCategory)
        {
            throw new NotImplementedException("There is not yet a method for updating a BudgetCategory");
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
            //return View(nameof(Edit), budgetCategory);
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

            return View(nameof(Delete), budgetCategory);
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
