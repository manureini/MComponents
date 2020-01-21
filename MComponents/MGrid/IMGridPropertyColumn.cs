using System;

namespace MComponents.MGrid
{
    public interface IMGridPropertyColumn
    {
        string Property { get; set; }

        Type PropertyType { get; set; }

        Attribute[] Attributes { get; set; }

        string StringFormat { get; set; }
    }
}
