using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;

namespace MComponents
{
    public class FilterBuilder<TSource>
    {
        private static readonly MethodInfo WhereMethod =
            typeof(Queryable).GetMethods()
            .Where(method => method.Name == "Where")
            .Where(method => method.GetParameters().Length == 2)
            .First();

        private static readonly MethodInfo StringContains = typeof(string).GetMethod("Contains", new[] { typeof(string) });
        private static readonly MethodInfo StringToLower = typeof(string).GetMethod("ToLowerInvariant");

        private static IQueryable<TSource> PerformOperation(IQueryable<TSource> source, FilterInstruction pInstruction, MethodInfo pMethodInfo)
        {
            var param = Expression.Parameter(typeof(TSource), "p");

            Expression propertyExpr = pInstruction.PropertyInfo.GetMemberExpression(param);

            Expression exprEqual = GetCompareExpression(pInstruction, propertyExpr);

            var lambda = Expression.Lambda(exprEqual, param);

            var method = pMethodInfo.MakeGenericMethod(new[] { typeof(TSource) });
            var ret = method.Invoke(null, new object[] { source, lambda });
            return (IQueryable<TSource>)ret;
        }

        private static Expression GetCompareExpression(FilterInstruction pInstruction, Expression property)
        {
            if (pInstruction.PropertyInfo.PropertyType == typeof(string))
            {
                var value = Expression.Constant(((string)pInstruction.Value).ToLowerInvariant());

                property = Expression.Call(property, StringToLower);
                Expression.Call(property, StringContains, value);

                return Expression.Call(property, StringContains, value);
            }

            if (pInstruction.PropertyInfo.PropertyType == typeof(DateTime) || pInstruction.PropertyInfo.PropertyType == typeof(DateTime?))
            {
                DateTime value = (DateTime)pInstruction.Value;

                var day = Expression.Convert(Expression.Constant(value.Date), pInstruction.PropertyInfo.PropertyType);
                var nextDay = Expression.Convert(Expression.Constant(value.Date.Add(TimeSpan.FromDays(1))), pInstruction.PropertyInfo.PropertyType);

                return Expression.AndAlso(Expression.GreaterThanOrEqual(property, day), Expression.LessThan(property, nextDay));
            }

            return Expression.Equal(property, Expression.Constant(pInstruction.Value));
        }

        public IQueryable<TSource> FilterBy(IQueryable<TSource> source, ICollection<FilterInstruction> instrcutions)
        {
            IQueryable<TSource> result = source;

            foreach (var instrcution in instrcutions)
            {
                result = PerformOperation(result, instrcution, WhereMethod);
            }

            return result;
        }
    }

    public class FilterInstruction
    {
        public IMPropertyInfo PropertyInfo { get; set; }

        public object Value { get; set; }
    }

}
