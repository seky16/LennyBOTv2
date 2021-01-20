using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using LennyBOTv2.Models.Weatherstack;
using Microsoft.Extensions.Configuration;

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

        internal async Task<IEnumerable<object>> GetWeatherForLocationAsync(string location)
        {
            var result = new List<object?>
            {
                await WeatherstackAsync(location).ConfigureAwait(false)
            };
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            return result.Where(obj => !(obj is null));
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
        }

        internal bool TryGetDefaultLocation(ulong id, out string location)
        {
#pragma warning disable CS8601 // Possible null reference assignment.
            return _weatherDefaults.TryGetValue(id, out location);
#pragma warning restore CS8601 // Possible null reference assignment.
        }

        private async Task<object?> WeatherstackAsync(string location)
        {
            var apiKey = _config["weatherstackAPIkey"];

            var model = await Helpers.GetFromJsonAsync<WeatherStackModel>($"http://api.weatherstack.com/current?access_key={apiKey}&query={location}").ConfigureAwait(false);

            if (model?.Current is null || model.Location is null || !model.Success)
            {
                await LoggingService.LogErrorAsync(model?.Error?.ToString(), nameof(WeatherService)).ConfigureAwait(false);
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
                $"UV Index: {model.Current.UVIndex}\n" +
                $"Visibility: {model.Current.Visibility} km");
        }
    }
}
