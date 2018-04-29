/*
    Code taken from this blog post: http://codethug.com/2015/03/20/mocking-dbset/
 */
 using Microsoft.EntityFrameworkCore;
 using Moq;
 using Moq.Language;
 using Moq.Language.Flow;
 using System;
 using System.Collections.Generic;
 using System.Linq;

 namespace ExpenseTracker.Tests
 {
     public static class DbSetMocking
     {
         #region CreaeMockSet

        /// <summary>
        /// Creates a Mock DbSet from an IQueryable data collection
        /// </summary>
        /// <param name="data">The data to create a DbSet from</param>
        /// <returns>A Mock DbSet of the provided data</returns>
        public static Mock<DbSet<T>> CreateMockSet<T>(IQueryable<T> data) where T : class {
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns((data ?? throw new ArgumentNullException("data")).Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            return mockSet;
        }

        /// <summary>
        /// Creates a Mock DbSet from an IEnumerable data collection
        /// </summary>
        /// <param name="data">The data to create a DbSet from</param>
        /// <returns>A Mock DbSet of the provided data</returns>
        public static Mock<DbSet<T>> CreateMockSet<T>(IEnumerable<T> data) 
            where T : class => CreateMockSet((data ?? throw new ArgumentNullException("data")).AsQueryable());

        #endregion // CreateMockSet

        #region ReturnsDbSet

        /// <summary>
        /// Mocks the return value of a method that returns a DbSet
        /// </summary>
        /// <param name="setup">The Mock setup to return a Mock DbSet</param>
        /// <param name="entities">The an array to return as a DbSet</param>
        /// <returns></returns>
        public static IReturnsResult<TContext> ReturnsDbSet<TEntity, TContext>(
            this IReturns<TContext, DbSet<TEntity>> setup,
            TEntity[] entities
        ) where TEntity : class
        where TContext : DbContext => setup.Returns(
            CreateMockSet((entities ?? throw new ArgumentNullException("entities")).AsQueryable()).Object
        );

        /// <summary>
        /// Mocks the return value of a method that returns a DbSet
        /// </summary>
        /// <param name="setup">The Mock setup to return a Mock DbSet</param>
        /// <param name="entities">The an IQueryable to return as a DbSet</param>
        /// <returns></returns>
        public static IReturnsResult<TContext> ReturnsDbSet<TEntity, TContext>(
            this IReturns<TContext, DbSet<TEntity>> setup,
            IQueryable<TEntity> entities
        ) where TEntity : class
        where TContext : DbContext => setup.Returns(CreateMockSet((entities ?? throw new ArgumentNullException("entites"))).Object);

        /// <summary>
        /// Mocks the return value of a method that returns a DbSet
        /// </summary>
        /// <param name="setup">The Mock setup to return a Mock DbSet</param>
        /// <param name="entities">The an IEnumerable to return as a DbSet</param>
        /// <returns></returns>
        public static IReturnsResult<TContext> ReturnsDbSet<TEntity, TContext>(
            this IReturns<TContext, DbSet<TEntity>> setup,
            IEnumerable<TEntity> entities
        ) where TEntity : class
        where TContext : DbContext => setup.Returns(CreateMockSet((entities ?? throw new ArgumentNullException("entites"))).Object);

        #endregion // ReturnsDbSet
    }
 }