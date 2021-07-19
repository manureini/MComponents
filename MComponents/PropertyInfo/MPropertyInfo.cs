using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MComponents
{
    public class MPropertyInfo : IMPropertyInfo
    {
        protected PropertyInfo mPropertyInfo;
        protected Attribute[] mAttributes;

        public IMPropertyInfo Parent { get; set; }

        public MPropertyInfo(PropertyInfo pPropertyInfo, IMPropertyInfo pParent)
        {
            mPropertyInfo = pPropertyInfo;
            Parent = pParent;
        }

        public string Name => mPropertyInfo.Name;

        public Type PropertyType
        {
            get
            {
                return mPropertyInfo.PropertyType;
            }
            set
            {
                throw new InvalidOperationException("Can not change Property of existing type!");
            }
        }

        public bool IsReadOnly => !mPropertyInfo.CanWrite;

        public void SetAttributes(Attribute[] pAttributes)
        {
            mAttributes = pAttributes;
        }

        public T GetCustomAttribute<T>() where T : Attribute
        {
            if (mAttributes != null)
            {
                return (T)mAttributes.FirstOrDefault(a => a.GetType() == typeof(T));
            }

            return (T)mPropertyInfo.GetCustomAttribute(typeof(T));
        }

        public IEnumerable<Attribute> GetAttributes()
        {
            return mAttributes ?? mPropertyInfo.GetCustomAttributes();
        }

        public object GetPropertyHolder(object pModel)
        {
            if (Parent != null)
            {
                return Parent.GetValue(pModel);
            }

            return pModel;
        }

        public object GetValue(object pModel)
        {
            if (pModel == null)
                return null;

            pModel = GetPropertyHolder(pModel);

            if (pModel == null)
                return null;

            return mPropertyInfo.GetValue(pModel);
        }

        public void SetValue(object pModel, object value)
        {
            if (pModel == null)
                throw new InvalidOperationException("Can not set null value");

            pModel = GetPropertyHolder(pModel);

            if (pModel == null)
                throw new InvalidOperationException("Can not set null value");

            if (!mPropertyInfo.CanWrite)
                throw new InvalidOperationException("Can not write");

            mPropertyInfo.SetValue(pModel, value);
        }

    }
}
