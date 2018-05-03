using System;
using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models
{
    public class CategoryCrudVm
    {
        #region Public Properties

        public int NavId { get; private set; }

        [Display(Name = "Effective From")]
        public DateTime EffectiveFrom { get; set; }

        public string Name { get; set; }

        [DisplayFormat(DataFormatString = "{0:$#,##0.00}", ApplyFormatInEditMode = false)]
        public double Amount { get; set; }

        public BudgetType Type { get; set; }

        #endregion // Public Properties

        #region Constructors

        public CategoryCrudVm() { }

        public CategoryCrudVm(BudgetCategory category) {
            NavId = category.ID;
            EffectiveFrom = category.EffectiveFrom;
            Name = category.Name;
            Amount = category.Amount;
            Type = category.Type;
        }

        #endregion // Constructors
    }
}