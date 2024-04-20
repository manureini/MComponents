namespace MComponents.MGrid
{
    public class AfterAddArgs<T> : RowEventArgs<T>
    {
        public MFormSubmitArgs FormSubmitArgs { get; set; }
    }
}
