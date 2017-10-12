namespace ExpenseTracker.Data.Repository
{
    public class Budget : IBudget
    {
        private readonly IBudgetAccess _repo;

        public Budget(IBudgetAccess repository) {
            _repo = repository;
        }


    }
}