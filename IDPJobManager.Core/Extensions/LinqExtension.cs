using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace IDPJobManager.Core.Extensions
{
    public static class LinqExtension
    {
        public static IQueryable<T> OrderBy<T>(this IQueryable<T> query, string sortKey, string sortType)
        {
            if (query == null)
                throw new ArgumentNullException("query");
            if (string.IsNullOrEmpty(sortKey))
                throw new ArgumentNullException("sortKey");
            string orderByMethod = string.Empty;
            if (string.IsNullOrEmpty(sortType))
                orderByMethod = "OrderBy";
            else if (sortType.ToUpper() == "ASC")
                orderByMethod = "OrderBy";
            else if (sortType.ToUpper() == "DESC")
                orderByMethod = "OrderByDescending";
            if (orderByMethod == string.Empty)
                throw new ArgumentException("invalid parameter: `sortType`.");
            Type type = typeof(T);
            ParameterExpression parameterExpr = Expression.Parameter(type, sortKey);
            PropertyInfo pi = type.GetProperty(sortKey);
            MemberExpression memberExpr = Expression.Property(parameterExpr, pi);
            Type[] types = new Type[] { type, pi.PropertyType };
            Expression expr = Expression.Call(typeof(Queryable), orderByMethod, types, query.Expression, Expression.Lambda(memberExpr, parameterExpr));
            return query.AsQueryable().Provider.CreateQuery<T>(expr);
        }
    }
}
