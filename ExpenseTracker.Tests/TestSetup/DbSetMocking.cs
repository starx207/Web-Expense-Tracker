/*
    Code taken from this blog post: http://codethug.com/2015/03/20/mocking-dbset/
 */
 using Microsoft.EntityFrameworkCore;
 using Moq;
 using Moq.Language;
 using Moq.Language.Flow;
 using System.Collections.Generic;
 using System.Linq;

 namespace ExpenseTracker.Tests
 {
     public static class DbSetMocking
     {
         public static Mock<DbSet<T>> CreateMockSet<T>(IQueryable<T> data) where T : class {
             var queryableData = data.AsQueryable();
             var mockSet = new Mock<DbSet<T>>();
             mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryableData.Provider);
             mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryableData.Expression);
             mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryableData.ElementType);
             mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryableData.GetEnumerator());

             return mockSet;
         }

         public static Mock<DbSet<T>> CreateMockSet<T>(IEnumerable<T> data) where T : class {
             return CreateMockSet(data.AsQueryable());
         }

         public static IReturnsResult<TContext> ReturnsDbSet<TEntity, TContext>(
             this IReturns<TContext, DbSet<TEntity>> setup,
             TEntity[] entities
         ) where TEntity : class 
         where TContext : DbContext {
             var mockSet = CreateMockSet(entities.AsQueryable());
             return setup.Returns(mockSet.Object);
         }

         public static IReturnsResult<TContext> ReturnsDbSet<TEntity, TContext>(
             this IReturns<TContext, DbSet<TEntity>> setup,
             IQueryable<TEntity> entities
         ) where TEntity : class 
         where TContext : DbContext {
             var mockSet = CreateMockSet(entities);
             return setup.Returns(mockSet.Object);
         }

         public static IReturnsResult<TContext> ReturnsDbSet<TEntity, TContext>(
             this IReturns<TContext, DbSet<TEntity>> setup,
             IEnumerable<TEntity> entities
         ) where TEntity : class 
         where TContext : DbContext {
             var mockSet = CreateMockSet(entities);
             return setup.Returns(mockSet.Object);
         }
     }
 }