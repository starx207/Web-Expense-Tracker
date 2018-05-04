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
        /// The Id of the <see cref="Payee"/> this <see cref="Alias"/> is assigned to
        /// </summary>
        public int? PayeeID { get; set; }

        /// <summary>
        /// The name of the <see cref="Payee"/> this <see cref="Alias"/> is assigned to
        /// </summary>
        public string PayeeName { get; set; }

        /// <summary>
        /// A list of <see cref="Payee"/> names and Ids to use in the select list
        /// </summary>
        public List<Payee> PayeeOptions { get; set; }

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
            PayeeOptions = new List<Payee>();
            foreach (Payee payee in service.GetPayees().OrderBy(p => p.Name)) {
                PayeeOptions.Add(payee);
            }

            if (alias == null) { return; }

            NavId = alias.ID;
            Name = alias.Name;
            PayeeID = alias.PayeeID;
            PayeeName = alias.Name;
        }

        /// <summary>
        /// Initialize an empty ViewModel with a populated <see cref="Payee"/> list
        /// </summary>
        /// <param name="service">The service to get <see cref="Payee"/> list from</param>
        /// <returns></returns>
        public AliasCrudVm(IAliasManagerService service) : this(null, service) { }

        #endregion // Constructors
    }
}