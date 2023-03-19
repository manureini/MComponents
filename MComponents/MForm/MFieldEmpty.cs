using System;
using System.Collections.Generic;

namespace MComponents.MForm
{
    internal class MFieldEmpty : IMField
    {
        public Attribute[] Attributes { get; set; }
        public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; } = new Dictionary<string, object>();
        public MFieldRow FieldRow { get; set; }

        public void Dispose()
        {
        }
    }
}
