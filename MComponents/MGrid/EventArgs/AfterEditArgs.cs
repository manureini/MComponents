namespace MComponents.MGrid
{
    public class AfterEditArgs<T> : RowEventArgs<T>
    {
        public MFormSubmitArgs FormSubmitArgs { get; set; }
    }
}
