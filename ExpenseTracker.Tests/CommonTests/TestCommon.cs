using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExpenseTracker.Tests
{
    public abstract class TestCommon
    {
        protected void AssertThatViewDataIsSelectList(ViewDataDictionary viewData, string viewDataKey, IQueryable<string> expectedValues, string selectedValue = null) {
            
            Assert.IsNotNull(viewData[viewDataKey], $"ViewData.{viewDataKey} should not be null");
            
            SelectList list = (SelectList)viewData[viewDataKey];
            int expectedCount = expectedValues.Count();

            Assert.AreEqual(expectedCount, list.Count(), "There are missing items in the select list");

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

            if (selectedValue != null) {
                bool isSelected = list.Where(i => i.Value == selectedValue).FirstOrDefault().Selected;

                Assert.IsTrue(isSelected, $"The SelectList item with value = '{selectedValue}' is not selected");
            }
        }
    }
}