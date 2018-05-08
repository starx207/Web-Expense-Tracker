using ExpenseTracker.Exceptions;
using ExpenseTracker.Models;
using ExpenseTracker.Repository.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Controllers
{
    public abstract class CRUDController<VM> : BaseController
        where VM : class, ICrudViewModel
    {
        #region Private Members

        private Func<Task<IEnumerable<VM>>> _getCollectionFunc;
        private Func<int, Task<int>> _removeObjectAsyncFunc;
        private Func<VM, Task<int>> _addObjectAsyncFunc;
        private Func<VM, Task<int>> _editObjectAsyncFunc;
        private Func<VM, bool> _checkExistsFunc;

        #endregion // Private Members

        #region Protected Properties

        /// <summary>
        /// The Func to use for creating <see cref="VM"/> objects. Initialized in constructor,
        /// but can be overridden later
        /// </summary>
        protected Func<int?, Task<VM>> ViewModelCreatorFunc { get; set; }

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
        public CRUDController(Func<int?, Task<VM>> viewModelCreator = null,
            Func<Task<IEnumerable<VM>>> collectionGetter = null,
            Func<VM, Task<int>> singleAdder = null,
            Func<int, Task<int>> singleDeleter = null,
            Func<VM, Task<int>> singleEditor = null,
            Func<VM, bool> existanceChecker = null) {

            _getCollectionFunc = collectionGetter ?? (() => throw new NotImplementedException("No method defined for retrieving class collection"));
            _removeObjectAsyncFunc = singleDeleter ?? (id => throw new NotImplementedException("No method defined for deleting class instance"));
            _addObjectAsyncFunc = singleAdder ?? (viewModel => throw new NotImplementedException("No method defined for adding class instance"));
            ViewModelCreatorFunc = viewModelCreator ?? (baseType => throw new NotImplementedException("No method defined for creating view model"));
            _editObjectAsyncFunc = singleEditor ?? (viewModel => throw new NotImplementedException("No method defined for editing class instance"));
            _checkExistsFunc = existanceChecker ?? (viewModel => throw new NotImplementedException("No method defined for checking object existance"));
        }

        #endregion // Constructors

        #region Public Actions

        #region GET Actions

        /// <summary>
        /// Returns the Index view for <see cref="VM"/>
        /// </summary>
        /// <returns></returns>
        public virtual async Task<IActionResult> Index() {
            return View(nameof(Index), await _getCollectionFunc());
        }

        /// <summary>
        /// Returns the Details view for <see cref="VM"/>
        /// </summary>
        /// <param name="id">The Id of the object to display</param>
        /// <returns></returns>
        public virtual async Task<IActionResult> Details(int? id) {
            VM objectToShow;
            try {
                objectToShow = (await ViewModelCreatorFunc(id)) ?? throw new NullViewModelException();
            } catch (ExpenseTrackerException) {
                return NotFound();
            }
            return View(nameof(Details), objectToShow);
        }

        /// <summary>
        /// Returns the Create view for <see cref="VM"/>
        /// </summary>
        /// <returns></returns>
        public virtual async Task<IActionResult> Create() => View(nameof(Create), await ViewModelCreatorFunc(null));

        /// <summary>
        /// Returns the Edit view for <see cref="VM"/>
        /// </summary>
        /// <param name="id">The Id of the object to display</param>
        /// <returns></returns>
        public virtual async Task<IActionResult> Edit(int? id) {
            VM objectToEdit;
            try {
                objectToEdit = await ViewModelCreatorFunc(id);
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
                objectToDelete = (await ViewModelCreatorFunc(id)) ?? throw new NullViewModelException();
            } catch (ExpenseTrackerException) {
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
                try {
                    await _addObjectAsyncFunc(createdObject);
                    return RedirectToAction(nameof(Index));
                } catch (ModelValidationException mvex) {
                    ModelState.AddModelError(mvex.PropertyName, mvex.Message);
                } catch (Exception ex) {
                    var result = ApplyCustomExceptionHandling(ex);
                    if (result != null) {
                        return result;
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
                try {
                    if (GetRoutedId() != editedObject.NavId) {
                        return NotFound();
                    }
                    await _editObjectAsyncFunc(editedObject);
                    return RedirectToAction(nameof(Index));
                } catch (ConcurrencyException) {
                    if (_checkExistsFunc(editedObject)) {
                        throw;
                    }
                    return NotFound();
                } catch (ModelValidationException mvex) {
                    ModelState.AddModelError(mvex.PropertyName, mvex.Message);
                } catch (Exception ex) {
                    var result = ApplyCustomExceptionHandling(ex);
                    if (result != null) {
                        return result;
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

        #region Private Helpers

        private IActionResult ApplyCustomExceptionHandling(Exception ex) {
            if (ExceptionHandling == null) { throw ex; }

            // Is Exception Type handled?
            Func<Exception, IActionResult> handler = GetHandler(ex.GetType()) ?? throw ex;

            // handle the exception
            return handler(ex);
        }

        private Func<Exception, IActionResult> GetHandler(Type exceptionType) {
            if (ExceptionHandling.TryGetValue(exceptionType, out Func<Exception, IActionResult> handler)) {
                return handler; // return the handler
            }
            if (exceptionType == typeof(Exception)) { 
                return null; // We've reached the base exception type and it is not handled
            }
            // Recurse to the exceptionTypes base type and recheck handling
            return GetHandler(exceptionType.BaseType);
        }

        #endregion // Private Helpers
    }
}