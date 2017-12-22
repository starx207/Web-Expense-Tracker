/*
    These classes were created using the information contained here: https://msdn.microsoft.com/en-us/library/dn314429(v=vs.113).aspx

    This is a potential solution to adapt to .NetCore: https://stackoverflow.com/questions/39719258/idbasyncqueryprovider-in-entityframeworkcore
 */
using System;
using System.Collections.Generic; 
using Microsoft.EntityFrameworkCore;
using System.Linq; 
using System.Linq.Expressions; 
using System.Threading; 
using System.Threading.Tasks;

 namespace ExpenseTracker.Tests
 {
     public class AsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public AsyncEnumerable(Expression expression)
            : base(expression) { }

        public IAsyncEnumerator<T> GetEnumerator() =>
            new AsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    public class AsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> enumerator;

        public AsyncEnumerator(IEnumerator<T> enumerator) =>
            this.enumerator = enumerator ?? throw new ArgumentNullException();

        public T Current => enumerator.Current;

        public void Dispose() { }

        public Task<bool> MoveNext(CancellationToken cancellationToken) =>
            Task.FromResult(enumerator.MoveNext());
    }
 }