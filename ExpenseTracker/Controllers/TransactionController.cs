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
    public class TransactionController : Controller
    {
        private readonly ITransactionManagerService _service;

        public TransactionController(ITransactionManagerService service) => _service = service;

        // GET: Transaction
        public async Task<IActionResult> Index() {
            return View(nameof(Index), await _service.GetTransactions(true, true).OrderByDescending(t => t.Date).Extension().ToListAsync());
        }

        // GET: Transaction/Details/5
        public async Task<IActionResult> Details(int? id) {
            Transaction transaction;
            try {
                transaction = await _service.GetSingleTransactionAsync(id, true);
            } catch (Exception ex) {
                if (ex is NullIdException || ex is IdNotFoundException) {
                    return NotFound();
                }
                throw;
            }
            return View(nameof(Details), transaction);
        }

        // GET: Transaction/Create
        public IActionResult Create() {
            PopulateSelectLists();
            return View(nameof(Create));
        }

        // POST: Transaction/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Date,Amount,OverrideCategoryID,PayeeID")] Transaction transaction) {
            if (ModelState.IsValid) {
                await _service.AddTransactionAsync(transaction);
                return RedirectToAction(nameof(Index));
            }
            PopulateSelectLists(transaction.OverrideCategoryID, transaction.PayeeID);
            return View(nameof(Create), transaction);
        }

        // GET: Transaction/Edit/5
        public async Task<IActionResult> Edit(int? id) {
            Transaction transaction;
            try {
                transaction = await _service.GetSingleTransactionAsync(id);
            } catch (Exception ex) {
                if (ex is NullIdException || ex is IdNotFoundException) {
                    return NotFound();
                }
                throw;
            }
            PopulateSelectLists(transaction.OverrideCategoryID, transaction.PayeeID);
            return View(nameof(Edit), transaction);
        }

        // POST: Transaction/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Date,Amount,OverrideCategoryID,PayeeID")] Transaction transaction) {
            if (ModelState.IsValid) {
                try {
                    await _service.UpdateTransactionAsync(id, transaction);
                } catch (Exception ex) {
                    if (ex is IdMismatchException || (ex is ConcurrencyException && (!_service.TransactionExists(transaction.ID)))) {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            PopulateSelectLists(transaction.OverrideCategoryID, transaction.PayeeID);
            return View(nameof(Edit), transaction);
        }

        // GET: Transaction/Delete/5
        public async Task<IActionResult> Delete(int? id) {
            Transaction transaction;
            try {
                transaction = await _service.GetSingleTransactionAsync(id, true);
            } catch (Exception ex) {
                if (ex is NullIdException || ex is IdNotFoundException) {
                    return NotFound();
                }
                throw;
            }
            return View(nameof(Delete), transaction);
        }

        // POST: Transaction/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) {
            await _service.RemoveTransactionAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private void PopulateSelectLists(int? selectedCategoryID = null, int? selectedPayeeID = null) {
            ViewData["CategoryList"] = new SelectList(_service.GetCategories().OrderBy(c => c.Name), "ID", "Name", selectedCategoryID);
            ViewData["PayeeList"] = new SelectList(_service.GetPayees().OrderBy(p => p.Name), "ID", "Name", selectedPayeeID);
        }
    }
}
