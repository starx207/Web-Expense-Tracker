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
        public int PayeeID { get; set; }

        [Required]
        public Payee AliasForPayee { get; set; }
    }
}