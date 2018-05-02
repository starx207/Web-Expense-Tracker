using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models
{
    public class AliasCrudVm
    {
        #region Public Properties

        public int AliasId { get; set; }
        public string Name { get; set; }

        [Display(Name = "Payee")]
        public int PayeeID { get; set; }

        #endregion // Public Properties

        #region Constructors

        public AliasCrudVm(Alias alias) {
            AliasId = alias.ID;
            Name = alias.Name;
            PayeeID = alias.PayeeID;
        }

        #endregion // Constructors
    }
}