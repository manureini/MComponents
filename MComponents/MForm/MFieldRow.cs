using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MComponents.MForm
{
    public class MFieldRow : ComponentBase
    {
        [Parameter]
        public RenderFragment ChildContent { get; set; }

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
                    mForm.RegisterRow(this);
                }
            }
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);

            RenderFragment child3() =>
                    (builder2) =>
                    {
                        builder2.AddMarkupContent(1, "\r\n");
                        builder2.AddContent(2, ChildContent);
                        builder2.AddMarkupContent(3, "\r\n");
                    };

            builder.OpenComponent<CascadingValue<MFieldRow>>(4);
            builder.AddAttribute(5, "Value", this);
            builder.AddAttribute(6, "ChildContent", child3());
            builder.CloseComponent();
        }
    }
}
