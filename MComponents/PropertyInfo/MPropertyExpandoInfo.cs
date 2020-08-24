using System;
using System.Collections.Generic;
using System.Linq;

namespace MComponents
{
    public class MPropertyExpandoInfo : IMPropertyInfo
    {
        public string Name { get; protected set; }

        public Type PropertyType { get; set; }

        public Attribute[] Attributes { get; set; }

        public IMPropertyInfo Parent { get; set; }

        public bool IsReadOnly => false;

        public MPropertyExpandoInfo(string pName, Type pType, IMPropertyInfo pParent)
        {
            Name = pName;
            PropertyType = pType;
            Parent = pParent;
        }

        public Attribute GetCustomAttribute(Type pType)
        {
            return Attributes?.FirstOrDefault(a => a.GetType() == pType);
        }

        public IEnumerable<Attribute> GetAttributes()
        {
            return Attributes;
        }

        public object GetValue(object pModel)
        {
            if (pModel == null)
                return null;

            pModel = GetPropertyHolder(pModel);

            if (pModel == null)
                return null;

            var dict = pModel as IDictionary<string, object>;

            if (!dict.ContainsKey(Name))
                return null;

            object value = dict[Name];

            if (value is Delegate)
            {
                return ((dynamic)value)();
            }

            return value;
        }

        public void SetValue(object pModel, object value)
        {
            if (pModel == null)
                throw new InvalidOperationException("Can not set null value");

            pModel = GetPropertyHolder(pModel);

            if (pModel == null)
                throw new InvalidOperationException("Can not set null value");

            var dict = pModel as IDictionary<string, object>;
            dict[Name] = value;
        }

        public void SetAttributes(Attribute[] pAttributes)
        {
            Attributes = pAttributes;
        }

        public object GetPropertyHolder(object pModel)
        {
            if (Parent != null)
            {
                return Parent.GetValue(pModel);
            }

            return pModel;
        }

    }
}
