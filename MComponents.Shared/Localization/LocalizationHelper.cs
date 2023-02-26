using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace MComponents.Shared.Localization
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
                        var locProp = pObject.GetType().GetProperty(GetLocPropertyName(pPropertyInfo.Name));
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
            var prop = pObject.GetType().GetProperty(GetLocPropertyName(pPropertyName));

            var locValuesJson = (string)prop.GetValue(pObject) ?? "[]";

            var locValues = JsonSerializer.Deserialize<List<LocalizedJsonString>>(locValuesJson);

            var culture = (pCulture ?? CultureInfo.CurrentUICulture).TwoLetterISOLanguageName;

            locValues.RemoveAll(l => l.Culture == culture);
            locValues.Add(new LocalizedJsonString(culture, pValue));

            var json = JsonSerializer.Serialize(locValues);

            prop.SetValue(pObject, json);
        }

        public static string GetLocalizedStringValue(object pObject, string pPropertyName, CultureInfo pCulture = null)
        {
            var prop = pObject.GetType().GetProperty(GetLocPropertyName(pPropertyName));

            var locValuesJson = (string)prop.GetValue(pObject) ?? "[]";

            var culture = (pCulture ?? CultureInfo.CurrentUICulture).TwoLetterISOLanguageName;

            var locValues = JsonSerializer.Deserialize<List<LocalizedJsonString>>(locValuesJson);

            var value = locValues.FirstOrDefault(l => l.Culture == culture);

            return value?.Value ?? string.Empty;
        }

        public static void SetLocalizedStringValues(object pObject, string pPropertyName, List<LocalizedJsonString> values)
        {
            var prop = pObject.GetType().GetProperty(GetLocPropertyName(pPropertyName));
            var json = JsonSerializer.Serialize(values);
            prop.SetValue(pObject, json);
        }

        public static List<LocalizedJsonString> GetLocalizedStringValues(object pObject, string pPropertyName)
        {
            var prop = pObject.GetType().GetProperty(GetLocPropertyName(pPropertyName));
            var locValuesJson = (string)prop.GetValue(pObject) ?? "[]";
            return JsonSerializer.Deserialize<List<LocalizedJsonString>>(locValuesJson);
        }
    }
}
