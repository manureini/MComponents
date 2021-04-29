using System;

namespace MComponents.Shared.Attributes
{
    public class RestrictValuesAttribute : Attribute
    {
        public object[] AllowedValues { get; protected set; }

        public RestrictValuesAttribute(params object[] pAllowedValues)
        {
            AllowedValues = pAllowedValues;
        }
    }
}
