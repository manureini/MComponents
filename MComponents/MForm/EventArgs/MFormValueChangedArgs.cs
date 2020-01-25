namespace MComponents
{
    public class MFormValueChangedArgs<T>
    {
        public string Property { get; protected set; }

        public object NewValue { get; protected set; }

        public T Model { get; protected set; }

        internal MFormValueChangedArgs(string pProperty, object pNewValue, T pModel)
        {
            Property = pProperty;
            NewValue = pNewValue;
            Model = pModel;
        }
    }
}
