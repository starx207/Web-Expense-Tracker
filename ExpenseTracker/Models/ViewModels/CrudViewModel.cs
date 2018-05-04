namespace ExpenseTracker.Models
{
    public abstract class CrudViewModel : ICrudViewModel
    {
        public int NavId { get; set; }
    }
}