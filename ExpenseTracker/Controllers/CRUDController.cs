using ExpenseTracker.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Controllers
{
    public abstract class CRUDController<T> : BaseController
        where T : class
    {
        #region Private Members

        private Func<IQueryable<T>> _getCollectionFunc;
        private Func<int?, Task<T>> _getSingleObjectAsyncFunc;
        private Func<int, Task<int>> _removeObjectAsyncFunc;
        private Func<T, Task<int>> _addObjectAsyncFunc;
        private string _createActionBinding;

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
        public CRUDController(Func<IQueryable<T>> collectionGetter, 
            Func<int?, Task<T>> singleGetter,
            Func<T, Task<int>> singleAdder,
            Func<int, Task<int>> singleDeleter) {
            _getCollectionFunc = collectionGetter;
            _getSingleObjectAsyncFunc = singleGetter;
            _removeObjectAsyncFunc = singleDeleter;
            _addObjectAsyncFunc = singleAdder;
        }

        #endregion // Constructors

        #region Public Actions

        #region GET Actions

        /// <summary>
        /// Returns the Index view for <see cref="T"/>
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

        /// <summary>
        /// Returns the Details view for <see cref="T"/>
        /// </summary>
        /// <param name="id">The Id of the object to display</param>
        /// <returns></returns>
        public virtual async Task<IActionResult> Details(int? id) {
            T objectToShow;
            try {
                objectToShow = await _getSingleObjectAsyncFunc(id);
            } catch (IdNotFoundException) {
                return NotFound();
            } catch (NullIdException) {
                return NotFound();
            }
            return View(nameof(Details), objectToShow);
        }

        /// <summary>
        /// Returns the Create view for <see cref="T"/>
        /// </summary>
        /// <returns></returns>
        public virtual IActionResult Create() => View(nameof(Create));

        /// <summary>
        /// Returns the Edit view for <see cref="T"/>
        /// </summary>
        /// <param name="id">The Id of the object to display</param>
        /// <returns></returns>
        public virtual async Task<IActionResult> Edit(int? id) {
            T objectToEdit;
            try {
                objectToEdit = await _getSingleObjectAsyncFunc(id);
            } catch (ExpenseTrackerException) {
                return NotFound();
            }

            return View(nameof(Edit), objectToEdit);
        }

        /// <summary>
        /// Returns the Delete view for <see cref="T"/>
        /// </summary>
        /// <param name="id">The Id of the object to delete</param>
        /// <returns></returns>
        public virtual async Task<IActionResult> Delete(int? id) {
            T objectToDelete;
            try {
                objectToDelete = await _getSingleObjectAsyncFunc(id);
            } catch (IdNotFoundException) {
                return NotFound();
            } catch (NullIdException) {
                return NotFound();
            }

            return View(nameof(Delete), objectToDelete);
        }

        #endregion // GET Actions

        #region POST Actions

        // TODO: figure out how to implement binding for Create/Edit POST actions
        //       for now, I'll just add stubs here

        [HttpPost, ValidateAntiForgeryToken]
        public abstract Task<IActionResult> Create(T createdObject);

        [HttpPost, ValidateAntiForgeryToken]
        public abstract Task<IActionResult> Edit(int id, T editedObject);

        /// <summary>
        /// Attempts to delete an object from the collection of <see cref="T"/>
        /// Then returns to <see cref="Index"/>
        /// </summary>
        /// <param name="id">The Id of the object to delete</param>
        /// <returns></returns>
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public virtual async Task<IActionResult> DeleteConfirmed(int id) {
            await _removeObjectAsyncFunc(id);
            return RedirectToAction(nameof(Index));
        }

        #endregion // POST Actions

        #endregion // Public Actions
    }
}