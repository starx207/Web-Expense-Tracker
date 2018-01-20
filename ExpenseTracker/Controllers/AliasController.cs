using ExpenseTracker.Exceptions;
using ExpenseTracker.Models;
using ExpenseTracker.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Models
{
    public class AliasController : Controller
    {
        private readonly IAliasRepo _context;
        private readonly string payeeIndex = "Index";

        public AliasController(IDataRepo context) => _context = context;

        // GET: Alias/Create
        public IActionResult Create(int? payeeID = null) {
            CreatePayeeSelectList(payeeID);
            return View(nameof(Create));
        }

        //POST: Alias/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,PayeeID")] Alias alias) {
            if (ModelState.IsValid) {
                // var payee = await _context.GetPayees().Where(p => p.ID == alias.PayeeID).SingleOrDefaultAsync();
                // alias.AliasForPayee = payee;
                await _context.AddAliasAsync(alias);
                return RedirectToAction(payeeIndex, nameof(Payee));
            }
            CreatePayeeSelectList(alias.PayeeID);
            return View(nameof(Create), alias);
        }

        // GET: Alias/Edit/5
        public async Task<IActionResult> Edit(int? id) {
            var alias = await _context.GetSingleAliasAsync(id);
            if (alias == null) {
                return NotFound();
            }
            CreatePayeeSelectList(alias.PayeeID);
            return View(nameof(Edit), alias);
        }

        // POST: Payee/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,PayeeID")] Alias alias) {
            if (id != alias.ID) {
                return NotFound();
            }

            if (ModelState.IsValid) {
                try {
                    await _context.UpdateAliasAsync(id, alias);
                } catch (ConcurrencyException) {
                    if (!_context.AliasExists(alias.ID)) {
                        return NotFound();
                    } else {
                        throw;
                    }
                }
                return RedirectToAction(payeeIndex, nameof(Payee));
            }
            CreatePayeeSelectList(alias.PayeeID);
            return View(nameof(Edit), alias);
        }

        // GET: Alias/Delete/5
        public async Task<IActionResult> Delete(int? id) {
             var alias = await _context.GetSingleAliasAsync(id, true);
             if (alias == null) {
                 return NotFound();
             }

             return View(nameof(Delete), alias);
        }

        // POST: Alias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) {
            var alias = await _context.GetSingleAliasAsync(id);
            if (alias != null) {
                await _context.RemoveAliasAsync(id);
            }
            return RedirectToAction(payeeIndex, nameof(Payee));
        }

        private void CreatePayeeSelectList(int? idToSelect = null) {
            ViewData["PayeeList"] = new SelectList(_context.GetOrderedPayees(nameof(Payee.Name)), "ID", "Name", idToSelect);
        }
    }
}