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

                    if (pChangedValues != null && !pChangedValues.ContainsKey(pPropertyInfo.Name + "Loc"))
                    {
                        var locProp = pObject.GetType().GetProperty(pPropertyInfo.Name + "Loc");
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

        public static void SetLocalizedStringValue(object pObject, string pPropertyName, string pValue)
        {
            var prop = pObject.GetType().GetProperty(pPropertyName + "Loc");

            var locValuesJson = (string)prop.GetValue(pObject) ?? "[]";

            var locValues = JsonSerializer.Deserialize<List<LocalizedJsonString>>(locValuesJson);

            var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            locValues.RemoveAll(l => l.Culture == culture);
            locValues.Add(new LocalizedJsonString(culture, pValue));

            var json = JsonSerializer.Serialize(locValues);

            prop.SetValue(pObject, json);
        }

        public static string GetLocalizedStringValue(object pObject, string pPropertyName)
        {
            var prop = pObject.GetType().GetProperty(pPropertyName + "Loc");

            var locValuesJson = (string)prop.GetValue(pObject) ?? "[]";

            var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            var locValues = JsonSerializer.Deserialize<List<LocalizedJsonString>>(locValuesJson);

            var value = locValues.FirstOrDefault(l => l.Culture == culture);

            if (value != null)
            {
                return value.Value;
            }

            return locValues.FirstOrDefault(l => l.Culture == "en")?.Value;
        }
    }
}
