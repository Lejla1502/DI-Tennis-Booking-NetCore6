using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace TennisBookings.Pages
{
    public class IndexModel : PageModel
    {
		private readonly IWeatherForecaster _weatherForecaster;
		private readonly ILogger _logger;
		private readonly FeaturesConfiguration _config;

		public IndexModel(IWeatherForecaster weatherForecaster, ILogger<IndexModel> logger, IOptionsSnapshot<FeaturesConfiguration> config)
		{
			_weatherForecaster = weatherForecaster;
			_logger = logger;
			_config = config.Value;
		}
		public string WeatherDescription { get; private set; } =
            "We don't have the latest weather information right now, " +
			"please check again later.";

		//set when the OnGet method is called
        public bool ShowWeatherForecast { get; private set; }
        public bool ShowGreeting => false;
        public string Greeting => "Welcome to Tennis by the Sea";

        public async Task OnGet()
        {
			//var forecaster = new RandomWeatherForecaster();

			ShowWeatherForecast = _config.EnableWeatherForcast;

			//we're calling ShowWeatherForcast only if its been enabled by configuration
			if(ShowWeatherForecast)
			{
				try
				{
					var currentWeather = await _weatherForecaster
						.GetCurrentWeatherAsync("Eastbourne");

					switch (currentWeather.Weather.Summary)
					{
						case "Sun":
							WeatherDescription = "It's sunny right now. " +
								"A great day for tennis!";
							break;

						case "Cloud":
							WeatherDescription = "It's cloudy at the moment " +
								"and the outdoor courts are in use.";
							break;

						case "Rain":
							WeatherDescription = "We're sorry but it's raining here. " +
								"No outdoor courts in use.";
							break;

						case "Snow":
							WeatherDescription = "It's snowing!! Outdoor courts will " +
								"remain closed until the snow has cleared.";
							break;
					}
				}
				catch
				{
					// TODO
					_logger.LogError("Oh noo!!!");
				}
			}

            
        }
    }
}
