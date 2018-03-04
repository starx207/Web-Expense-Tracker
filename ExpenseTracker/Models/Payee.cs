using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models
{
    public class Payee
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Effective From")]
        public DateTime EffectiveFrom { get; set; }

        [StringLength(50)]
        [Required]
        public string Name { get; set; }

        public int? BudgetCategoryID { get; set; }

        public virtual BudgetCategory Category { get; set; }

        public virtual ICollection<Transaction> Transactions { get; set; }

        public virtual ICollection<Alias> Aliases { get; set; }
    }
}