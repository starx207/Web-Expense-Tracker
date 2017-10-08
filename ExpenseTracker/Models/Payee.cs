using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models
{
    public class Payee
    {
        [Key]
        public int ID { get; set; }

        [StringLength(50)]
        [Required]
        public string Name { get; set; }

        public int? BudgetCategoryID { get; set; }

        public BudgetCategory Category { get; set; }

        public ICollection<Transaction> Transactions { get; set; }
    }
}