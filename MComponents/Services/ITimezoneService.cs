using System;

namespace MComponents.Services
{
    public interface ITimezoneService
    {
        public DateTime ToLocalTime(DateTime pUtcDateTime);
        public DateTime ToUtcTime(DateTime pLocalDateTime);
    }
}
