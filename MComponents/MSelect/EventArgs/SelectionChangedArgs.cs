namespace MComponents.MSelect
{
    public class SelectionChangedArgs<T>
    {
        public T NewValue { get; set; }

        public T OldValue { get; set; }
    }
}
