using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MComponents.MQueryBuilder
{
    public class MQueryBuilderRuleGroup
    {
        public MQueryBuilderRuleGroupOperator Operator { get; set; }

        public List<MQueryBuilderJsonCondition> Conditions { get; set; } = new();

        public List<MQueryBuilderRuleGroup> ChildGroups { get; set; }
    }
}
