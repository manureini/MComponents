using MComponents.MGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MComponents.ExampleApp.Data
{
    public class GridDataAdapter : IMGridDataAdapter<WeatherForecast>
    {
        protected WeatherForecastService mWeatherForecastService;

        public GridDataAdapter(WeatherForecastService pWeatherForecastService)
        {
            mWeatherForecastService = pWeatherForecastService;
        }

        public Task Add(WeatherForecast pNewValue)
        {
            return Task.CompletedTask;
        }

        public Task<IEnumerable<WeatherForecast>> GetData(IQueryable<WeatherForecast> pQueryable)
        {
            return GetFilteredData(pQueryable);
        }

        public async Task<long> GetDataCount(IQueryable<WeatherForecast> pQueryable)
        {
            return (await mWeatherForecastService.GetForecastAsync(DateTime.Now)).LongCount();
        }

        public async Task<long> GetTotalDataCount()
        {
            return (await mWeatherForecastService.GetForecastAsync(DateTime.Now)).LongCount();
        }

        public async Task<IEnumerable<WeatherForecast>> GetFilteredData(IQueryable<WeatherForecast> pQueryable)
        {
            var data = await mWeatherForecastService.GetForecastAsync(DateTime.Now);

            QueryExpressionVisitor<WeatherForecast> visitor = new QueryExpressionVisitor<WeatherForecast>(data.AsQueryable());
            var newExpressionTree = visitor.Visit(pQueryable.Expression);

            var lambda = Expression.Lambda(newExpressionTree);
            var compiled = lambda.Compile();

            return (IEnumerable<WeatherForecast>)compiled.DynamicInvoke();
        }

        public Task Remove(WeatherForecast pValue)
        {
            return Task.CompletedTask;
        }

        public Task Update(WeatherForecast pValue)
        {
            return Task.CompletedTask;
        }
    }
}
