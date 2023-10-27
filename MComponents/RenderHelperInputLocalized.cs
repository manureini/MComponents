using MComponents.MForm;
using MComponents.MForm.InputElements;
using MComponents.MGrid;
using MComponents.Shared.Attributes;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.CompilerServices;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Globalization;
using System.Linq;

namespace MComponents
{
    internal static class RenderHelperInputLocalized
    {
        public static void AppendInput<T>(RenderTreeBuilder pBuilder, IMPropertyInfo pPropertyInfo, object pModel, IMForm pParent, IMField pField, bool pUpdateOnInput, CultureInfo[] pCultures)
        {
            bool isReadOnly = pPropertyInfo.IsReadOnly || pPropertyInfo.GetCustomAttribute<ReadOnlyAttribute>() != null;

            foreach (var culture in pCultures)
            {
                RenderInputForLanguage<T>(pBuilder, pPropertyInfo, pModel, pParent, pField, pUpdateOnInput, isReadOnly, culture);
            }
        }

        private static void RenderInputForLanguage<T>(RenderTreeBuilder pBuilder, IMPropertyInfo pPropertyInfo, object pModel, IMForm pParent, IMField pField, bool pUpdateOnInput, bool isReadOnly, CultureInfo pCulture)
        {
            var id = Guid.NewGuid();

            T value = (T)(object)LocalizationHelper.GetLocalizedStringValue(pModel, pPropertyInfo.Name, pCulture);

            if (pPropertyInfo.GetCustomAttribute<TextAreaAttribute>() != null)
            {
                pBuilder.OpenComponent<InputTextArea>(0);
            }
            else
            {
                if (pUpdateOnInput)
                {
                    pBuilder.OpenComponent<InputTextOnInput>(0);
                }
                else
                {
                    pBuilder.OpenComponent<InputText>(0);
                }
            }

            if (pField.AdditionalAttributes != null)
            {
                pBuilder.AddMultipleAttributes(17, pField.AdditionalAttributes
                    .Where(a => a.Key != Extensions.MFORM_IN_TABLE_ROW_TD_STYLE_ATTRIBUTE)
                    .Where(a => a.Key != nameof(IMGridColumn))
                    .ToDictionary(a => a.Key, a => a.Value));
            }

            pBuilder.AddAttribute(178, "id", id);
            pBuilder.AddAttribute(179, "Value", value);

            pBuilder.AddAttribute(181, "form", pParent.Id);

            pBuilder.AddAttribute(183, "ValueChanged", RuntimeHelpers.CreateInferredEventCallback<T>(pParent, async __value =>
            {
                LocalizationHelper.SetLocalizedStringValue(pModel, pPropertyInfo.Name, (string)(object)__value, pCulture);

                var locProp = ReflectionHelper.GetIMPropertyInfo(pModel.GetType(), LocalizationHelper.GetLocPropertyName(pPropertyInfo.Name), typeof(T));
                await pParent.OnInputValueChanged(new MFieldEmpty(), locProp, null);

                if (pCulture.TwoLetterISOLanguageName == CultureInfo.CurrentUICulture.TwoLetterISOLanguageName)
                {
                    await pParent.OnInputValueChanged(pField, pPropertyInfo, __value);
                }
            }, value));

            pBuilder.AddAttribute(188, "onkeyup", EventCallback.Factory.Create<KeyboardEventArgs>(pParent, (a) =>
            {
                // pParent.OnInputKeyUp(a, pPropertyInfo);
            }));

            var valueExpression = RenderHelper.GetFakePropertyInfoExpression<T>(pModel, pPropertyInfo.Name + "_" + pCulture.TwoLetterISOLanguageName);

            pBuilder.AddAttribute(195, "ValueExpression", global::Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck<global::System.Linq.Expressions.Expression<System.Func<T>>>(valueExpression));

            var regionInfo = new RegionInfo(pCulture.Name);

            string cssClass = "m-form-control m-flag m-flag-" + regionInfo.TwoLetterISORegionName.ToLowerInvariant();

            if (isReadOnly)
            {
                pBuilder.AddAttribute(33, "disabled", string.Empty);
                pBuilder.AddAttribute(33, "IsDisabled", true);
            }

            pBuilder.AddAttribute(10, "class", cssClass);
            pBuilder.AddAttribute(10, "placeholder", pCulture.NativeName);

            // pBuilder.SetUpdatesAttributeName(pPropertyInfo.Name); <- new code generator will add this, but I don't know why

            pBuilder.CloseComponent();

            if (pParent.EnableValidation)
            {
                pBuilder.OpenComponent<ValidationMessage<T>>(60);
                pBuilder.AddAttribute(61, "For", valueExpression);
                pBuilder.CloseComponent();
            }
        }
    }
}
