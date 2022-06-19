using System;

namespace MComponents.MGrid
{
    public interface IMGridPropertyColumn : IMGridColumn
    {
        string Property { get; set; }

        Type PropertyType { get; set; }

        Attribute[] Attributes { get; set; }

        bool ExtendAttributes { get; set; }

        string StringFormat { get; set; }
    }
}
