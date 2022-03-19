using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;

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

        [Parameter]
        public bool ExtendAttributes { get; set; }

        [Parameter]
        public RenderFragment TemplateAfterLabel { get; set; }

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

        [CascadingParameter]
        public MFieldRow FieldRow { get; set; }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            if (Form == null)
                throw new ArgumentNullException(nameof(Form), $"No cascading {nameof(Form)} value found. Please ensure field is inside a form.");

            if (Property != null)
            {
                if (ExtendAttributes && Attributes != null)
                {
                    var pi = ReflectionHelper.GetIMPropertyInfo(Form.ModelType, Property, PropertyType);
                    Attributes = Attributes.Concat(pi.GetAttributes()).ToArray();
                }
                else if (Attributes == null)
                {
                    var pi = ReflectionHelper.GetIMPropertyInfo(Form.ModelType, Property, PropertyType);
                    Attributes = pi.GetAttributes().ToArray();
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
