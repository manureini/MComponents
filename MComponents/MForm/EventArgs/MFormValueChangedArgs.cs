using MComponents.MForm;

namespace MComponents
{
    public class MFormValueChangedArgs<T>
    {
        public string Property { get; protected set; }

        public object OldValue { get; protected set; }
        public object NewValue { get; protected set; }

        public T Model { get; protected set; }

        public IMField Field { get; protected set; }

        public IMPropertyInfo PropertyInfo { get; protected set; }

        internal MFormValueChangedArgs(IMField pField, IMPropertyInfo pPropertyInfo, object pOldValue, object pNewValue, T pModel)
        {
            Field = pField;
            PropertyInfo = pPropertyInfo;
            OldValue = pOldValue;
            NewValue = pNewValue;
            Model = pModel;
            Property = pPropertyInfo.Name;
        }
    }
}
