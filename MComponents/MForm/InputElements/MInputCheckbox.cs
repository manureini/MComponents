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
            builder.AddAttribute(5, "style", "text-align: center; display: block;");

            builder.OpenElement(0, "label");
            builder.AddAttribute(4, "style", "margin-bottom: 0;");

            base.BuildRenderTree(builder);

            builder.OpenElement(4, "span");
            builder.AddAttribute(3, "class", "fa"); //TODO <- WTF fa?
            builder.AddAttribute(5, "style", "padding: 0; margin-top: 3px;");

            builder.CloseElement();

            builder.CloseElement();

            builder.CloseElement();
        }


    }

}
