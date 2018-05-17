using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ExpenseTracker.Services;

namespace ExpenseTracker.Models
{
    public class PayeeCrudVm : CrudViewModel
    {
        #region Private Members

        private List<string> _categoryOptions;

        #endregion // Private Members

        #region Public Properties

        [Display(Name = "Effective From")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode=true)]
        public DateTime EffectiveFrom { get; set; }

        public string Name { get; set; }

        [Display(Name = "Budget Category")]
        public string CategoryName { get; set; }

        public List<string> CategoryOptions { 
            get => _categoryOptions;
            set {
                _categoryOptions = value;
                _categoryOptions.Sort();
            }
        }

        public List<AliasCrudVm> Aliases { get; set; }

        #endregion // Public Properties

        #region Constructors

        public PayeeCrudVm() { }

        public PayeeCrudVm(Payee payee, List<string> allCategoryNames) {
            InitializeViewModel(payee, allCategoryNames);
        }

        public PayeeCrudVm(Payee payee, ICommonService service) {
            InitializeViewModel(payee, service.GetCategories().Select(c => c.Name).ToList());
        }

        #endregion // Constructors

        #region Private Helpers

        private void InitializeViewModel(Payee payee, List<string> allCategoryNames) {
            CategoryOptions = allCategoryNames;

            if (payee == null) { return; }

            NavId = payee.ID;
            EffectiveFrom = payee.EffectiveFrom;
            Name = payee.Name;
            CategoryName = payee.Category?.Name;

            Aliases = new List<AliasCrudVm>();
            if (payee.Aliases == null) { return; }
            
            foreach (var alias in payee.Aliases) {
                Aliases.Add(new AliasCrudVm(alias));
            }
        }

        #endregion // Private Helpers
    }
}