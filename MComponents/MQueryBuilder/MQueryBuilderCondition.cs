using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MComponents.MQueryBuilder
{
    public class MQueryBuilderJsonCondition
    {         
        public string Property { get; set; }

        public MQueryBuilderConditionOperator? Operator { get; set; }

        public object Value { get; set; }
    }
}
