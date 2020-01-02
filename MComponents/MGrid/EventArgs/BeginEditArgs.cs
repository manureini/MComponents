namespace MComponents.MGrid
{
    public class BeginEditArgs<T> : RowEventArgs<T>, ICancelableEvent
    {
        public bool Cancelled { get; set; }
    }
}
