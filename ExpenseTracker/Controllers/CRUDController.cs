using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Controllers
{
    public abstract class CRUDController<T> : Controller
        where T : class
    {
        #region Private Members

        private Func<IQueryable<T>> _getCollectionFunc;

        #endregion // Private Members

        #region Protected Properties

        /// <summary>
        /// The Func to use to sort the collection of <see cref="T"/>
        /// </summary>
        /// <returns></returns>
        protected Func<T, object> CollectionOrderFunc { get; set; }

        /// <summary>
        /// Indicates whether the collection of <see cref="T"/> should be sorted in descending order
        /// </summary>
        /// <returns></returns>
        protected bool OrderDescending { get; set; } = false;

        #endregion // Protected Properties

        #region Constructors

        /// <summary>
        /// Default constructor for a CRUDController
        /// </summary>
        /// <param name="collectionGetter">The function used to get a collection of <see cref="T"/></param>
        public CRUDController(Func<IQueryable<T>> collectionGetter) {
            _getCollectionFunc = collectionGetter;
        }

        #endregion // Constructors

        #region Public Actions

        /// <summary>
        /// Returns the Index view which displays a collection of <see cref="T"/>
        /// </summary>
        /// <returns></returns>
        public virtual async Task<IActionResult> Index() {
            IQueryable<T> data = _getCollectionFunc();
            if (CollectionOrderFunc != null) {
                if (OrderDescending) {
                    data = data.OrderByDescending(d => CollectionOrderFunc(d));
                } else {
                    data = data.OrderBy(d => CollectionOrderFunc(d));
                }
            }
            return View(nameof(Index), await data.ToListAsync());
        }

        #endregion // Public Actions
    }
}