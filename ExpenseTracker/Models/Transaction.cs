using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseTracker.Models
{
    public class Transaction
    {
        [Key]
        public int ID { get; set; }

        [DataType(DataType.Date)]
        [Required]
        public DateTime Date { get; set; }

        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:$#,##0.00}", ApplyFormatInEditMode = false)]
        [Required]
        public double Amount { get; set; }

        [ForeignKey("OverrideCategory")]
        public int? OverrideCategoryID { get; set; }

        public int? PayeeID { get; set; }

        [Display(Name = "Payable To")]
        public Payee PayableTo { get; set; }

        [Display(Name = "Override Category")]
        public BudgetCategory OverrideCategory { get; set; }
    }
}