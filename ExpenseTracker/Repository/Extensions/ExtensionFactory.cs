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
        public static Func<IQueryable<BudgetCategory>, ICategoryExtMask> CategoryExtFactory { get; set; }
        public static Func<IQueryable<Payee>, IPayeeExtMask> PayeeExtFactory { get; set; }
        public static Func<IQueryable<Alias>, IAliasExtMask> AliasExtFactory { get; set; }
        public static Func<IQueryable<Transaction>, ITransactionExtMask> TransactionExtFactory { get; set; }

        static ExtensionFactory() {
            CategoryExtFactory = col => new CategoryExt(col);
            PayeeExtFactory = col => new PayeeExt(col);
            AliasExtFactory = col => new AliasExt(col);
            TransactionExtFactory = col => new TransactionExt(col);
        }

        public static ICategoryExtMask Extension(this IQueryable<BudgetCategory> collection) {
            return CategoryExtFactory(collection);
        }

        public static IPayeeExtMask Extension(this IQueryable<Payee> collection) {
            return PayeeExtFactory(collection);
        }

        public static IAliasExtMask Extension(this IQueryable<Alias> collection) {
            return AliasExtFactory(collection);
        }

        public static ITransactionExtMask Extension(this IQueryable<Transaction> collection) {
            return TransactionExtFactory(collection);
        }
    }
}