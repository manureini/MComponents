using System;
using System.Collections.Generic;

namespace MComponents
{
    public interface IMPropertyInfo
    {
        string Name { get; }

        Type PropertyType { get; set; }

        void SetAttributes(Attribute[] pAttributes);

        Attribute GetCustomAttribute(Type pType);

        IEnumerable<Attribute> GetAttributes();

        object GetValue(object pModel);

        void SetValue(object pModel, object value);

        IMPropertyInfo Parent { get; }

        object GetPropertyHolder(object pModel);

        bool IsReadOnly { get; }
    }
}