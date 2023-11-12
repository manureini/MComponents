using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MComponents.MQueryBuilder
{
    public interface IMQueryBuilderField
    {
        string Property { get; }
        Type PropertyType { get; }
    }
}
