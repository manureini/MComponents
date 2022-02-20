using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;

namespace MComponents.MForm
{
    public class MFieldGenerator : ComponentBase, IMFieldGenerator
    {
        [Parameter]
        public Attribute[] Attributes { get; set; }

        [Parameter]
        public RenderFragment<MFieldGeneratorContext> Template { get; set; }

        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

        [CascadingParameter]
        public MFieldRow FieldRow { get; set; }

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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                mForm?.UnregisterField(this);
            }
        }
    }
}
