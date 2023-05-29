using TennisBookings.Services.Time;

namespace TennisBookings.Services
{
	public class CachedWeatherForecaster : IWeatherForecaster
	{
		private readonly IWeatherForecaster _forecaster;
		private readonly IUtcTimeService _timeService;
		private readonly IDistributedCache<WeatherResult> _cache;

		public CachedWeatherForecaster(IWeatherForecaster forecaster,
			IUtcTimeService timeService,
			IDistributedCache<WeatherResult> cache)
		{
			_forecaster = forecaster;
			_timeService = timeService;
			_cache = cache;
		}	
		public async Task<WeatherResult> GetCurrentWeatherAsync(string city)
		{
			var cacheKey = $"weather_{city}_{_timeService.CurrentUtcDateTime:yyyy_MM-dd}";

			var (isCached, forecast) = await _cache.TryGetValueAsync(cacheKey);

			if (isCached)
				return forecast!;

			var result = await _forecaster.GetCurrentWeatherAsync(city);

			await _cache.SetAsync(cacheKey, result, 60);

			return result;
		}
	}
}
