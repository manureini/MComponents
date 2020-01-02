using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;

namespace MComponents.MForm
{
    public class MField : ComponentBase, IMPropertyField
    {
        [Parameter]
        public string Property { get; set; }

        [Parameter]
        public virtual Type PropertyType { get; set; }

        [Parameter]
        public Attribute[] Attributes { get; set; }

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
