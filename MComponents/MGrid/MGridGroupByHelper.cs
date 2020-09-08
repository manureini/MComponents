using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace MComponents.MGrid
{
    /*
    public class TestType
    {
        public bool IsGoodWeather { get; set; }

        public bool IsGoodWeather1 { get; set; }

        public bool IsGoodWeather2 { get; set; }

        public override bool Equals(object obj)
        {
            return MGridGroupByAnonymousTypeHelper.AnonymousTypeEquals(this, obj);
        }

        public override int GetHashCode()
        {
            //return MGridGroupByHelper.AnonymousTypeHashCode(this);
            return 42;
        }
    }*/

    public class MGridGroupByHelper
    {
        private static readonly MethodInfo GroupByMethod =
             typeof(Queryable).GetMethods()
             .Where(method => method.Name == "GroupBy")
             .Where(method => method.GetParameters().Length == 2)
             .First();

        private static readonly MethodInfo SelectMethod =
             typeof(Queryable).GetMethods()
             .Where(method => method.Name == "Select")
             .Where(method => method.GetParameters().Length == 2)
             .First();

        private static readonly MethodInfo CountMethod =
             typeof(Enumerable).GetMethods()
             .Where(method => method.Name == "Count")
             .Where(method => method.GetParameters().Length == 1)
             .First();


        public static IQueryable GetGroupKeyCounts<T>(IQueryable<T> pQueryable, IEnumerable<IMPropertyInfo> pProperties)
        {           
            var anType = MGridGroupByAnonymousTypeHelper.GetAnonymousType(pProperties);

            ParameterExpression parameter = Expression.Parameter(typeof(T), "t");

            List<MemberBinding> bindings = new List<MemberBinding>();

            foreach (var property in pProperties)
            {
                var field = anType.GetProperty(property.Name);
                var exprBind = Expression.Bind(field, property.GetMemberExpression(parameter));

                bindings.Add(exprBind);
            }

            Expression selector = Expression.Lambda(Expression.MemberInit(Expression.New(anType.GetConstructor(Type.EmptyTypes)), bindings), parameter);

            var miGroupBy = GroupByMethod.MakeGenericMethod(new[] { typeof(T), anType });
            var groupedQueryable = (IEnumerable)miGroupBy.Invoke(null, new object[] { pQueryable, selector });

            var igroupType = groupedQueryable.GetType().GenericTypeArguments.First();

            ParameterExpression parameterGrouped = Expression.Parameter(igroupType, "t");
            var keyprop = Expression.PropertyOrField(parameterGrouped, "Key");

            var miCount = CountMethod.MakeGenericMethod(new[] { typeof(T) });
            var countprop = Expression.Call(miCount, new[] { parameterGrouped });


            var tupleGenericType = Type.GetType($"System.Tuple`2");

            var keyPropertyTypes = new[] { anType, typeof(int) };
            var tupleType = tupleGenericType.MakeGenericType(keyPropertyTypes);
            var tupleConstructor = tupleType.GetConstructor(keyPropertyTypes);

            var newTupleExpression = Expression.New(tupleConstructor, new Expression[] { keyprop, countprop });

            var lambda = Expression.Lambda(newTupleExpression, new[] { parameterGrouped });

            var miSelect = SelectMethod.MakeGenericMethod(new[] { igroupType, tupleType });
            var groupSelectedQueryable = miSelect.Invoke(null, new object[] { groupedQueryable, lambda });

            return (IQueryable)groupSelectedQueryable;
        }

        public static IEnumerable<MGridGroupByHelperKeyInfo> GetKeys(IQueryable pKeyCounts, int pSkip, int? pTake, IEnumerable<object> hiddenGroupByKeys)
        {
            int currentIndex = 0;

            List<MGridGroupByHelperKeyInfo> keys = new List<MGridGroupByHelperKeyInfo>();

            int? rowsMissing = pTake;
            var skip = pSkip;

            foreach (dynamic entry in pKeyCounts)
            {
                var dynamicKeyType = entry.Item1;

                if (rowsMissing.HasValue && rowsMissing <= 0)
                    break;

                int countInGroupPart = entry.Item2;
                currentIndex += countInGroupPart;

                //   var properties = (IEnumerable<PropertyInfo>)dynamicKeyType.GetType().GetProperties();
                //   var values = properties.Select(p => p.GetValue(dynamicKeyType)).ToArray();

                if (hiddenGroupByKeys.Any(h => MGridGroupByAnonymousTypeHelper.AnonymousTypeEquals(h, dynamicKeyType)))
                {
                    skip += countInGroupPart;

                    if (currentIndex >= skip)
                    {
                        keys.Add(new MGridGroupByHelperKeyInfo()
                        {
                            DynamicKeyObj = dynamicKeyType,
                            Offset = 0,
                            Take = 0
                        });
                    }

                    continue;
                }

                if (currentIndex > skip)
                {
                    if (rowsMissing.HasValue)
                    {
                        var offset = Math.Max(0, skip - (currentIndex - countInGroupPart));
                        int entryCount = countInGroupPart - offset;

                        var take = rowsMissing.Value;

                        if (take > entryCount)
                        {
                            take = entryCount;
                        }

                        keys.Add(new MGridGroupByHelperKeyInfo()
                        {
                            DynamicKeyObj = dynamicKeyType,
                            Offset = offset,
                            Take = take
                        });

                        rowsMissing -= take;
                    }
                    else
                    {
                        keys.Add(new MGridGroupByHelperKeyInfo()
                        {
                            DynamicKeyObj = dynamicKeyType,
                            Offset = 0,
                            Take = countInGroupPart
                        });
                    }
                }
            }

            return keys;
        }


        public static long GetDataCount(IQueryable pKeyObjects, IEnumerable<object> hiddenGroupByKeys)
        {
            long total = 0;

            foreach (dynamic tuple in pKeyObjects)
            {
                object keyObj = tuple.Item1;

                if (hiddenGroupByKeys.Any(h => MGridGroupByAnonymousTypeHelper.AnonymousTypeEquals(h, keyObj)))
                    continue;

                int count = tuple.Item2;
                total += count;
            }

            return total;
        }

    }
}
