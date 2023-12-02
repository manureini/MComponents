using System;
using System.Linq;
using System.Threading.Tasks;

namespace MComponents.ExampleApp.Data
{
    public class WeatherForecastService
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public WeatherStation[] AllWeatherStations = new[] {
                new WeatherStation() {
                    Name = "Main Station"
                },
                new WeatherStation() {
                    Name = "Secondary Station"
                },
            };

        protected WeatherForecast[] mValues;

        public WeatherForecastService()
        {
            var rng = new Random();
            mValues = Enumerable.Range(1, 500).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                UtcInternalDate = DateTime.UtcNow,
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)],
                WeatherStation = AllWeatherStations[rng.Next(0, 2)],
                IsGoodWeather = rng.Next(0, 2) == 0
            }).ToArray();
        }

        public Task<WeatherForecast[]> GetForecastAsync(DateTime startDate)
        {
            return Task.FromResult(mValues);
        }
    }
}
