using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using System.Collections.Generic;
using System.Linq;

namespace MComponents
{
    public class MFormContainer : ComponentBase
    {
        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

        [Parameter]
        public RenderFragment<MFormContainerContext> ChildContent { get; set; }

        protected MFormContainerContext mFormContext;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            mFormContext = new MFormContainerContext();
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenRegion(mFormContext.GetHashCode());

            builder.OpenElement(0, "div");
            builder.AddMultipleAttributes(1, AdditionalAttributes);

            builder.OpenComponent<CascadingValue<MFormContainerContext>>(3);
            builder.AddAttribute(4, "IsFixed", true);
            builder.AddAttribute(5, "Value", mFormContext);
            builder.AddAttribute(6, "ChildContent", ChildContent.Invoke(mFormContext));
            builder.CloseComponent();

            builder.CloseElement();

            builder.OpenElement(19, "button");
            builder.AddAttribute(20, "type", "button");
            builder.AddAttribute(20, "class", "btn btn-primary");
            builder.AddAttribute(21, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, Click));
            builder.AddContent(22, "Save");
            builder.CloseElement();

            builder.CloseRegion();
        }

        protected void Click(MouseEventArgs pArgs)
        {
            TrySubmit();
        }

        public bool TrySubmit()
        {
            return mFormContext.NotifySubmit();
        }

        public bool HasUnsavedChanges
        {
            get { return mFormContext.Forms.Any(f => f.HasUnsavedChanges); }
        }
    }
}
