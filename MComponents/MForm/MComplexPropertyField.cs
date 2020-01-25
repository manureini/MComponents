using Microsoft.AspNetCore.Components;
using System;

namespace MComponents.MForm
{
    public class MComplexPropertyField<T, TProperty> : MField, IMComplexField
    {
        [Parameter]
        public RenderFragment<MComplexPropertyFieldContext<T, TProperty>> Template { get; set; }

        public override Type PropertyType
        {
            get
            {
                return typeof(TProperty);
            }
            set
            {
                // throw new InvalidOperationException("Can't set type for Complex Field");
            }
        }
    }
}
