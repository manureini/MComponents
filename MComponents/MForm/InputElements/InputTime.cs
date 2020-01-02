using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using System;

namespace MComponents.InputElements
{
    public class InputTime<T> : InputBase<T>
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "input");
            builder.AddMultipleAttributes(1, AdditionalAttributes);
            builder.AddAttribute(2, "class", CssClass);
            builder.AddAttribute(2, "type", "time");
            builder.AddAttribute(3, "value", FormatValueAsString(CurrentValue)); //custom format
            builder.AddAttribute(4, "onchange", EventCallback.Factory.CreateBinder<string>(this, __value => CurrentValueAsString = __value, CurrentValueAsString));
            builder.CloseElement();
        }

        protected override bool TryParseValueFromString(string value, out T result, out string validationErrorMessage)
        {
            validationErrorMessage = null;

            if (value == null || value == string.Empty)
            {
                result = default;
                return false;
            }

            object datetime = DateTime.Parse(value);

            result = (T)datetime;
            return true;
        }

        protected override string FormatValueAsString(T value)
        {
            if (value == null)
                return null; 

            dynamic v = value;
            return v.ToString("HH:mm");
        }
    }

}