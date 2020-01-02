using System;
using System.Collections.Generic;

namespace MComponents.MForm
{
    public interface IMField
    {
        Attribute[] Attributes { get; set; }

        IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }
    }
}
