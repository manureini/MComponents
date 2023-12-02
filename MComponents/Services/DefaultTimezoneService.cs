using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MComponents.Services
{
    public class DefaultTimezoneService : ITimezoneService
    {
        public DateTime ToLocalTime(DateTime pUtcDateTime)
        {
            return pUtcDateTime.ToLocalTime();
        }

        public DateTime ToUtcTime(DateTime pLocalDateTime)
        {
            return pLocalDateTime.ToUniversalTime();
        }
    }
}
