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


        public static MemberExpression GetMemberExpression(this IMPropertyInfo pPropertyInfo, ParameterExpression param)
        {
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
