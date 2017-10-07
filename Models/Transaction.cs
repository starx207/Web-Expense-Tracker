using System;
using System.ComponentModel.DataAnnotations;

namespace Web_Expense_Tracker.Models
{
    public class Transaction
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public double Amount { get; set; }

        [Required]
        [StringLength(100)]
        public string Payee { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
    }
}