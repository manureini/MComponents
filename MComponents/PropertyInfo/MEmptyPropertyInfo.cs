using System;
using System.Collections.Generic;
using System.Linq;

namespace MComponents
{
    internal class MEmptyPropertyInfo : IMPropertyInfo
    {
        public string Name { get; set; }

        public Type PropertyType { get; set; }

        public IMPropertyInfo Parent => null;

        public bool IsReadOnly => true;

        public MEmptyPropertyInfo()
        {
        }

        public MEmptyPropertyInfo(string pPropertyName, Type pPropertyType)
        {
            Name = pPropertyName;
            PropertyType = pPropertyType;
        }

        public T GetCustomAttribute<T>() where T : Attribute
        {
            return null;
        }

        public object GetPropertyHolder(object pModel)
        {
            return pModel;
        }

        public object GetValue(object pModel)
        {
            return null;
        }

        public void SetAttributes(Attribute[] pAttributes)
        {
        }

        public void SetValue(object pModel, object value)
        {
        }

        public IEnumerable<Attribute> GetAttributes()
        {
            return Enumerable.Empty<Attribute>();
        }
    }
}
