using System;
using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models
{
    public abstract class DbCommon
    {
        [Key]
        public int ID { get; set; }
        
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Effective From")]
        public DateTime BeginEffectiveDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Effective To")]
        public DateTime? EndEffectiveDate { get; set; }
    }
}