using MComponents.MForm;
using MComponents.MForm.InputElements;
using MComponents.MGrid;
using MComponents.Services;
using MComponents.Shared.Attributes;
using MComponents.Tabs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.CompilerServices;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Globalization;
using System.Linq;

namespace MComponents
{
    internal static class RenderHelperInputLocalized
    {
        public static void AppendInput<T>(RenderTreeBuilder pBuilder, IMPropertyInfo pPropertyInfo, object pModel, IMForm pParent, IMField pField, bool pUpdateOnInput, MComponentSettings pSettings, MFormGridContext pGridContext)
        {
            bool isReadOnly = pPropertyInfo.IsReadOnly || pPropertyInfo.GetCustomAttribute<ReadOnlyAttribute>() != null;

            bool useTabs = pPropertyInfo.GetCustomAttribute<TextAreaAttribute>() != null;

            if (useTabs)
            {
                RenderFragment child() => (builder2) =>
                {
                    foreach (var culture in pSettings.SupportedCultures)
                    {
                        RenderFragment childTab() => (builder3) =>
                           {
                               if (pSettings.CustomTextAreaComponent != null)
                               {
                                   RenderComplexInputForLanguage<T>(builder3, pPropertyInfo, pModel, pParent, pField, isReadOnly, pGridContext, culture, pSettings);
                               }
                               else
                               {
                                   RenderInputForLanguage<T>(builder3, pPropertyInfo, pModel, pParent, pField, pUpdateOnInput, isReadOnly, culture, pSettings);
                               }
                           };

                        builder2.OpenComponent<MTab>(10);
                        builder2.AddAttribute(11, nameof(MTab.Title), culture.Parent.NativeName);
                        builder2.AddAttribute(12, nameof(MTab.CssClassButton), GetCssFlagClass(culture));
                        builder2.AddAttribute(13, nameof(MTab.ChildContent), childTab());
                        builder2.CloseComponent(); //MTab
                    }
                };

                pBuilder.OpenComponent<MTabs>(0);
                pBuilder.AddAttribute(1, nameof(MTabs.ChildContent), child());
                pBuilder.AddAttribute(2, nameof(MTabs.CssClass), "m-localization-tab");
                pBuilder.CloseComponent(); //MTabs

                return;
            }

            foreach (var culture in pSettings.SupportedCultures)
            {
                RenderInputForLanguage<T>(pBuilder, pPropertyInfo, pModel, pParent, pField, pUpdateOnInput, isReadOnly, culture, pSettings);
            }
        }

        private static void RenderComplexInputForLanguage<T>(RenderTreeBuilder pBuilder, IMPropertyInfo pPropertyInfo, object pModel, IMForm pParent, IMField pField, bool isReadOnly, MFormGridContext pGridContext, CultureInfo pCulture, MComponentSettings pSettings)
        {
            var id = Guid.NewGuid();

            var value = (string)(object)LocalizationHelper.GetLocalizedStringValue(pModel, pPropertyInfo.Name, pCulture);

            var valueExpression = RenderHelper.GetFakePropertyInfoExpression<string>(pModel, pPropertyInfo.Name + "_" + pCulture.TwoLetterISOLanguageName);

            var context = new MComplexPropertyFieldContext<string>
            {
                Row = pModel,
                InputId = id.ToString(),
                FormId = pParent.Id.ToString(),
                Form = pParent,
                Value = value,
                MFormGridContext = pGridContext,
                ValueChanged = GetValueChangedCallback(pPropertyInfo, pModel, pParent, pField, pCulture, value),
                ValueExpression = valueExpression,
                IsReadOnly = isReadOnly,
            };

            pBuilder.AddContent(263, pSettings.CustomTextAreaComponent.Invoke(context));

            if (pParent.EnableValidation)
            {
                pBuilder.OpenComponent<ValidationMessage<string>>(236);
                pBuilder.AddAttribute(237, "For", context.ValueExpression);
                pBuilder.CloseComponent();
            }
        }

        private static void RenderInputForLanguage<T>(RenderTreeBuilder pBuilder, IMPropertyInfo pPropertyInfo, object pModel, IMForm pParent, IMField pField, bool pUpdateOnInput, bool isReadOnly, CultureInfo pCulture, MComponentSettings pSettings)
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

            pBuilder.AddAttribute(183, "ValueChanged", GetValueChangedCallback(pPropertyInfo, pModel, pParent, pField, pCulture, value));

            var valueExpression = RenderHelper.GetFakePropertyInfoExpression<T>(pModel, pPropertyInfo.Name + "_" + pCulture.TwoLetterISOLanguageName);

            pBuilder.AddAttribute(195, "ValueExpression", global::Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck<global::System.Linq.Expressions.Expression<System.Func<T>>>(valueExpression));

            if (isReadOnly)
            {
                pBuilder.AddAttribute(33, "disabled", string.Empty);
                pBuilder.AddAttribute(33, "IsDisabled", true);
            }

            pBuilder.AddAttribute(10, "class", "m-form-control " + GetCssFlagClass(pCulture));
            pBuilder.AddAttribute(10, "placeholder", pCulture.Parent.NativeName);

            // pBuilder.SetUpdatesAttributeName(pPropertyInfo.Name); <- new code generator will add this, but I don't know why

            pBuilder.CloseComponent();

            if (pParent.EnableValidation)
            {
                pBuilder.OpenComponent<ValidationMessage<T>>(60);
                pBuilder.AddAttribute(61, "For", valueExpression);
                pBuilder.CloseComponent();
            }
        }

        private static string GetCssFlagClass(CultureInfo pCulture)
        {
            var regionInfo = new RegionInfo(pCulture.Name);
            return "m-flag m-flag-" + regionInfo.TwoLetterISORegionName.ToLowerInvariant();
        }

        private static EventCallback<T> GetValueChangedCallback<T>(IMPropertyInfo pPropertyInfo, object pModel, IMForm pParent, IMField pField, CultureInfo pCulture, T value)
        {
            return RuntimeHelpers.CreateInferredEventCallback<T>(pParent, async __value =>
            {
                LocalizationHelper.SetLocalizedStringValue(pModel, pPropertyInfo.Name, (string)(object)__value, pCulture);

                var locProp = ReflectionHelper.GetIMPropertyInfo(pModel.GetType(), LocalizationHelper.GetLocPropertyName(pPropertyInfo.Name), typeof(T));
                await pParent.OnInputValueChanged(new MFieldEmpty(), locProp, null);

                if (pCulture.TwoLetterISOLanguageName == CultureInfo.CurrentUICulture.TwoLetterISOLanguageName)
                {
                    await pParent.OnInputValueChanged(pField, pPropertyInfo, __value);
                }
            }, value);
        }
    }
}
