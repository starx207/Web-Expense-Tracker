using ExpenseTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Data
{
    public class BudgetContext : DbContext
    {
        public BudgetContext(DbContextOptions<BudgetContext> options) : base(options) { }

        public virtual DbSet<BudgetCategory> BudgetCategories { get; set; }
        public virtual DbSet<Payee> Payees { get; set; }
        public virtual DbSet<Transaction> Transactions { get; set; }
        public virtual DbSet<Alias> Aliases { get; set; }
    }
}