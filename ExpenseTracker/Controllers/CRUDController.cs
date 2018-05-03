using ExpenseTracker.Exceptions;
using ExpenseTracker.Repository.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Controllers
{
    public abstract class CRUDController<VM, T> : BaseController
        where VM : class
        where T : class
    {
        #region Private Members

        private Func<T, VM> _viewModelCreator;
        private Func<IQueryable<T>> _getCollectionFunc;
        private Func<int?, Task<T>> _getSingleObjectAsyncFunc;
        private Func<int, Task<int>> _removeObjectAsyncFunc;
        private Func<VM, Task<int>> _addObjectAsyncFunc;

        #endregion // Private Members

        #region Protected Properties

        /// <summary>
        /// The Func to use to sort the collection of <see cref="VM"/>
        /// </summary>
        /// <returns></returns>
        protected Func<VM, object> CollectionOrderFunc { get; set; } = null;

        /// <summary>
        /// Indicates whether the collection of <see cref="VM"/> should be sorted in descending order
        /// </summary>
        /// <returns></returns>
        protected bool OrderDescending { get; set; } = false;

        protected Dictionary<Type, Action<Exception>> ExceptionHandling { get; set; } = null;

        #endregion // Protected Properties

        #region Constructors

        /// <summary>
        /// Default constructor for a CRUDController
        /// </summary>
        /// <param name="collectionGetter">The function used to get a collection of <see cref="VM"/></param>
        public CRUDController(Func<T, VM> viewModelCreator,
            Func<IQueryable<T>> collectionGetter, 
            Func<int?, Task<T>> singleGetter,
            Func<VM, Task<int>> singleAdder,
            Func<int, Task<int>> singleDeleter) {

            _getCollectionFunc = collectionGetter;
            _getSingleObjectAsyncFunc = singleGetter;
            _removeObjectAsyncFunc = singleDeleter;
            _addObjectAsyncFunc = singleAdder;
            _viewModelCreator = viewModelCreator;
        }

        #endregion // Constructors

        #region Public Actions

        #region GET Actions

        /// <summary>
        /// Returns the Index view for <see cref="VM"/>
        /// </summary>
        /// <returns></returns>
        public virtual async Task<IActionResult> Index() {
            IQueryable<VM> data = _getCollectionFunc().Select(d => _viewModelCreator(d));
            if (CollectionOrderFunc != null) {
                if (OrderDescending) {
                    data = data.OrderByDescending(d => CollectionOrderFunc(d));
                } else {
                    data = data.OrderBy(d => CollectionOrderFunc(d));
                }
            }
            return View(nameof(Index), await data.Extension().ToListAsync());
        }

        /// <summary>
        /// Returns the Details view for <see cref="VM"/>
        /// </summary>
        /// <param name="id">The Id of the object to display</param>
        /// <returns></returns>
        public virtual async Task<IActionResult> Details(int? id) {
            VM objectToShow;
            try {
                objectToShow = _viewModelCreator(await _getSingleObjectAsyncFunc(id));
            } catch (IdNotFoundException) {
                return NotFound();
            } catch (NullIdException) {
                return NotFound();
            }
            return View(nameof(Details), objectToShow);
        }

        /// <summary>
        /// Returns the Create view for <see cref="VM"/>
        /// </summary>
        /// <returns></returns>
        public virtual IActionResult Create() => View(nameof(Create));

        /// <summary>
        /// Returns the Edit view for <see cref="VM"/>
        /// </summary>
        /// <param name="id">The Id of the object to display</param>
        /// <returns></returns>
        public virtual async Task<IActionResult> Edit(int? id) {
            VM objectToEdit;
            try {
                objectToEdit = _viewModelCreator(await _getSingleObjectAsyncFunc(id));
            } catch (ExpenseTrackerException) {
                return NotFound();
            }

            return View(nameof(Edit), objectToEdit);
        }

        /// <summary>
        /// Returns the Delete view for <see cref="VM"/>
        /// </summary>
        /// <param name="id">The Id of the object to delete</param>
        /// <returns></returns>
        public virtual async Task<IActionResult> Delete(int? id) {
            VM objectToDelete;
            try {
                objectToDelete = _viewModelCreator(await _getSingleObjectAsyncFunc(id));
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
        public virtual async Task<IActionResult> Create(VM createdObject) {
            if (ModelState.IsValid) {
                try {
                    await _addObjectAsyncFunc(createdObject);
                    RedirectToAction(nameof(Index));
                } catch (UniqueConstraintViolationException uex) {
                    ModelState.AddModelError(uex.PropertyName, uex.Message);
                } catch (Exception ex) {
                    if (ExceptionHandling == null) {
                        throw;
                    }
                    if (!ExceptionHandling.TryGetValue(ex.GetType(), out Action<Exception> handler)) {
                        throw;
                    }
                    handler(ex);
                }
            }
            return View(nameof(Create), createdObject);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public abstract Task<IActionResult> Edit(VM editedObject);

        /// <summary>
        /// Attempts to delete an object from the collection of <see cref="VM"/>
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