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
        private Func<VM, Task<int>> _editObjectAsyncFunc;
        private Func<VM, bool> _checkExistsFunc;

        #endregion // Private Members

        #region Protected Properties

        /// <summary>
        /// The Func to use to sort the collection of <see cref="VM"/>
        /// </summary>
        protected Func<VM, object> CollectionOrderFunc { get; set; } = null;

        /// <summary>
        /// Indicates whether the collection of <see cref="VM"/> should be sorted in descending order
        /// </summary>
        protected bool OrderDescending { get; set; } = false;

        /// <summary>
        /// A Dictionary of Exception Handling functions. The key should be the type of exception to handle.
        /// The value should be the function that will handle the exception type. Exception types will first
        /// be compared at face-value before evaluating Base exception types.
        /// 
        /// The Function's return value should be an IActionResult of where to go next after the exception has been
        /// handled. If you wish for execution to continue as normal, return null
        /// </summary>
        protected Dictionary<Type, Func<Exception, IActionResult>> ExceptionHandling { get; set; } = null;

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
            Func<int, Task<int>> singleDeleter,
            Func<VM, Task<int>> singleEditor,
            Func<VM, bool> existanceChecker) {

            _getCollectionFunc = collectionGetter;
            _getSingleObjectAsyncFunc = singleGetter;
            _removeObjectAsyncFunc = singleDeleter;
            _addObjectAsyncFunc = singleAdder;
            _viewModelCreator = viewModelCreator;
            _editObjectAsyncFunc = singleEditor;
            _checkExistsFunc = existanceChecker;
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

        /// <summary>
        /// Attempts to create an object of type <see cref="T"/>
        /// If the model is not valid, returns to the view
        /// </summary>
        /// <param name="createdObject">The <see cref="VM"/> to use to create a new object of <see cref="T"/></param>
        /// <returns></returns>
        [HttpPost, ValidateAntiForgeryToken]
        public virtual async Task<IActionResult> Create(VM createdObject) {
            if (ModelState.IsValid) {
                try {
                    await _addObjectAsyncFunc(createdObject);
                    return RedirectToAction(nameof(Index));
                } catch (UniqueConstraintViolationException uex) {
                    ModelState.AddModelError(uex.PropertyName, uex.Message);
                } catch (Exception ex) {
                    // If the child class has defined Exception Handling for the exception
                    // type, handle it, otherwise throw
                    if (ExceptionHandling == null) {
                        throw;
                    }
                    if (!ExceptionHandling.TryGetValue(ex.GetType(), out Func<Exception, IActionResult> handler)) {
                        throw;
                    }
                    var result = handler(ex);
                    if (result != null) {
                        return result;
                    }
                }
            }
            return View(nameof(Create), createdObject);
        }

        /// <summary>
        /// Attempts to edit an object of type <see cref="T"/>
        /// If the model is not valid, returns to the view
        /// </summary>
        /// <param name="editedObject">The <see cref="VM"/> to use to edit an object of <see cref="T"/></param>
        /// <returns></returns>
        [HttpPost, ValidateAntiForgeryToken]
        public virtual async Task<IActionResult> Edit(VM editedObject) {
            if (ModelState.IsValid) {
                try {
                    await _editObjectAsyncFunc(editedObject);
                    return RedirectToAction(nameof(Index));
                } catch (Exception ex) {
                    // Concurrency exception should return NotFound if
                    // the object doesn't exist, otherwise should throw
                    if (ex is ConcurrencyException) {
                        if (_checkExistsFunc(editedObject)) {
                            throw;
                        } else {
                            return NotFound();
                        }
                    }

                    // If the child class has defined Exception Handling for the exception
                    // type, handle it, otherwise throw
                    if (ExceptionHandling == null) {
                        throw;
                    }
                    // First Check if Exception type directly handled
                    if (!ExceptionHandling.TryGetValue(ex.GetType(), out Func<Exception, IActionResult> handler)) {
                        // If not, check if parent type handled
                        if (!ExceptionHandling.TryGetValue(ex.GetType().BaseType, out handler)) {
                            throw;
                        }
                    }
                    var result = handler(ex);
                    if (result != null) {
                        return result;
                    }
                }
            }
            return View(nameof(Edit), editedObject);
        }

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