using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MComponents.ExampleApp.Data
{
    public class QueryExpressionVisitor<T> : ExpressionVisitor where T : class
    {
        protected IQueryable<T> mDataSource;

        public QueryExpressionVisitor(IQueryable<T> pDataSource)
        {
            mDataSource = pDataSource;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {

            if (node == null)
                return node;

            if (node.Type.IsGenericType && node.Type.GetGenericTypeDefinition() == typeof(EnumerableQuery<>))
            {
                return Expression.Constant(mDataSource);
            }

            return base.VisitConstant(node);
        }
    }
}
