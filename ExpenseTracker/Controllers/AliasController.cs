using ExpenseTracker.Repository;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Models
{
    public class AliasController : Controller
    {
        private readonly IBudget _context;
        private readonly string payeeIndex = "Index";

        public AliasController(IBudget context) {
            _context = context;
        }

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
                _context.AddAlias(alias);
                await _context.SaveChangesAsync();
                return RedirectToAction(payeeIndex, nameof(Payee));
            }
            CreatePayeeSelectList(alias.PayeeID);
            return View(nameof(Create), alias);
        }

        // GET: Alias/Edit/5
        public async Task<IActionResult> Edit(int? id) {
            var alias = await GetAliasById(id);
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
                    _context.UpdateAlias(alias);
                    await _context.SaveChangesAsync();
                } catch (DbUpdateConcurrencyException) {
                    if (!AliasExists(alias.ID)) {
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
             var alias = await GetAliasById(id, true);
             if (alias == null) {
                 return NotFound();
             }

             return View(nameof(Delete), alias);
        }

        // POST: Alias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) {
            var alias = await GetAliasById(id);
            if (alias != null) {
                _context.RemoveAlias(alias);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(payeeIndex, nameof(Payee));
        }

        private bool AliasExists(int id) {
            return _context.GetAliases().Any(a => a.ID == id);
        }

        private void CreatePayeeSelectList(int? idToSelect = null) {
            ViewData["PayeeList"] = new SelectList(_context.GetPayees().OrderBy(p => p.Name), "ID", "Name", idToSelect);
        }

        private async Task<Alias> GetAliasById(int? id, bool includePayee = false) {
            if (id == null) { return null; }
            
            var alias = _context.GetAliases().Where(a => a.ID == id);
            if (includePayee) {
                alias = alias.Include(a => a.AliasForPayee);
            }
            return await alias.SingleOrDefaultAsync();
        }
    }
}