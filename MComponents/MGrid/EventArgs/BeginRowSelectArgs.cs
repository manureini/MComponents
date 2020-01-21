namespace MComponents.MGrid
{
    public class BeginRowSelectArgs<T> : RowEventArgs<T>, ICancelableEvent
    {
        public bool Cancelled { get; set; }
    }
}
