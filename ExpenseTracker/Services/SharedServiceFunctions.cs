using System;
using System.Linq;

namespace ExpenseTracker.Repository
{
    public class SharedServiceFunctions
    {
        protected IQueryable<T> SortQueryableByProperty<T>(IQueryable<T> queryable, string propertyName, bool descending) {
            if (typeof(T).GetProperty(propertyName) == null) {
                throw new ArgumentException($"{typeof(T).Name}.{propertyName} does not exist");
            }

            if (descending) {
                queryable = queryable.OrderByDescending(q => GetPropertyValue(propertyName, q));
            } else {
                queryable = queryable.OrderBy(q => GetPropertyValue(propertyName, q));
            }
            return queryable;
        }

        private object GetPropertyValue<T>(string propertyName, T obj) {
            return obj.GetType().GetProperty(propertyName).GetAccessors()[0].Invoke(obj, null);
        }
    }
}