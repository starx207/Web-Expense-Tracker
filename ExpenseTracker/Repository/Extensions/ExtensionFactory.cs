/*
    This example code taken from http://blogs.clariusconsulting.net/kzu/making-extension-methods-amenable-to-mocking/
 */
 
using ExpenseTracker.Models;
using System;
using System.Linq;

namespace ExpenseTracker.Repository.Extensions
{
    public static class ExtensionFactoryHelpers<T>
    {
        internal static Func<IQueryable<T>, IExtensionMask<T>> ExtFactoryOverride { get; set; }
    }

    public static class ExtensionFactory
    {
        public static IExtensionMask<T> Extension<T>(this IQueryable<T> collection) {
            if (ExtensionFactoryHelpers<T>.ExtFactoryOverride != null) {
                return ExtensionFactoryHelpers<T>.ExtFactoryOverride(collection);
            }
            return new GenericExt<T>(collection);
        }
    }
}