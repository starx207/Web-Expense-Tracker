namespace ExpenseTracker.Data.Repository
{
    public class Budget : IBudget
    {
        private readonly BudgetContext _context;

        public Budget(BudgetContext context) {
            _context = context;
        }


    }
}