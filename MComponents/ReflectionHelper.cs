using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

        public static object ChangeType(object pObject, Type pType)
        {
            if (pObject == null)
                return null;

            if (pType.IsAssignableFrom(pObject.GetType()))
                return pObject;

            if (pType.IsEnum)
            {
                Enum.TryParse(pType, pObject.ToString(), true, out object enumResult);
                return enumResult;
            }

            if (pType == typeof(Guid))
                return Guid.Parse(pObject.ToString());

            if (IsNullable(pType))
            {
                var innerType = Nullable.GetUnderlyingType(pType);
                var innerValue = ChangeType(pObject, innerType);
                return innerValue;
            }

            return Convert.ChangeType(pObject, pType);
        }

        public static bool IsNullable(Type pType)
        {
            if (!pType.IsValueType)
                return true;

            if (Nullable.GetUnderlyingType(pType) != null)
                return true;

            return false;
        }
    }
}
