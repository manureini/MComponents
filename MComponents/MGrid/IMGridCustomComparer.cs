using System.Collections.Generic;

namespace MComponents.MGrid
{
    public interface IMGridCustomComparer
    {

    }

    public interface IMGridCustomComparer<T> : IMGridCustomComparer
    {
        IComparer<T> Comparer { get; }
    }
}
