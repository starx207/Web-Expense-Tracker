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
        #region Protected Properties

        /// <summary>
        /// The Func to rebind elements of the view model before returning to the view
        /// on a failed POST action
        /// </summary>
        /// <returns></returns>
        protected Action<VM> FailedPostRebinder { get; set; }

        /// <summary>
        /// The Func to use to check if an object of type <see cref="VM"/> exists
        /// </summary>
        /// <returns></returns>
        protected Func<VM, bool> ExistanceChecker { get; set; }

        /// <summary>
        /// The Func to use to edit an object of <see cref="VM"/>
        /// </summary>
        /// <returns></returns>
        protected Func<VM, Task<int>> SingleEditor { get; set; }

        /// <summary>
        /// The Func to use to add a <see cref="VM"/> to the collection
        /// </summary>
        /// <returns></returns>
        protected Func<VM, Task<int>> SingleAdder { get; set; }

        /// <summary>
        /// The Func to use to retrieve a single instance of a <see cref="VM"/>
        /// </summary>
        /// <returns></returns>
        protected Func<int, Task<int>> SingleDeleter { get; set; }

        /// <summary>
        /// The Func to use for retrieving a collection of <see cref="VM"/> objects
        /// </summary>
        /// <returns></returns>
        protected Func<Task<IEnumerable<VM>>> CollectionGetter { get; set; }

        /// <summary>
        /// The Func to use for creating <see cref="VM"/> objects. Initialized in constructor,
        /// but can be overridden later
        /// </summary>
        protected Func<int?, Task<VM>> ViewModelCreator { get; set; }

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

            CollectionGetter = collectionGetter ?? (() => throw new NotImplementedException("No method defined for retrieving class collection"));
            SingleDeleter = singleDeleter ?? (id => throw new NotImplementedException("No method defined for deleting class instance"));
            SingleAdder = singleAdder ?? (viewModel => throw new NotImplementedException("No method defined for adding class instance"));
            ViewModelCreator = viewModelCreator ?? (baseType => throw new NotImplementedException("No method defined for creating view model"));
            SingleEditor = singleEditor ?? (viewModel => throw new NotImplementedException("No method defined for editing class instance"));
            ExistanceChecker = existanceChecker ?? (viewModel => throw new NotImplementedException("No method defined for checking object existance"));
        }

        #endregion // Constructors

        #region Public Actions

        #region GET Actions

        /// <summary>
        /// Returns the Index view for <see cref="VM"/>
        /// </summary>
        /// <returns></returns>
        public virtual async Task<IActionResult> Index() {
            return View(nameof(Index), await CollectionGetter());
        }

        /// <summary>
        /// Returns the Details view for <see cref="VM"/>
        /// </summary>
        /// <param name="id">The Id of the object to display</param>
        /// <returns></returns>
        public virtual async Task<IActionResult> Details(int? id) {
            VM objectToShow;
            try {
                objectToShow = (await ViewModelCreator(id)) ?? throw new NullViewModelException();
            } catch (ExpenseTrackerException) {
                return NotFound();
            }
            return View(nameof(Details), objectToShow);
        }

        /// <summary>
        /// Returns the Create view for <see cref="VM"/>
        /// </summary>
        /// <returns></returns>
        public virtual async Task<IActionResult> Create() => View(nameof(Create), await ViewModelCreator(null));

        /// <summary>
        /// Returns the Edit view for <see cref="VM"/>
        /// </summary>
        /// <param name="id">The Id of the object to display</param>
        /// <returns></returns>
        public virtual async Task<IActionResult> Edit(int? id) {
            VM objectToEdit;
            try {
                objectToEdit = await ViewModelCreator(id);
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
                objectToDelete = (await ViewModelCreator(id)) ?? throw new NullViewModelException();
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
                await SingleAdder(createdObject);
                return RedirectToAction(nameof(Index));
            } catch (ModelValidationException mvex) {
                ModelState.AddModelError(mvex.PropertyName, mvex.Message);
            } catch (Exception ex) {
                var result = ApplyCustomExceptionHandling(ex);
                if (result != null) {
                    return result;
                }
            }
            if (FailedPostRebinder != null) {
                FailedPostRebinder(createdObject);
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
                await SingleEditor(editedObject);
                return RedirectToAction(nameof(Index));
            } catch (ConcurrencyException) {
                if (ExistanceChecker(editedObject)) {
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
            if (FailedPostRebinder != null) {
                FailedPostRebinder(editedObject);
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
            await SingleDeleter(id);
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