namespace MComponents.MGrid
{
    public class BeginAddArgs<T> : RowEventArgs<T>, ICancelableEvent
    {
        public bool Cancelled { get; set; }
    }
}
