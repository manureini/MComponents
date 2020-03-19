namespace MComponents.MGrid
{
    public interface IMGridSortableColumn
    {
        MSortDirection SortDirection { get; }

        int SortIndex { get; }

        bool EnableSorting { get; }
    }
}
