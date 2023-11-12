using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MComponents.MQueryBuilder
{
    public interface IMQueryBuilder
    {
        Type ModelType { get; }

        List<IMQueryBuilderField> Fields { get; }

        void RegisterField(IMQueryBuilderField pField);

        void RemoveCondition(MQueryBuilderJsonCondition pCondition);

        void RemoveRuleGroup(MQueryBuilderRuleGroup pRuleGroup);

        Task InvokeRulesChanged();
    }
}
