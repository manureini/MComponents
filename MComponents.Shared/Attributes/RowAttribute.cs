using System;

namespace MComponents.Shared.Attributes
{
    public class RowAttribute : Attribute
    {
        public int RowId { get; protected set; }

        public RowAttribute(int pRowId)
        {
            RowId = pRowId;
        }
    }
}
