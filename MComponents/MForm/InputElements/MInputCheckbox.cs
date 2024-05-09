using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;

namespace MComponents.InputElements
{
    public class MInputCheckbox : InputCheckbox
    {
        [Parameter]
        public string SpanCssClass { get; set; } = "m-form-control m-checkbox m-checkbox--icon";

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(10, "span");
            builder.AddAttribute(11, "class", SpanCssClass);

            builder.OpenElement(13, "label");

            base.BuildRenderTree(builder);

            builder.OpenElement(17, "span");
            builder.AddAttribute(18, "class", "fa");
            builder.CloseElement(); //span

            builder.CloseElement(); //label

            builder.CloseElement(); //span
        }
    }
}
