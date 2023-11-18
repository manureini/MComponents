using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MComponents.MQueryBuilder
{
    public interface IMQueryBuilderField
    {
        string RuleName { get; }
        string Title { get; }
        Type PropertyType { get; }
        MQueryBuilderConditionOperator[] AllowedOperators { get; }
    }
}
