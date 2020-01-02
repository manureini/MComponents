namespace MComponents
{
    public class MFormValueChangedArgs
    {
        public string Property { get; protected set; }

        public object NewValue { get; protected set; }

        public object Model { get; protected set; }

        internal MFormValueChangedArgs(string pProperty, object pNewValue, object pModel)
        {
            Property = pProperty;
            NewValue = pNewValue;
            Model = pModel;
        }
    }
}
