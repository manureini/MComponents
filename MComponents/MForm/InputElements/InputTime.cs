using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using System;

namespace MComponents.InputElements
{
    public class InputTime<T> : InputBase<T>
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(12, "input");
            builder.AddMultipleAttributes(13, AdditionalAttributes);
            builder.AddAttribute(14, "class", CssClass);
            builder.AddAttribute(15, "type", "time");
            builder.AddAttribute(16, "value", FormatValueAsString(CurrentValue)); //custom format
            builder.AddAttribute(17, "onchange", EventCallback.Factory.CreateBinder<string>(this, pValue => CurrentValueAsString = pValue, CurrentValueAsString));
            builder.AddAttribute(18, "onfocus", EventCallback.Factory.Create<FocusEventArgs>(this, _ => SetDefaultTimeIfNull()));
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

            if(DateTime.TryParse(value, out var datetime))
            {
                result = (T)((object)datetime);
                return true;
            }

            result = default;
            return false;
        }

        protected override string FormatValueAsString(T value)
        {
            if (value == null)
                return null;

            dynamic v = value;
            return v.ToString("HH:mm");
        }

        protected void SetDefaultTimeIfNull()
        {
            if (string.IsNullOrEmpty(CurrentValueAsString))
            {
                CurrentValueAsString = "00:00";
            }
        }
    }

}