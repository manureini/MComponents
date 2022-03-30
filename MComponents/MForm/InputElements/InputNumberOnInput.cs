using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;

namespace MComponents.InputElements
{
    public partial class InputNumberOnInput<T> : InputNumber<T>
    {
        private static readonly string _stepAttributeValue = GetStepAttributeValue();

        private static string GetStepAttributeValue()
        {
            // Unwrap Nullable<T>, because InputBase already deals with the Nullable aspect
            // of it for us. We will only get asked to parse the T for nonempty inputs.
            var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            if (targetType == typeof(int) ||
                targetType == typeof(long) ||
                targetType == typeof(short) ||
                targetType == typeof(float) ||
                targetType == typeof(double) ||
                targetType == typeof(decimal))
            {
                return "any";
            }
            else
            {
                throw new InvalidOperationException($"The type '{targetType}' is not a supported numeric type.");
            }
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            /*
            __builder.OpenElement(0, "input");
            __builder.AddMultipleAttributes(1, AdditionalAttributes);
            __builder.AddAttribute(2, "type", "number");
            __builder.AddAttribute(3, "class", CssClass);
            __builder.AddAttribute(4, "value", stringValue);
            __builder.AddAttribute(5, "oninput", EventCallback.Factory.Create<ChangeEventArgs>(this, OnInput));
            __builder.AddAttribute(6, "onblur", EventCallback.Factory.Create<FocusEventArgs>(this, OnBlur));
            __builder.CloseElement();
            */

            builder.OpenElement(0, "input");
            builder.AddAttribute(1, "step", _stepAttributeValue);
            builder.AddMultipleAttributes(2, AdditionalAttributes);
            builder.AddAttribute(3, "type", "number");

            if (!string.IsNullOrEmpty(CssClass))
            {
                builder.AddAttribute(4, "class", CssClass);
            }

            //   builder.AddAttribute(5, "value", BindConverter.FormatValue(CurrentValueAsString));
            //   builder.AddAttribute(6, "onchange", EventCallback.Factory.CreateBinder<string?>(this, __value => CurrentValueAsString = __value, CurrentValueAsString));

            builder.AddAttribute(5, "value", stringValue);
            builder.AddAttribute(6, "oninput", EventCallback.Factory.Create<ChangeEventArgs>(this, OnInput));
            builder.AddAttribute(7, "onblur", EventCallback.Factory.Create<FocusEventArgs>(this, OnBlur));

#if NET6_0_OR_GREATER
            builder.AddElementReferenceCapture(8, __inputReference => this.Element = __inputReference);
#endif

            builder.CloseElement();
        }

        private string stringValue;
        private T lastParsedValue;

        protected override void OnParametersSet()
        {
            // Only overwrite the "stringValue" when the Value is different
            if (!Equals(CurrentValue, lastParsedValue))
            {
                lastParsedValue = CurrentValue;
                stringValue = CurrentValueAsString;
            }
        }

        private void OnInput(ChangeEventArgs e)
        {
            stringValue = (string)e.Value;

            CurrentValueAsString = stringValue;
            lastParsedValue = CurrentValue;
        }

        private void OnBlur(FocusEventArgs e)
        {
            // Overwrite the stringValue property with the parsed value.
            // This call Value.ToString(), so the value in the input is well formatted.
            // note: Ensure the string value is valid before updating the content
            if (!EditContext.GetValidationMessages(FieldIdentifier).Any())
            {
                stringValue = CurrentValueAsString;
            }
        }
    }
}
