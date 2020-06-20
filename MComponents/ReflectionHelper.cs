using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace MComponents
{
    internal static class ReflectionHelper
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
                return new MPropertyExpandoInfo(pProperty, pPropertyType);
            }

            if (pProperty.Contains('.'))
            {
                var properties = pProperty.Split('.');

                if (properties.Length < 2)
                    throw new ArgumentException($"{nameof(pProperty)} not valid: " + pProperty);

                var parentProperty = pObjectType.GetProperty(properties.First());

                string childProperties = string.Join(".", properties.Skip(1));

                var parent = new MPropertyInfo(parentProperty, pParent);

                return GetIMPropertyInfo(parentProperty.PropertyType, childProperties, null, parent); //TODO pPropertyType
            }

            var pi = pObjectType.GetProperty(pProperty);

            if (pi == null && pObjectType.IsInterface)
            {
                pi = pObjectType.GetInterfaces().SelectMany(i => i.GetProperties()).SingleOrDefault(p => p.Name == pProperty);
            }

            if (pi == null)
                throw new InvalidOperationException(pObjectType + " does not have property " + pProperty);

            return new MPropertyInfo(pi, pParent);
        }


        public static IEnumerable<IMPropertyInfo> GetProperties(object pValue)
        {
            if (pValue is IDictionary<string, object> dict)
            {
                return dict.Select(v => GetIMPropertyInfo(pValue.GetType(), v.Key, v.Value.GetType()));
            }

            return pValue.GetType().GetProperties().Select(v => GetIMPropertyInfo(pValue.GetType(), v.Name, v.PropertyType));
        }


    }
}
