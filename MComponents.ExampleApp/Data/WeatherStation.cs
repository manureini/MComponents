using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace MComponents.ExampleApp.Data
{
    public class WeatherStation : IComparable<WeatherStation>
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; }

        public int CompareTo([AllowNull] WeatherStation other)
        {
            return Id.CompareTo(other.Id);
        }

        public override bool Equals(object obj)
        {
            return obj is WeatherStation station &&
                   Id.Equals(station.Id);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }
    }
}
