using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;

namespace MComponents.InputElements
{
    public class MInputCheckbox : InputCheckbox
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "span");
            builder.AddAttribute(3, "class", "m-switch m-switch--icon");

            builder.OpenElement(0, "label");

            base.BuildRenderTree(builder);

            builder.OpenElement(4, "span");
            builder.AddAttribute(4, "class", "fa");
            builder.CloseElement(); //span

            builder.CloseElement(); //label

            builder.CloseElement(); //span
        }
    }
}
