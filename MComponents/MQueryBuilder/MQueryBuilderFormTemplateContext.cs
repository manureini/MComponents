using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MComponents.MQueryBuilder
{
    public class MQueryBuilderFormTemplateContext<TProperty>
    {
        public TProperty Value { get; set; }

        public EventCallback<TProperty> ValueChanged { get; set; }

        public Expression<Func<TProperty>> ValueExpression { get; set; }
    }
}
