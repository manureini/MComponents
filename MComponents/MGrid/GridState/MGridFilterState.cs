using System;

namespace MComponents.MGrid
{
    public class MGridFilterState
    {
        public string ColumnIdentifier { get; set; }

        public object Value { get; set; }

        public Guid? ReferencedId { get; set; }
    }
}
