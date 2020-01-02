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

        public IMPropertyInfo Parent => null;

        public bool IsReadOnly => false;

        public MPropertyExpandoInfo(string pName, Type pType)
        {
            Name = pName;
            PropertyType = pType;
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
            var dict = pModel as IDictionary<string, object>;
            dict[Name] = value;
        }

        public void SetAttributes(Attribute[] pAttributes)
        {
            Attributes = pAttributes;
        }

        public object GetPropertyHolder(object pModel)
        {
            throw new NotImplementedException();
        }


    }
}
