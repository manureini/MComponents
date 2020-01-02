using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using System;

namespace MComponents.InputElements
{
    public class InputGuid<T> : InputBase<T>
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "input");
            builder.AddMultipleAttributes(1, AdditionalAttributes);
            builder.AddAttribute(2, "class", CssClass);
            builder.AddAttribute(3, "value", BindConverter.FormatValue(CurrentValue));
            builder.AddAttribute(4, "onchange", EventCallback.Factory.CreateBinder<string>(this, __value => CurrentValueAsString = __value, CurrentValueAsString));
            builder.CloseElement();
        }

        protected override bool TryParseValueFromString(string value, out T result, out string validationErrorMessage)
        {
            if (!Guid.TryParse(value, out Guid val))
            {
                validationErrorMessage = "Could not parse Guid";
                result = default(T);
                return false;
            }
            result = (T)(object)val;
            validationErrorMessage = null;
            return true;
        }
    }

}