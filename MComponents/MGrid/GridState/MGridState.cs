using System;
using System.Collections.Generic;
using System.Text;

namespace MComponents.MGrid
{
    public class MGridState
    {
        public long? Page { get; set; }

        public int? PageSize { get; set; }

        public bool IsFilterRowVisible { get; set; }

        public Guid? SelectedRow { get; set; }

        public MGridFilterState[] FilterState { get; set; }

        public MGridSorterState[] SorterState { get; set; }
    }
}
