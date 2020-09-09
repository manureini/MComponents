using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MComponents
{
    internal static class IMPropertyInfoExtensions
    {
        public static ICollection<IMPropertyInfo> GetParents(this IMPropertyInfo pPropertyInfo)
        {
            List<IMPropertyInfo> ret = new List<IMPropertyInfo>();
            GetParents(pPropertyInfo, ref ret);
            ret.Reverse();
            return ret;
        }

        private static void GetParents(IMPropertyInfo pPropertyInfo, ref List<IMPropertyInfo> pResult)
        {
            if (pPropertyInfo.Parent == null)
                return;

            pResult.Add(pPropertyInfo.Parent);
            GetParents(pPropertyInfo.Parent, ref pResult);
        }


        public static Expression GetMemberExpression(this IMPropertyInfo pPropertyInfo, ParameterExpression param)
        {
            if (typeof(IDictionary<string, object>).IsAssignableFrom(param.Type))
            {
                if (pPropertyInfo.Parent != null)
                    throw new System.NotImplementedException();

                var p = Expression.Convert(param, typeof(IDictionary<string, object>));

                var expKey = Expression.Constant(pPropertyInfo.Name);

                var containsMi = typeof(IDictionary<string, object>).GetMethod("ContainsKey");

                var exprContains = Expression.Call(p, containsMi, expKey);

                var exprGet = Expression.Property(p, "Item", expKey);

                Expression expGetConvert;

                if (pPropertyInfo.PropertyType == typeof(string))
                {
                    expGetConvert = Expression.Call(exprGet, "ToString", Type.EmptyTypes);
                }
                else
                {
                    expGetConvert = Expression.Convert(exprGet, pPropertyInfo.PropertyType);
                }

                var ifnull = Expression.Condition(exprContains, expGetConvert, Expression.Constant(null, pPropertyInfo.PropertyType));

                return ifnull;
            }

            MemberExpression propertyExpr = null;

            if (pPropertyInfo.Parent != null)
            {
                var parents = GetParents(pPropertyInfo);

                foreach (var entry in parents)
                {
                    if (propertyExpr == null)
                        propertyExpr = Expression.Property(param, entry.Name);
                    else
                        propertyExpr = Expression.Property(propertyExpr, entry.Name);
                }

                propertyExpr = Expression.Property(propertyExpr, pPropertyInfo.Name);
            }
            else
            {
                propertyExpr = Expression.Property(param, pPropertyInfo.Name);
            }

            return propertyExpr;
        }

        public static string GetFullName(this IMPropertyInfo pPropertyInfo)
        {
            var parents = GetParents(pPropertyInfo);

            if (parents.Count <= 0)
                return pPropertyInfo.Name;

            string ret = string.Join(".", parents.Select(p => p.Name));

            ret += "." + pPropertyInfo.Name;

            return ret;
        }
    }
}
