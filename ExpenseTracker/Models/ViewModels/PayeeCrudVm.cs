using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ExpenseTracker.Services;

namespace ExpenseTracker.Models
{
    public class PayeeCrudVm : CrudViewModel
    {
        #region Public Properties

        [Display(Name = "Effective From")]
        [DisplayFormat(DataFormatString = "MM/dd/yyyy", ApplyFormatInEditMode=true)]
        public DateTime EffectiveFrom { get; set; }

        public string Name { get; set; }

        public string CategoryName { get; set; }

        public List<string> CategoryOptions { get; set; }

        public List<AliasCrudVm> Aliases { get; set; }

        #endregion // Public Properties

        #region Constructors

        public PayeeCrudVm() { }

        public PayeeCrudVm(Payee payee, ICommonService service) {
            CategoryOptions = new List<string>();
            foreach (var category in service.GetCategories()) {
                CategoryOptions.Add(category.Name);
            }
            CategoryOptions.Sort();

            if (payee == null) { return; }

            NavId = payee.ID;
            EffectiveFrom = payee.EffectiveFrom;
            Name = payee.Name;
            CategoryName = payee.Category?.Name;

            Aliases = new List<AliasCrudVm>();
            foreach (var alias in payee.Aliases) {
                Aliases.Add(new AliasCrudVm(alias));
            }
        }

        #endregion // Constructors
    }
}