using Microsoft.AspNetCore.Components;
using System;
using System.Linq.Expressions;

namespace MComponents.MForm
{
    public class MComplexPropertyFieldContext<T, TProperty>
    {
        public T Row { get; set; }

        public TProperty Value { get; set; }

        public EventCallback<TProperty> ValueChanged { get; set; }

        public Expression<Func<TProperty>> ValueExpression { get; set; }

        public Guid InputId { get; set; }

        public MFormGridContext MFormGridContext { get; set; }
    }
}
