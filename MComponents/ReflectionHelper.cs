﻿using MComponents.MGrid;
using MComponents.Services;
using MComponents.Shared.Attributes;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace MComponents
{
    public static class ReflectionHelper
    {
        public static object GetPropertyValue(object pObject, string pProperty)
        {
            if (pObject == null)
                return null;

            if (pObject is IDictionary<string, object> dict)
            {
                object value = dict[pProperty];

                if (value is Delegate)
                {
                    return ((dynamic)value)();
                }

                return value;
            }

            var pi = pObject.GetType().GetProperty(pProperty);

            if (pi == null)
                throw new InvalidOperationException(pObject.GetType() + " does not have property " + pProperty);

            return pi.GetValue(pObject);
        }

        public static IMPropertyInfo GetIMPropertyInfo(Type pObjectType, string pProperty, Type pPropertyType, IMPropertyInfo pParent = null)
        {
            if (typeof(IDictionary<string, object>).IsAssignableFrom(pObjectType))
            {
                return GetMPropertyExpandoInfo(pObjectType, pProperty, pPropertyType, pParent);
            }

            return GetMPropertyInfo(pObjectType, pProperty, pParent);
        }

        private static IMPropertyInfo GetMPropertyInfo(Type pObjectType, string pProperty, IMPropertyInfo pParent)
        {
            if (pProperty.Contains('.'))
            {
                var properties = pProperty.Split('.');

                if (properties.Length < 2)
                    throw new ArgumentException($"{nameof(pProperty)} not valid: " + pProperty);

                var parentProperty = GetPropertyInfo(pObjectType, properties.First());

                string childProperties = string.Join(".", properties.Skip(1));

                var parent = new MPropertyInfo(parentProperty, pParent);

                var childPropType = parentProperty.PropertyType.GetProperty(properties.Skip(1).First());

                return GetIMPropertyInfo(parentProperty.PropertyType, childProperties, childPropType.PropertyType, parent);
            }

            PropertyInfo pi = GetPropertyInfo(pObjectType, pProperty);

            if (pi == null)
                throw new InvalidOperationException(pObjectType + " does not have property " + pProperty);

            return new MPropertyInfo(pi, pParent);
        }

        private static PropertyInfo GetPropertyInfo(Type pObjectType, string pProperty)
        {
            var pi = pObjectType.GetProperty(pProperty);

            if (pi == null && pObjectType.IsInterface)
            {
                pi = pObjectType.GetInterfaces().SelectMany(i => i.GetProperties()).SingleOrDefault(p => p.Name == pProperty);
            }

            return pi;
        }

        private static IMPropertyInfo GetMPropertyExpandoInfo(Type pObjectType, string pProperty, Type pPropertyType, IMPropertyInfo pParent)
        {
            if (pProperty.Contains('.'))
            {
                var properties = pProperty.Split('.');

                if (properties.Length < 2)
                    throw new ArgumentException($"{nameof(pProperty)} not valid: " + pProperty);

                var parent = new MPropertyExpandoInfo(properties.First(), null, pParent);

                string childProperties = string.Join(".", properties.Skip(1));

                return GetIMPropertyInfo(pObjectType, childProperties, pPropertyType, parent);
            }

            return new MPropertyExpandoInfo(pProperty, pPropertyType, pParent);
        }

        public static IEnumerable<IMPropertyInfo> GetProperties(object pValue)
        {
            if (pValue is IDictionary<string, object> dict)
            {
                return dict.Select(v => GetIMPropertyInfo(pValue.GetType(), v.Key, v.Value?.GetType() ?? typeof(object)));
            }

            return pValue.GetType().GetProperties().Select(v => GetIMPropertyInfo(pValue.GetType(), v.Name, v.PropertyType));
        }

        public static T ChangeType<T>(object pObjecte)
        {
            return (T)ChangeType(pObjecte, typeof(T));
        }

        public static object ChangeType(object pObject, Type pType)
        {
            if (pObject == null)
                return null;

            if (pType.IsAssignableFrom(pObject.GetType()))
                return pObject;

            if (pType.IsEnum)
            {
                if (Enum.TryParse(pType, pObject.ToString(), true, out object enumResult))
                    return enumResult;

                return null;
            }

            if (pType == typeof(Guid))
            {
                if (Guid.TryParse(pObject.ToString(), out Guid result))
                    return result;

                return null;
            }

            if (pType == typeof(int))
            {
                if (int.TryParse(pObject.ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out int result))
                    return result;
                return null;
            }

            if (pType == typeof(long))
            {
                if (long.TryParse(pObject.ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out long result))
                    return result;
                return null;
            }

            if (pType == typeof(float))
            {
                if (float.TryParse(pObject.ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out float result))
                    return result;
                return null;
            }

            if (pType == typeof(double))
            {
                if (double.TryParse(pObject.ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out double result))
                    return result;
                return null;
            }

            if (pType == typeof(decimal))
            {
                if (decimal.TryParse(pObject.ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out decimal result))
                    return result;
                return null;
            }

            if (pObject is JsonElement elem)
            {
                return elem.ToObject(pType);
            }

            var innerType = Nullable.GetUnderlyingType(pType);

            if (innerType != null)
            {        
                var innerValue = ChangeType(pObject, innerType);
                return innerValue;
            }

            return Convert.ChangeType(pObject, pType);
        }
         

        public static string GetDisplayName<T>(string pProperty, IStringLocalizer L)
        {
            return GetMPropertyInfo(typeof(T), pProperty, null).GetDisplayName(L);
        }

        public static DateTime? AdjustTimezoneIfUtcInternal(ITimezoneService pTimezoneService, DateTime? pDateTime, IMPropertyInfo pPropertyInfo)
        {
            if (pDateTime != null && pPropertyInfo.GetCustomAttribute<UtcInternalDisplayUserTimezone>() != null)
            {
                if (pDateTime.Value.Kind != DateTimeKind.Utc)
                {
                    throw new InvalidOperationException($"{nameof(UtcInternalDisplayUserTimezone)} DateTimeKind {pDateTime.Value.Kind} != UTC");
                }

                pDateTime = pTimezoneService.ToLocalTime(pDateTime.Value);
            }

            return pDateTime;
        }
    }
}
