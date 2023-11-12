using DocumentFormat.OpenXml.Office2013.Word;
using DocumentFormat.OpenXml.Presentation;
using LinqKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MComponents.MQueryBuilder
{
    public class MQueryBuilderHelper
    {
        private static readonly MethodInfo WhereMethod =
                 typeof(Queryable).GetMethods()
                 .Where(method => method.Name == "Where")
                 .Where(method => method.GetParameters().Length == 2)
                 .First();

        public static IQueryable<T> ApplyRules<T>(IQueryable<T> pDataSource, MQueryBuilderRuleGroup pRules, Func<string, Expression<Func<T, object, bool>>> pExpressionCallback)
        {

            //DataSource.Where(d => d.Equals = "sdf " && d  d () )

            var expandable = pDataSource.AsExpandable();

            var groupExpr = GetGroupExpression(pRules, pExpressionCallback);






            //    Expression.Constant(pRules)


            return expandable.Where(groupExpr);

        }

        private static ExpressionStarter<T> GetGroupExpression<T>(MQueryBuilderRuleGroup pRuleGroup, Func<string, Expression<Func<T, object, bool>>> pExpressionCallback)
        {
            var groupExpr = PredicateBuilder.New<T>();

            foreach (var condition in pRuleGroup.Conditions)
            {
                var expr = pExpressionCallback(condition.Property);

                if (expr != null)
                {
                    if (pRuleGroup.Operator == MQueryBuilderRuleGroupOperator.Or)
                    {
                        groupExpr = groupExpr.Or(p => expr.Invoke(p, condition.Value));
                    }
                    else
                    {
                        groupExpr = groupExpr.And(p => expr.Invoke(p, condition.Value));
                    }
                }
            }

            if (pRuleGroup.ChildGroups != null)
                foreach (var childGroup in pRuleGroup.ChildGroups)
                {
                    var expr = GetGroupExpression(childGroup, pExpressionCallback);

                    if (pRuleGroup.Operator == MQueryBuilderRuleGroupOperator.Or)
                    {
                        groupExpr = groupExpr.Or(expr);
                    }
                    else
                    {
                        groupExpr = groupExpr.And(expr);
                    }
                }

            return groupExpr;
        }
    }
}
