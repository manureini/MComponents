using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using MComponents.MGrid;

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

        private static readonly MethodInfo ObjectEquals = typeof(object).GetMethod("Equals", BindingFlags.Instance | BindingFlags.Public);

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
            if (pInstruction.Value != null && pInstruction.PropertyInfo.PropertyType == typeof(string))
            {
                var value = Expression.Constant(((string)pInstruction.Value).Trim().ToLowerInvariant());

                var exprToLower = Expression.Call(property, StringToLower);

                return Expression.Condition(
                    Expression.Equal(property, Expression.Constant(null)),
                    Expression.Constant(false),
                    Expression.Call(exprToLower, StringContains, value)
                    );
            }

            if (pInstruction.Value != null && (pInstruction.PropertyInfo.PropertyType == typeof(DateTime) || pInstruction.PropertyInfo.PropertyType == typeof(DateTime?)))
            {
                DateTime value = (DateTime)pInstruction.Value;

                var day = Expression.Convert(Expression.Constant(value.Date), pInstruction.PropertyInfo.PropertyType);
                var nextDay = Expression.Convert(Expression.Constant(value.Date.Add(TimeSpan.FromDays(1))), pInstruction.PropertyInfo.PropertyType);

                return Expression.AndAlso(Expression.GreaterThanOrEqual(property, day), Expression.LessThan(property, nextDay));
            }

            return Expression.Equal(property, Expression.Constant(pInstruction.Value));
        }

        public IQueryable<TSource> FilterBy(IQueryable<TSource> source, ICollection<FilterInstruction> instructions)
        {
            IQueryable<TSource> result = source;

            foreach (var instruction in instructions)
            {
                result = PerformOperation(result, instruction, WhereMethod);
            }

            return result;
        }
    }

    public class FilterInstruction
    {
        public IMGridColumn GridColumn { get; set; }

        public IMPropertyInfo PropertyInfo { get; set; }

        public object Value { get; set; }
    }

}
