using System.ComponentModel.DataAnnotations;

namespace Web_Expense_Tracker.Models
{
    public class BudgetCategory
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public BudgetType Type { get; set; }

        public double Amount { get; set; }
    }
}