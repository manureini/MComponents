using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MComponents
{
    public class SorterBuilder<TSource>
    {
        private static readonly MethodInfo OrderByMethod =
            typeof(Queryable).GetMethods()
            .Where(method => method.Name == "OrderBy")
            .Where(method => method.GetParameters().Length == 2)
            .Single();

        private static readonly MethodInfo OrderByDescendingMethod =
         typeof(Queryable).GetMethods()
         .Where(method => method.Name == "OrderByDescending")
         .Where(method => method.GetParameters().Length == 2)
         .Single();

        private static readonly MethodInfo ThenByMethod =
             typeof(Queryable).GetMethods()
             .Where(method => method.Name == "ThenBy")
             .Where(method => method.GetParameters().Length == 2)
             .Single();

        private static readonly MethodInfo ThenByDescendingMethod =
         typeof(Queryable).GetMethods()
         .Where(method => method.Name == "ThenByDescending")
         .Where(method => method.GetParameters().Length == 2)
         .Single();


        private static IOrderedQueryable<TSource> PerformOperation(IQueryable<TSource> source, IMPropertyInfo pField, MethodInfo mi)
        {
            if (typeof(IDictionary<string, object>).IsAssignableFrom(typeof(TSource)))
            {
                Expression<Func<TSource, object>> keySelector = v => ((IDictionary<string, object>)v)[pField.Name];
                var method2 = mi.MakeGenericMethod(new[] { typeof(TSource), typeof(object) });
                var ret2 = method2.Invoke(null, new object[] { source, keySelector });
                return (IOrderedQueryable<TSource>)ret2;
            }

            var param = Expression.Parameter(typeof(TSource), "p");
            var prop = pField.GetMemberExpression(param);
            var exp = Expression.Lambda(prop, param);
            var method = mi.MakeGenericMethod(new[] { typeof(TSource), prop.Type });
            var ret = method.Invoke(null, new object[] { source, exp });
            return (IOrderedQueryable<TSource>)ret;
        }

        public IOrderedQueryable<TSource> SortBy(IQueryable<TSource> source, ICollection<SortInstruction> instrcutions)
        {
            IOrderedQueryable<TSource> result = null;

            foreach (var instrcution in instrcutions.OrderBy(i => i.Index))
                result = result == null ? this.SortFirst(instrcution, source) : this.SortNext(instrcution, result);

            return result;
        }

        protected IOrderedQueryable<TSource> SortFirst(SortInstruction instrcution, IQueryable<TSource> source)
        {
            if (instrcution.Direction == MSortDirection.Ascending)
                return PerformOperation(source, instrcution.PropertyInfo, OrderByMethod);

            return PerformOperation(source, instrcution.PropertyInfo, OrderByDescendingMethod);
        }

        protected IOrderedQueryable<TSource> SortNext(SortInstruction instrcution, IOrderedQueryable<TSource> source)
        {
            if (instrcution.Direction == MSortDirection.Ascending)
                return PerformOperation(source, instrcution.PropertyInfo, ThenByMethod);

            return PerformOperation(source, instrcution.PropertyInfo, ThenByDescendingMethod);
        }
    }

    public class SortInstruction
    {
        public IMPropertyInfo PropertyInfo { get; set; }

        public MSortDirection Direction { get; set; }

        public int Index { get; set; }
    }

}
