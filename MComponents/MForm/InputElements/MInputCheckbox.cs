﻿using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;

namespace MComponents.InputElements
{
    public class MInputCheckbox : InputCheckbox
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(10, "span");
            builder.AddAttribute(11, "class", "m-form-control m-checkbox m-checkbox--icon");

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
