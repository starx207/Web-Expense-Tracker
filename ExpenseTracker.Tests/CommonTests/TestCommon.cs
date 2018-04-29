using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExpenseTracker.TestResources
{
    public abstract class TestCommon
    {
        #region Assertion Helpers

        /// <summary>
        /// Performs standard Assertions to check that a Select List is passed to ViewData correctly
        /// </summary>
        /// <param name="viewData">The View Data that is expected to contain a Select List</param>
        /// <param name="viewDataKey">The key value to get the Select List from the View Data</param>
        /// <param name="expectedValues">The string values expected for the Select List values</param>
        /// <param name="selectedValue">The value that should be selected in the Select List by default. Null, if no default value</param>
        protected void AssertThatViewDataIsSelectList(ViewDataDictionary viewData, string viewDataKey, IQueryable<string> expectedValues, string selectedValue = null) {
            // Check viewData key is not null
            Assert.IsNotNull(viewData[viewDataKey], $"ViewData.{viewDataKey} should not be null");
            
            // Check that all expected items are in list
            SelectList list = (SelectList)viewData[viewDataKey];
            int expectedCount = expectedValues.Count();

            Assert.AreEqual(expectedCount, list.Count(), "There are missing items in the select list");

            // Check all items in list are correct
            string errorMsg = "The following IDs are missing from the Select List: ";
            int missingItems = 0;
            foreach (var item in expectedValues) {
                if (list.Where(i => i.Value == item.ToString()).FirstOrDefault() == null) {
                    missingItems += 1;
                    errorMsg += item.ToString() + ", ";
                }
            }
            errorMsg = errorMsg.Substring(0, errorMsg.Length - 2);

            Assert.AreEqual(0, missingItems, errorMsg);

            // Check that the correct item is pre-selected
            if (selectedValue != null) {
                bool isSelected = list.Where(i => i.Value == selectedValue).FirstOrDefault().Selected;

                Assert.IsTrue(isSelected, $"The SelectList item with value = '{selectedValue}' is not selected");
            }
        }

        #endregion // Assertion Helpers
    }
}