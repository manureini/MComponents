using MComponents.Shared.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace MComponents
{
    public static class LocalizationHelper
    {
        public static string GetLocPropertyName(string pProperty)
        {
            return pProperty + "Loc";
        }

        public static void SyncLocalizedStrings(object pObject, IDictionary<string, object> pChangedValues = null)
        {
            foreach (var prop in pObject.GetType().GetProperties().Where(p => p.GetCustomAttribute<LocalizedStringAttribute>() != null))
            {
                SyncLocalizedStrings(pObject, prop, pChangedValues);
            }
        }

        public static void SyncLocalizedStrings(object pObject, PropertyInfo pPropertyInfo, IDictionary<string, object> pChangedValues = null)
        {
            var value = (string)pPropertyInfo.GetValue(pObject);

            if (value != null)
            {
                var oldValue = GetLocalizedStringValue(pObject, pPropertyInfo.Name);
                if (oldValue != value)
                {
                    SetLocalizedStringValue(pObject, pPropertyInfo.Name, value);

                    if (pChangedValues != null && !pChangedValues.ContainsKey(GetLocPropertyName(pPropertyInfo.Name)))
                    {
                        var locProp = ReflectionHelper.GetIMPropertyInfo(pObject.GetType(), GetLocPropertyName(pPropertyInfo.Name), typeof(JsonDocument));
                        pChangedValues.Add(locProp.Name, locProp.GetValue(pObject));
                    }
                }
            }
            else
            {
                value = GetLocalizedStringValue(pObject, pPropertyInfo.Name);
                pPropertyInfo.SetValue(pObject, value);
            }
        }

        public static void SetLocalizedStringValue(object pObject, string pPropertyName, string pValue, CultureInfo pCulture = null)
        {
            var prop = ReflectionHelper.GetIMPropertyInfo(pObject.GetType(), GetLocPropertyName(pPropertyName), typeof(JsonDocument));

            var locValuesJson = (JsonDocument)prop.GetValue(pObject) ?? JsonDocument.Parse("{}");

            var culture = (pCulture ?? CultureInfo.CurrentUICulture).TwoLetterISOLanguageName;

            var locValues = locValuesJson.RootElement.Deserialize<Dictionary<string, string>>();

            locValues.Remove(culture);
            locValues.Add(culture, pValue);

            var json = JsonSerializer.Serialize(locValues);

            var jsonDoc = JsonDocument.Parse(json);

            prop.SetValue(pObject, jsonDoc);
        }

        public static string GetLocalizedStringValue(object pObject, string pPropertyName, CultureInfo pCulture = null)
        {
            var prop = ReflectionHelper.GetIMPropertyInfo(pObject.GetType(), GetLocPropertyName(pPropertyName), typeof(JsonDocument));

            var locValuesJson = (JsonDocument)prop.GetValue(pObject) ?? JsonDocument.Parse("{}");

            var culture = (pCulture ?? CultureInfo.CurrentUICulture).TwoLetterISOLanguageName;

            if (locValuesJson.RootElement.TryGetProperty(culture, out var element))
            {
                return element.GetString();
            }

            return string.Empty;
        }
    }
}
