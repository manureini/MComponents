using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MComponents.MForm
{
    public sealed class MFieldComponent : IMField
    {
        public Attribute[] Attributes { get; set; }

        public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

        public Type CompnentType { get; set; }

        [CascadingParameter]
        public MFieldRow FieldRow { get; set; }

        public void Dispose()
        {
        }
    }
}
