/*
    This example code taken from http://blogs.clariusconsulting.net/kzu/making-extension-methods-amenable-to-mocking/
 */
 
using ExpenseTracker.Models;
using System;
using System.Linq;

namespace ExpenseTracker.Repository.Extensions
{
    public static class ExtensionFactory
    {
        internal static Func<IQueryable<BudgetCategory>, CategoryExt> CategoryExtFactory { get; set; }
        internal static Func<IQueryable<CategoryCrudVm>, CategoryCrudExt> CategoryCrudExtFactory { get; set; }
        internal static Func<IQueryable<Payee>, PayeeExt> PayeeExtFactory { get; set; }
        internal static Func<IQueryable<Alias>, AliasExt> AliasExtFactory { get; set; }
        internal static Func<IQueryable<Transaction>, TransactionExt> TransactionExtFactory { get; set; }

        static ExtensionFactory() {
            CategoryExtFactory = null;
            PayeeExtFactory = null;
            AliasExtFactory = null;
            TransactionExtFactory = null;
        }

        // TODO: figure out how to create generic implentation of this

        public static IExtensionMask<T> Extension<T>(this IQueryable<T> collection) {
            if (CategoryExtFactory != null && typeof(T) == typeof(BudgetCategory)) {
                return (IExtensionMask<T>)CategoryExtFactory((IQueryable<BudgetCategory>)collection);
            }
            if (CategoryCrudExtFactory != null && typeof(T) == typeof(CategoryCrudVm)) {
                return (IExtensionMask<T>)CategoryCrudExtFactory((IQueryable<CategoryCrudVm>)collection);
            }
            if (PayeeExtFactory != null && typeof(T) == typeof(Payee)) {
                return (IExtensionMask<T>)PayeeExtFactory((IQueryable<Payee>)collection);
            }
            if (AliasExtFactory != null && typeof(T) == typeof(Alias)) {
                return (IExtensionMask<T>)AliasExtFactory((IQueryable<Alias>)collection);
            }
            if (TransactionExtFactory != null && typeof(T) == typeof(Transaction)) {
                return (IExtensionMask<T>)TransactionExtFactory((IQueryable<Transaction>)collection);
            }
            return new GenericExt<T>(collection);
        }

        // public static ICategoryExtMask Extension(this IQueryable<BudgetCategory> collection) {
        //     return CategoryExtFactory(collection);
        // }

        // public static IPayeeExtMask Extension(this IQueryable<Payee> collection) {
        //     return PayeeExtFactory(collection);
        // }

        // public static IAliasExtMask Extension(this IQueryable<Alias> collection) {
        //     return AliasExtFactory(collection);
        // }

        // public static ITransactionExtMask Extension(this IQueryable<Transaction> collection) {
        //     return TransactionExtFactory(collection);
        // }
    }
}