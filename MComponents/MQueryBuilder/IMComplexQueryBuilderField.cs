using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MComponents.MQueryBuilder
{
    public interface IMComplexQueryBuilderField
    {
        public int ParameterCount { get; }
        public bool HasFormTemplate { get; }
    }
}
