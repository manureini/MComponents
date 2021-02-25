using MComponents.MGrid;
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


        /////////////////////////////////    


        private static readonly MethodInfo OrderByMethod2 =
        typeof(Queryable).GetMethods()
        .Where(method => method.Name == "OrderBy")
        .Where(method => method.GetParameters().Length == 3)
        .Single();

        private static readonly MethodInfo OrderByDescendingMethod2 =
         typeof(Queryable).GetMethods()
         .Where(method => method.Name == "OrderByDescending")
         .Where(method => method.GetParameters().Length == 3)
         .Single();

        private static readonly MethodInfo ThenByMethod2 =
             typeof(Queryable).GetMethods()
             .Where(method => method.Name == "ThenBy")
             .Where(method => method.GetParameters().Length == 3)
             .Single();

        private static readonly MethodInfo ThenByDescendingMethod2 =
         typeof(Queryable).GetMethods()
         .Where(method => method.Name == "ThenByDescending")
         .Where(method => method.GetParameters().Length == 3)
         .Single();

        private static IOrderedQueryable<TSource> PerformOperation(IQueryable<TSource> source, IMPropertyInfo pField, MethodInfo mi, object pComparer)
        {
            var param = Expression.Parameter(typeof(TSource), "p");
            var prop = pField.GetMemberExpression(param);
            var exp = Expression.Lambda(prop, param);
            var method = mi.MakeGenericMethod(new[] { typeof(TSource), prop.Type });

            if (pComparer == null)
                return (IOrderedQueryable<TSource>)method.Invoke(null, new object[] { source, exp });

            return (IOrderedQueryable<TSource>)method.Invoke(null, new object[] { source, exp, pComparer });
        }

        public IOrderedQueryable<TSource> SortBy(IQueryable<TSource> source, IEnumerable<SortInstruction> instrcutions)
        {
            IOrderedQueryable<TSource> result = null;

            foreach (var instrcution in instrcutions.OrderBy(i => i.Index))
                result = result == null ? this.SortFirst(instrcution, source) : this.SortNext(instrcution, result);

            return result;
        }

        protected IOrderedQueryable<TSource> SortFirst(SortInstruction instruction, IQueryable<TSource> source)
        {
            if (instruction.Comparer == null)
            {
                if (instruction.Direction == MSortDirection.Ascending)
                    return PerformOperation(source, instruction.PropertyInfo, OrderByMethod, null);

                return PerformOperation(source, instruction.PropertyInfo, OrderByDescendingMethod, null);
            }


            if (instruction.Direction == MSortDirection.Ascending)
                return PerformOperation(source, instruction.PropertyInfo, OrderByMethod2, instruction.Comparer);

            return PerformOperation(source, instruction.PropertyInfo, OrderByDescendingMethod2, instruction.Comparer);
        }

        protected IOrderedQueryable<TSource> SortNext(SortInstruction instruction, IOrderedQueryable<TSource> source)
        {
            if (instruction.Comparer == null)
            {
                if (instruction.Direction == MSortDirection.Ascending)
                    return PerformOperation(source, instruction.PropertyInfo, ThenByMethod, null);

                return PerformOperation(source, instruction.PropertyInfo, ThenByDescendingMethod, null);
            }

            if (instruction.Direction == MSortDirection.Ascending)
                return PerformOperation(source, instruction.PropertyInfo, ThenByMethod2, instruction.Comparer);

            return PerformOperation(source, instruction.PropertyInfo, ThenByDescendingMethod2, instruction.Comparer);
        }
    }

    public class SortInstruction
    {
        public IMGridPropertyColumn GridColumn { get; set; }

        public IMPropertyInfo PropertyInfo { get; set; }

        public MSortDirection Direction { get; set; }

        public object Comparer { get; set; }

        public int Index { get; set; }
    }

}
