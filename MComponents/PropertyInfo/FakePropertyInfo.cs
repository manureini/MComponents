using System;
using System.Globalization;
using System.Reflection;

namespace MComponents
{
    internal class FakePropertyInfo<T> : PropertyInfo
    {
        protected string mFieldName;

        public FakePropertyInfo(string pFieldName)
        {
            mFieldName = pFieldName;
        }

        public override PropertyAttributes Attributes => throw new NotImplementedException();

        public override bool CanRead => true;

        public override bool CanWrite => throw new NotImplementedException();

        public override Type PropertyType => typeof(T);

        public override Type DeclaringType => throw new NotImplementedException();

        public override string Name => mFieldName;

        public override Type ReflectedType => throw new NotImplementedException();

        public override MethodInfo[] GetAccessors(bool nonPublic)
        {
            throw new NotImplementedException();
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return Array.Empty<Attribute>();
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return Array.Empty<Attribute>();
        }

        public override MethodInfo GetGetMethod(bool nonPublic)
        {
            return null;
        }

        public override ParameterInfo[] GetIndexParameters()
        {
            return Array.Empty<ParameterInfo>();
        }

        public override MethodInfo GetSetMethod(bool nonPublic)
        {
            return null;
        }

        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
