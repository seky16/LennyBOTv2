using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using LennyBOTv2.Models.Weatherstack;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace LennyBOTv2.Services
{
    public class WeatherService
    {
        private readonly IConfiguration _config;

        private readonly Dictionary<ulong, string> _weatherDefaults = new Dictionary<ulong, string>()
        {
            { 237996378293272576, "Rotterdam" }, // Sean
            { 161053293513146371, "Villefontaine" }, // Gugu
            { 239502997074345984, "Olomouc" }, // Lonxie
            { 246370598421397506, "Olomouc" }, // seky16
            { 263375481158500364, "Olomouc" }, // Dwarf
            { 246997041480204288, "Brno" }, // Brain Damage
            { 247826910934073355, "Brno" }, // Peter
            { 247825199817424896, "Brno" }, // Daniel
        };

        public WeatherService(IConfiguration config)
        {
            _config = config;
        }

        internal async Task<List<object?>> GetWeatherForLocationAsync(string location)
        {
            var result = new List<object?>
            {
                await WeatherstackAsync(location).ConfigureAwait(false)
            };
            return result;
        }

        internal bool TryGetDefaultLocation(ulong id, out string location)
        {
#pragma warning disable CS8601 // Possible null reference assignment.
            return _weatherDefaults.TryGetValue(id, out location);
#pragma warning restore CS8601 // Possible null reference assignment.
        }

        private async Task<object?> WeatherstackAsync(string location)
        {
            var jsonString = string.Empty;
            using (var client = new HttpClient())
            {
                var result = await client.GetAsync(string.Format("http://api.weatherstack.com/current?access_key={1}&query={0}", location.Replace(' ', '+'), _config["weatherstackAPIkey"])).ConfigureAwait(false);
                if (!result.IsSuccessStatusCode)
                {
                    await LoggingService.LogErrorAsync($"Weatherstack API returned {result.StatusCode}", nameof(WeatherService)).ConfigureAwait(false);
                    return null;
                }

                jsonString = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            WeatherStackModel? model = null;
            try
            {
                model = JsonConvert.DeserializeObject<WeatherStackModel>(jsonString);
            }
            catch (Exception ex)
            {
                await LoggingService.LogExceptionAsync(ex, nameof(WeatherService)).ConfigureAwait(false);
                return null;
            }

            if (model?.Current is null || model.Location is null)
            {
                await LoggingService.LogErrorAsync($"Can't deserialize '{jsonString}'", nameof(WeatherService)).ConfigureAwait(false);
                return null;
            }

            var updatedOnUtc = DateTimeOffset.UtcNow;

            if (model.Location.LocaltimeEpoch.HasValue && model.Location.UtcOffset.HasValue)
            {
                updatedOnUtc = DateTimeOffset.FromUnixTimeSeconds(model.Location.LocaltimeEpoch.Value).AddHours(-model.Location.UtcOffset.Value);
            }

            return new EmbedBuilder()
                .WithTitle($"Weather in {model.Location.Name}, {model.Location.Region}, {model.Location.Country}")
                .WithDescription($"{model.Current.Temperature} °C {model.Current.WeatherDescriptions.FirstOrDefault()}")
                .WithThumbnailUrl(model.Current.WeatherIcons.FirstOrDefault())
                .WithFooter($"Last update: {updatedOnUtc.ToPragueTimeString()}")
                .WithAuthor(author =>
                {
                    author
                        .WithName("weatherstack")
                        .WithUrl("https://weatherstack.com")
                        .WithIconUrl("https://weatherstack.com/site_images/weatherstack_icon.png");
                })
                .AddField("Details",
                $"Feels like: {model.Current.Feelslike} °C\n" +
                $"Cloud coverage: {model.Current.Cloudcover} %\n" +
                $"Precipitation: {model.Current.Precip} mm\n" +
                $"Humidity: {model.Current.Humidity} %\n" +
                $"Pressure: {model.Current.Pressure} mBar\n" +
                $"Wind: {model.Current.WindSpeed} km/h {model.Current.WindDir}\n" +
                $"UV Index: {model.Current.UVIndex}\n"+
                $"Visibility: {model.Current.Visibility} km");
        }
    }
}
