using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Linq;

namespace MComponents.InputElements
{
    public class InputType : InputBase<Type>
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

        protected override bool TryParseValueFromString(string value, out Type result, out string validationErrorMessage)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                result = null;
                validationErrorMessage = null;
                return true;
            }

            result = FindType(value);

            if (result == null)
            {
                validationErrorMessage = "Type not found!";
                return false;
            }

            validationErrorMessage = null;
            return true;
        }

        //where should we put this code? Use better version from coreflow?
        public static Type FindType(string pTypeName)
        {
            var type = Type.GetType(pTypeName);

            if (type != null)
                return type;

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetTypes().FirstOrDefault(t => t.FullName == pTypeName);

                if (type != null)
                    return type;
            }

            return null;
        }
    }
}