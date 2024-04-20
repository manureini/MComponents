using Microsoft.AspNetCore.Components.Web;

namespace MComponents.MGrid
{
    public abstract class RowEventArgs<T>
    {
        public T Row { get; set; }

        public MouseEventArgs MouseEventArgs { get; set; }
    }
}
