namespace MComponents.MGrid
{
    public interface IMGridSortableColumn
    {
        MSortDirection SortDirection { get; }

        int SortIndex { get; }

        bool EnableSort { get; }
    }
}
