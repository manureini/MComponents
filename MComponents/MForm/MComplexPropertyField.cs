using Microsoft.AspNetCore.Components;
using System;

namespace MComponents.MForm
{
    public class MComplexPropertyField<T> : MField, IMComplexField
    {
        [Parameter]
        public RenderFragment<MComplexPropertyFieldContext<T>> Template { get; set; }

        public override Type PropertyType
        {
            get
            {
                return typeof(T);
            }
            set
            {
               // throw new InvalidOperationException("Can't set type for Complex Field");
            }
        }
    }
}
