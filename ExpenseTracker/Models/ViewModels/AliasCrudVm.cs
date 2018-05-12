using ExpenseTracker.Services;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ExpenseTracker.Models
{
    public class AliasCrudVm : CrudViewModel
    {
        #region Public Properties

        /// <summary>
        /// The name of the <see cref="Alias"/>
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The name of the <see cref="Payee"/> this <see cref="Alias"/> is assigned to
        /// </summary>
        public string PayeeName { get; set; }

        /// <summary>
        /// A list of <see cref="Payee"/> names and Ids to use in the select list
        /// </summary>
        public List<string> PayeeOptions { get; set; }

        #endregion // Public Properties

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public AliasCrudVm() { }

        /// <summary>
        /// Initialize ViewModel with <see cref="Alias"/> info
        /// </summary>
        /// <param name="alias">The <see cref="Alias"/> to initialize with</param>
        /// <param name="service">The service to get <see cref="Payee"/> list from</param>
        public AliasCrudVm(Alias alias, ICommonService service) {
            PayeeOptions = new List<string>();
            foreach (Payee payee in service.GetPayees().OrderBy(p => p.Name)) {
                PayeeOptions.Add(payee.Name);
            }

            if (alias == null) { return; }

            NavId = alias.ID;
            Name = alias.Name;
            PayeeName = alias.AliasForPayee?.Name;
        }

        public AliasCrudVm(Alias alias) {
            if (alias == null) { return; }
            NavId = alias.ID;
            Name = alias.Name;
            PayeeName = alias.AliasForPayee?.Name;
            PayeeOptions = new List<string>();
        }

        #endregion // Constructors
    }
}