using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models
{
    public class Alias
    {
        [Key]
        public int ID { get; set; }

        [StringLength(50)]
        [Required]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Payee")]
        public int PayeeID { get; set; }

        [Display(Name = "Payee")]
        public Payee AliasForPayee { get; set; }
    }
}