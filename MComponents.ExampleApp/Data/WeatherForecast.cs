using MComponents.Shared.Attributes;
using System;

namespace MComponents.ExampleApp.Data
{
    public class WeatherForecast
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime Date { get; set; }

        [Row(4)]
        public int TemperatureC { get; set; }

        [Row(4)]
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        public string Summary { get; set; }

        public WeatherForecastType ForcastType { get; set; }

        [ReadOnly]
        public bool IsGoodWeather { get; set; }

        public Type TypeOfClouds { get; set; }

        public WeatherStation WeatherStation { get; set; }
    }
}
