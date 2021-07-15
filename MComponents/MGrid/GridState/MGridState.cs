using System;

namespace MComponents.MGrid
{
    public class MGridState
    {
        public long? Page { get; set; }

        public int? PageSize { get; set; }

        public bool IsFilterRowVisible { get; set; }

        public string SelectedRow { get; set; }

        public MGridFilterState[] FilterState { get; set; }

        public MGridSorterState[] SorterState { get; set; }
    }
}
