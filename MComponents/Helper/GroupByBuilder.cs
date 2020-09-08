using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using MComponents.MGrid;

namespace MComponents
{/*
    public class GroupByBuilder<TSource>
    {
        private static readonly MethodInfo GroupByMethod =
            typeof(Queryable).GetMethods()
            .Where(method => method.Name == "GroupBy")
            .Where(method => method.GetParameters().Length == 2)
            .First();

        private static readonly MethodInfo SkipMethod =
            typeof(Queryable).GetMethods()
            .Where(method => method.Name == "Skip")
            .Where(method => method.GetParameters().Length == 2)
            .First();

        private static readonly MethodInfo TakeMethod =
            typeof(Queryable).GetMethods()
            .Where(method => method.Name == "Take")
            .Where(method => method.GetParameters().Length == 2)
            .First();

        private static IQueryable PerformOperation(IQueryable<TSource> source, GroupByInstruction pInstruction, MethodInfo pMethodInfo)
        {
            var param = Expression.Parameter(typeof(TSource), "p");

            Expression propertyExpr = pInstruction.PropertyInfo.GetMemberExpression(param);


            var lambda = Expression.Lambda(propertyExpr, param);

            var method = pMethodInfo.MakeGenericMethod(new[] { typeof(TSource), pInstruction.PropertyInfo.PropertyType });
            var ret = method.Invoke(null, new object[] { source, lambda });
            return (IQueryable)ret;
        }

        public IQueryable GroupBy(IQueryable<TSource> source, GroupByInstruction instruction)
        {
            var result = PerformOperation(source, instruction, GroupByMethod);
            return result;
        }

        public IQueryable Skip(IQueryable source, int pCount)
        {
            var method = SkipMethod.MakeGenericMethod(new[] { source.ElementType });
            var ret = method.Invoke(null, new object[] { source, pCount });
            return (IQueryable)ret;
        }

        public IQueryable Take(IQueryable source, int pCount)
        {
            var method = TakeMethod.MakeGenericMethod(new[] { source.ElementType });
            var ret = method.Invoke(null, new object[] { source, pCount });
            return (IQueryable)ret;
        }
    }
    */

    public class GroupByInstruction<T> : SortInstruction
    {
        public T FirstValue { get; set; } 
    }

}
