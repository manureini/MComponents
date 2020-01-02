using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

namespace MComponents.MForm
{
    public class MFieldGenerator<T> : ComponentBase, IMFieldGenerator<T>
    {
        [Parameter]
        public Attribute[] Attributes { get; set; }

        [Parameter]
        public RenderFragment<MFieldGeneratorContext<T>> Template { get; set; }

        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

        private IMForm mForm;

        [CascadingParameter]
        public IMForm Form
        {
            get
            {
                return mForm;
            }
            set
            {
                if (value != mForm)
                {
                    mForm = value;
                    mForm.RegisterField(this);
                }
            }
        }

    }
}
