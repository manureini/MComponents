using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using System;

namespace MComponents.InputElements
{
    public class InputDateTime<T> : InputBase<T>
    {
        //datetime-local is not supported in firefox - maybe in 10 years or something.....
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            /*
            builder.OpenElement(0, "input");
            builder.AddMultipleAttributes(1, AdditionalAttributes);
            builder.AddAttribute(2, "class", CssClass);
            builder.AddAttribute(2, "type", "datetime-local");
            builder.AddAttribute(3, "value", FormatValueAsString(CurrentValue)); //custom format
            builder.AddAttribute(4, "onchange", EventCallback.Factory.CreateBinder<string>(this, __value => CurrentValueAsString = __value, CurrentValueAsString));
            builder.CloseElement();
            */

            builder.OpenElement(0, "div");
            builder.AddAttribute(24, "class", "m-combined-input-container " + CssClass);

            builder.OpenComponent<InputDate<T>>(20);
            builder.AddAttribute(21, "Value", CurrentValue);
            builder.AddAttribute(22, "ValueChanged", ValueChanged);
            builder.AddAttribute(23, "ValueExpression", ValueExpression);
            builder.AddAttribute(24, "class", "m-form-control m-combined-input");
            builder.AddAttribute(25, "style", "width: 66%;");
            builder.CloseComponent();

            builder.OpenComponent<InputTime<T>>(30);
            builder.AddAttribute(31, "Value", CurrentValue);
            builder.AddAttribute(32, "ValueChanged", ValueChanged);
            builder.AddAttribute(33, "ValueExpression", ValueExpression);
            builder.AddAttribute(34, "class", "m-form-control m-combined-input");
            builder.AddAttribute(35, "style", "width: 34%;");
            builder.CloseComponent();

            builder.CloseElement(); //div
        }

        protected override bool TryParseValueFromString(string value, out T result, out string validationErrorMessage)
        {
            /*
            validationErrorMessage = null;

            if (value == null || value == string.Empty)
            {
                result = default;
                return false;
            }

            if (!DateTime.TryParse(value, out DateTime datetime))
            {
                result = default;
                return false;
            }

            result = (T)(object)datetime;
            return true;
            */
            throw new NotImplementedException();
        }

        protected override string FormatValueAsString(T value)
        {/*
            if (value == null)
                return null;

            dynamic v = value;
            return v.ToString("yyyy-MM-ddTHH:mm");
            */
            throw new NotImplementedException();
        }
    }
}