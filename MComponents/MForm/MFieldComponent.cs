using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MComponents.MForm
{
    public class MFieldComponent : IMField
    {
        public virtual Attribute[] Attributes { get; set; }

        public virtual IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

        public Type CompnentType { get; set; }

        [CascadingParameter]
        public MFieldRow FieldRow { get; set; }
    }
}
