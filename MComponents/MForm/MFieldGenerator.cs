using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

namespace MComponents.MForm
{
    public class MFieldGenerator : ComponentBase, IMFieldGenerator
    {
        [Parameter]
        public virtual Attribute[] Attributes { get; set; }

        [Parameter]
        public virtual RenderFragment<MFieldGeneratorContext> Template { get; set; }

        [Parameter(CaptureUnmatchedValues = true)]
        public virtual IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

        private IMForm mForm;

        [CascadingParameter]
        public virtual IMForm Form
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
