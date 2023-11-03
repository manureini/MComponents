using Microsoft.AspNetCore.Components;
using System;
using System.Linq.Expressions;

namespace MComponents.MForm
{
    public class MComplexPropertyFieldContext<TProperty>
    {
        public dynamic Row { get; set; }

        public TProperty Value { get; set; }

        public EventCallback<TProperty> ValueChanged { get; set; }

        public Expression<Func<TProperty>> ValueExpression { get; set; }

        public string InputId { get; set; }

        public string FormId { get; set; }

        public IMForm Form { get; set; }

        public MFormGridContext MFormGridContext { get; set; }

        public bool IsReadOnly { get; set; }
    }
}
