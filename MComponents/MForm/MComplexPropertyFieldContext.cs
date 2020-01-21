using Microsoft.AspNetCore.Components;
using System;
using System.Linq.Expressions;

namespace MComponents.MForm
{
    public class MComplexPropertyFieldContext<T>
    {
        public T Value { get; set; }

        public EventCallback<T> ValueChanged { get; set; }

        public Expression<Func<T>> ValueExpression { get; set; }

        public Guid InputId { get; set; }

        public MFormGridContext MFormGridContext { get; set; }
    }
}
