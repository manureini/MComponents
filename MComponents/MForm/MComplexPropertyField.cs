﻿using Microsoft.AspNetCore.Components;
using System;

namespace MComponents.MForm
{
    public class MComplexPropertyField<TProperty> : MField, IMComplexField
    {
        [Parameter]
        public RenderFragment<MComplexPropertyFieldContext<TProperty>> Template { get; set; }

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
