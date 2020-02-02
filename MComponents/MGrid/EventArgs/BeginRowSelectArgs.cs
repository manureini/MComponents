using Microsoft.AspNetCore.Components.Web;

namespace MComponents.MGrid
{
    public class BeginRowSelectArgs<T> : RowEventArgs<T>, ICancelableEvent
    {
        public bool Cancelled { get; set; }
    }
}
