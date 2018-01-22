using ExpenseTracker.Exceptions;
using ExpenseTracker.Models;
using ExpenseTracker.Repository;
using ExpenseTracker.Repository.Extensions;
using ExpenseTracker.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace ExpenseTracker.Controllers
{
    public class TransactionController : Controller
    {
        private readonly ITransactionManagerService _service;

        public TransactionController(IBudgetRepo repo) => _service = new TransactionManagerService(repo);

        // GET: Transaction
        public async Task<IActionResult> Index() {
            return View(nameof(Index), await _service.GetOrderedTransactions(orderBy: nameof(Transaction.Date), orderByDescending: true, includeAll: true).Extension().ToListAsync());
        }

        // GET: Transaction/Details/5
        public async Task<IActionResult> Details(int? id) {
            var transaction = await _service.GetSingleTransactionAsync(id, true);
            if (transaction == null) {
                return NotFound();
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
            var transaction = await _service.GetSingleTransactionAsync(id);
            if (transaction == null) {
                return NotFound();
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
            if (id != transaction.ID) {
                return NotFound();
            }

            if (ModelState.IsValid) {
                try {
                    await _service.UpdateTransactionAsync(id, transaction);
                }
                catch (ConcurrencyException) {
                    if (!_service.TransactionExists(transaction.ID)) {
                        return NotFound();
                    }
                    else {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            PopulateSelectLists(transaction.OverrideCategoryID, transaction.PayeeID);
            return View(nameof(Edit), transaction);
        }

        // GET: Transaction/Delete/5
        public async Task<IActionResult> Delete(int? id) {
            var transaction = await _service.GetSingleTransactionAsync(id, true);
            if (transaction == null) {
                return NotFound();
            }

            return View(nameof(Delete), transaction);
        }

        // POST: Transaction/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) {
            var transaction = await _service.GetSingleTransactionAsync(id);
            if (transaction != null) {
                await _service.RemoveTransactionAsync(id);
            }
            return RedirectToAction(nameof(Index));
        }

        private void PopulateSelectLists(int? selectedCategoryID = null, int? selectedPayeeID = null) {
            ViewData["CategoryList"] = new SelectList(_service.GetOrderedCategories(nameof(BudgetCategory.Name)), "ID", "Name", selectedCategoryID);
            ViewData["PayeeList"] = new SelectList(_service.GetOrderedPayees(nameof(Payee.Name)), "ID", "Name", selectedPayeeID);
        }
    }
}
