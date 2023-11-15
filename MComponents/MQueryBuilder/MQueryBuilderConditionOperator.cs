using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MComponents.MQueryBuilder
{

    public enum MQueryBuilderConditionOperator
    {
        None,

        [Display(Name = "=")]
        Equal,

        [Display(Name = "!=")]
        NotEqual,

        Contains,

        StartsWith,
        EndsWith,

        [Display(Name = ">")]
        GreaterThan,

        [Display(Name = ">=")]
        GreaterThanOrEqual,

        [Display(Name = "<")]
        LessThan,

        [Display(Name = "<=")]
        LessThanOrEqual,

        Between,
        NotBetween,

        IsEmpty,
        IsNotEmpty,
        IsNull,
        IsNotNull,

        In,
        NotIn,
    }

    public static class MQueryBuilderConditionOperatorHelper
    {
        public static MQueryBuilderConditionOperator[] GetOperatorsFromType(Type pType)
        {
            Type type = Nullable.GetUnderlyingType(pType) ?? pType;

            var isDate = pType == typeof(DateTime) || pType == typeof(DateOnly) || pType == typeof(TimeOnly);

            var operators = new List<MQueryBuilderConditionOperator>
            {
                MQueryBuilderConditionOperator.Equal,
                MQueryBuilderConditionOperator.NotEqual
            };

            if (RenderHelper.NumberTypes.Contains(type) || isDate)
            {
                operators.Add(MQueryBuilderConditionOperator.GreaterThan);
                operators.Add(MQueryBuilderConditionOperator.GreaterThanOrEqual);
                operators.Add(MQueryBuilderConditionOperator.LessThan);
                operators.Add(MQueryBuilderConditionOperator.LessThanOrEqual);
            }
            else if (pType == typeof(string))
            {
                operators.Add(MQueryBuilderConditionOperator.Contains);
                operators.Add(MQueryBuilderConditionOperator.StartsWith);
                operators.Add(MQueryBuilderConditionOperator.EndsWith);
                operators.Add(MQueryBuilderConditionOperator.IsEmpty);
                operators.Add(MQueryBuilderConditionOperator.IsNotEmpty);
            }

            if (Nullable.GetUnderlyingType(pType) != null)
            {
                operators.Add(MQueryBuilderConditionOperator.IsNull);
                operators.Add(MQueryBuilderConditionOperator.IsNotNull);
            }

            return operators.ToArray();
        }
    }

}

