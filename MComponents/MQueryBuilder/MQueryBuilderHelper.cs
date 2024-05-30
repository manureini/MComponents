using LinqKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;

namespace MComponents.MQueryBuilder
{
    public class MQueryBuilderHelper
    {
        public static IQueryable<T> ApplyRules<T>(IQueryable<T> pDataSource, MQueryBuilderRuleGroup pRules, Func<string, Expression<Func<T, object, object>>> pExpressionCallback, Func<object, object> pConditionValueModifier = null)
        {
            var expandable = pDataSource.AsExpandable();

            var paramExpr = Expression.Parameter(typeof(T), "p");
            var groupExpr = GetGroupExpression(pRules, paramExpr, pExpressionCallback, pConditionValueModifier);

            if (groupExpr == null)
            {
                groupExpr = Expression.Constant(true);
            }

            var expr = (Expression<Func<T, bool>>)Expression.Lambda(groupExpr, paramExpr);

            return expandable.Where(expr).AsQueryable();
        }

        private static Expression GetGroupExpression<T>(MQueryBuilderRuleGroup pRuleGroup, ParameterExpression pParameterExpression, Func<string, Expression<Func<T, object, object>>> pExpressionCallback, Func<object, object> pConditionValueModifier)
        {
            var parameterExpressions = new List<ParameterExpression>();

            Expression groupExpr = null;

            foreach (var condition in pRuleGroup.Conditions)
            {
                var expr = pExpressionCallback(condition.RuleName);

                if (expr != null)
                {
                    parameterExpressions.Add(expr.Parameters[0]);

                    var val = condition.Value;

                    if (condition.ValueTypeName != null)
                    {
                        val = ReflectionHelper.ChangeType(val, Type.GetType(condition.ValueTypeName));
                    }

                    if (pConditionValueModifier != null)
                    {
                        val = pConditionValueModifier(val);
                    }

                    var valueExpr = Expression.Constant(val);

                    var containsVisitor = new ExpressionContainsExpressionParameter(expr.Parameters[1]);
                    containsVisitor.Visit(expr.Body);
                    bool containValueExpr = containsVisitor.ContainsExpressionParameter;

                    Expression operatorExpr = null;

                    if (containValueExpr)
                    {
                        var valCast = Expression.Convert(valueExpr, typeof(object));
                        operatorExpr = Expression.Convert(Expression.Invoke(expr, pParameterExpression, valCast), typeof(bool));
                    }
                    else
                    {
                        operatorExpr = condition.Operator switch
                        {
                            MQueryBuilderConditionOperator.NotEqual => Expression.NotEqual(expr.Body, valueExpr),

                            MQueryBuilderConditionOperator.GreaterThan => Expression.GreaterThan(expr.Body, valueExpr),
                            MQueryBuilderConditionOperator.GreaterThanOrEqual => Expression.GreaterThanOrEqual(expr.Body, valueExpr),
                            MQueryBuilderConditionOperator.LessThan => Expression.LessThan(expr.Body, valueExpr),
                            MQueryBuilderConditionOperator.LessThanOrEqual => Expression.LessThanOrEqual(expr.Body, valueExpr),

                            MQueryBuilderConditionOperator.StartsWith => Expression.Call(expr.Body, typeof(string).GetMethod(nameof(string.StartsWith), new Type[] { typeof(string) }), Expression.Constant(val, typeof(string))),
                            MQueryBuilderConditionOperator.EndsWith => Expression.Call(expr.Body, typeof(string).GetMethod(nameof(string.EndsWith), new Type[] { typeof(string) }), Expression.Constant(val, typeof(string))),
                            MQueryBuilderConditionOperator.Contains => Expression.Call(expr.Body, typeof(string).GetMethod(nameof(string.Contains), new Type[] { typeof(string) }), Expression.Constant(val, typeof(string))),

                            _ => Expression.Equal(expr.Body, valueExpr),
                        };
                    }

                    groupExpr = AppendExpression(groupExpr, operatorExpr, pRuleGroup.Operator);
                }
            }

            if (pRuleGroup.ChildGroups != null)
                foreach (var childGroup in pRuleGroup.ChildGroups)
                {
                    var expr = GetGroupExpression(childGroup, pParameterExpression, pExpressionCallback, pConditionValueModifier);

                    if (expr != null)
                    {
                        groupExpr = AppendExpression(groupExpr, expr, pRuleGroup.Operator);
                    }
                }

            if (groupExpr == null)
                return null;

            return new ExpressionParameterReplacer(parameterExpressions, pParameterExpression).Visit(groupExpr);
        }

        private static Expression AppendExpression(Expression left, Expression right, MQueryBuilderRuleGroupOperator ruleOperator)
        {
            if (left == null)
                return right;

            if (ruleOperator == MQueryBuilderRuleGroupOperator.Or)
            {
                return Expression.Or(left, right);
            }

            return Expression.And(left, right);
        }

        internal class ExpressionParameterReplacer : ExpressionVisitor
        {
            private readonly List<ParameterExpression> mOldParameterExpressions;
            private readonly Expression mNewExpression;

            public ExpressionParameterReplacer(List<ParameterExpression> pOldParameterExpressions, Expression pNewExpression)
            {
                mOldParameterExpressions = pOldParameterExpressions;
                mNewExpression = pNewExpression;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (mOldParameterExpressions.Contains(node))
                {
                    return mNewExpression;
                }

                return base.VisitParameter(node);
            }
        }

        internal class ExpressionContainsExpressionParameter : ExpressionVisitor
        {
            private readonly ParameterExpression mExpression;

            public bool ContainsExpressionParameter { get; set; }

            public ExpressionContainsExpressionParameter(ParameterExpression pExpression)
            {
                mExpression = pExpression;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (mExpression == node)
                {
                    ContainsExpressionParameter = true;
                }

                return base.VisitParameter(node);
            }
        }
    }
}
