namespace MComponents.MGrid
{
    public class BeginDeleteArgs<T> : RowEventArgs<T>, ICancelableEvent
    {
        public bool Cancelled { get; set; }
        public bool UseDeleteConfirmationWithAlert { get; set; }
    }
}
