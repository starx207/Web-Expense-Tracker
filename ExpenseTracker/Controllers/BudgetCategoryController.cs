using ExpenseTracker.Exceptions;
using ExpenseTracker.Models;
using ExpenseTracker.Repository;
using ExpenseTracker.Repository.Extensions;
using ExpenseTracker.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Controllers
{
    public class BudgetCategoryController : Controller
    {
        private readonly ICategoryManagerService _service;

        public BudgetCategoryController(ICategoryManagerService service) => _service = service;

        // GET: BudgetCategory
        public async Task<IActionResult> Index() {
            return View(nameof(Index), await _service.GetCategories().OrderBy(c => c.Name).Extension().ToListAsync());
        }

        // GET: BudgetCategory/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            BudgetCategory budgetCategory;
            try {
                budgetCategory = await _service.GetSingleCategoryAsync(id);
            } catch (Exception ex) {
                if (ex is IdNotFoundException || ex is NullIdException) {
                    return NotFound();
                }
                throw;
            }
            return View(nameof(Details), budgetCategory);
        }

        // GET: BudgetCategory/Create
        public IActionResult Create() => View(nameof(Create));

        // POST: BudgetCategory/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,Amount,EffectiveFrom,Type")] BudgetCategory budgetCategory)
        {
            if (ModelState.IsValid)
            {
                try {
                    await _service.AddCategoryAsync(budgetCategory);
                    return RedirectToAction(nameof(Index));
                } catch (UniqueConstraintViolationException) {
                    ModelState.AddModelError(nameof(BudgetCategory.Name), "Name already in use by another Budget Category");
                }
                
            }
            return View(nameof(Create), budgetCategory);
        }

        // GET: BudgetCategory/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            BudgetCategory budgetCategory;
            try {
                budgetCategory = await _service.GetSingleCategoryAsync(id);
            } catch (ExpenseTrackerException) {
                return NotFound();
            }
            SetEffectiveFromViewBag();
            return View(nameof(Edit) ,budgetCategory);
        }

        // POST: BudgetCategory/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,Amount,EffectiveFrom,Type")] BudgetCategory budgetCategory)
        {
            string effectiveDateError = "";
            if (ModelState.IsValid) {
                try {
                    await _service.UpdateCategoryAsync(id, budgetCategory);
                    return RedirectToAction(nameof(Index));
                } catch (InvalidDateExpection dteex) {
                    effectiveDateError = dteex.Message;
                } catch (ExpenseTrackerException ex) {
                    if (ex is ConcurrencyException && (_service.CategoryExists(budgetCategory.ID))) {
                        throw;
                    }
                    return NotFound();
                }
            }
            SetEffectiveFromViewBag(effectiveDateError);
            return View(nameof(Edit), budgetCategory);
        }

        // GET: BudgetCategory/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            BudgetCategory budgetCategory;
            try {
                budgetCategory = await _service.GetSingleCategoryAsync(id);
            } catch (Exception ex) {
                if (ex is IdNotFoundException || ex is NullIdException) {
                    return NotFound();
                }
                throw;
            }

            return View(nameof(Delete), budgetCategory);
        }

        // POST: BudgetCategory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _service.RemoveCategoryAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private void SetEffectiveFromViewBag(string message = "") {
            @ViewBag.EffectiveFromError = message;
        }
    }
}
