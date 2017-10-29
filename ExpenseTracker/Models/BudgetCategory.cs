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
        [StringLength(100)]
        public string Name { get; set; }

        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:$#,##0.00}", ApplyFormatInEditMode = false)]
        [Required]
        public double Amount { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime BeginEffectiveDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EndEffectiveDate { get; set; }

        [Required]
        public BudgetType Type { get; set; }

        public ICollection<Payee> Payees { get; set; }
    }
}