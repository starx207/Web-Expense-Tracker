using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;

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

        #region Mock Helpers

        #region Mock RouteData

        protected void SetupControllerRouteData(Controller controller, string routeKey, object routeValue) {
            SetupControllerRouteData(controller, new Dictionary<string, object> { { routeKey, routeValue } });
        }

        protected void SetupControllerRouteData(Controller controller, Dictionary<string, object> routeValues) {
            SetupControllerContext(controller, routeValues, null);
        }

        #endregion // Mock RouteData

        #region Mock RequestQuery

        protected void SetupControllerRequestQuery(Controller controller, string queryName, object queryValue) {
            SetupControllerRequestQuery(controller, new Dictionary<string, object> { { queryName, queryValue } });
        }

        protected void SetupControllerRequestQuery(Controller controller, Dictionary<string, object> queryValues) {
            SetupControllerContext(controller, null, queryValues);
        }

        #endregion // Mock RequestQuery

        #region Mock HttpContext

        protected void SetupControllerContext(Controller controller, string routeKey, object routeValue, string queryName, object queryValue) {
            SetupControllerContext(controller,
                new Dictionary<string, object> { { routeKey, routeValue } },
                new Dictionary<string, object> { { queryName, queryValue } });
        }

        protected void SetupControllerContext(Controller controller, Dictionary<string, object> routeValues, Dictionary<string, object> queryValues) {
            var queryString = new QueryString();
            if (queryValues != null) {
                foreach (var pair in queryValues) {
                    queryString.Add(pair.Key, pair.Value.ToString());
                }
            }
            var routeData = new RouteData();
            if (routeValues != null) {
                foreach (var pair in routeValues) {
                    routeData.Values.Add(pair.Key, pair.Value);
                }
            }

            var mockHttpContext = new Mock<HttpContext>();
            var mockHttpRequest = new Mock<HttpRequest>();
            // TODO: Need to setup the getter for request.Query
            mockHttpRequest.SetupGet(m => m.QueryString).Returns(queryString);
            mockHttpContext.SetupGet(m => m.Request).Returns(mockHttpRequest.Object);

            var mockActionDescriptor = new Mock<ControllerActionDescriptor>();
            var actionContext = new ActionContext(mockHttpContext.Object, routeData, mockActionDescriptor.Object);
            controller.ControllerContext = new ControllerContext(actionContext);
        }

        #endregion // Mock HttpContext

        #endregion // Mock Helpers
    }
}