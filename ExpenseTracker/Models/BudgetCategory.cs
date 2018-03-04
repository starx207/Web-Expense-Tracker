using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models
{
    public enum BudgetType {
        Income, Expense
    }

    public class BudgetCategory
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Effective From")]
        public DateTime EffectiveFrom { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:$#,##0.00}", ApplyFormatInEditMode = false)]
        [Required]
        public double Amount { get; set; }

        [Required]
        public virtual BudgetType Type { get; set; }

        public virtual ICollection<Payee> Payees { get; set; }
    }
}