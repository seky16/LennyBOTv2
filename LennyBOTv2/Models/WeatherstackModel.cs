using System.Collections.Generic;

namespace LennyBOTv2.Models
{
    // https://weatherstack.com/documentation#current_weather
#pragma warning disable IDE1006 // Naming Styles

    internal static class WeatherstackModel
    {
        public class Current
        {
            /// <summary>
            /// Returns the cloud cover level in percentage.
            /// </summary>
            public int cloudcover { get; set; }

            /// <summary>
            /// Returns the "Feels Like" temperature in the selected unit. (Default: Celsius)
            /// </summary>
            public double feelslike { get; set; }

            /// <summary>
            /// Returns the air humidity level in percentage.
            /// </summary>
            public int humidity { get; set; }

            /// <summary>
            /// Returns the UTC time for when the returned whether data was collected.
            /// </summary>
            public string observation_time { get; set; } = "";

            /// <summary>
            /// Returns the precipitation level in the selected unit. (Default: MM - millimeters)
            /// </summary>
            public double precip { get; set; }

            /// <summary>
            /// Returns the air pressure in the selected unit. (Default: MB - millibar)
            /// </summary>
            public double pressure { get; set; }

            /// <summary>
            /// Returns the temperature in the selected unit. (Default: Celsius)
            /// </summary>
            public double temperature { get; set; }

            /// <summary>
            /// Returns the UV index associated with the current weather condition.
            /// </summary>
            public int uv_index { get; set; }

            /// <summary>
            /// Returns the visibility level in the selected unit. (Default: kilometers)
            /// </summary>
            public int visibility { get; set; }

            /// <summary>
            /// Returns the universal weather condition code associated with the current weather condition.
            /// See https://weatherstack.com/site_resources/weatherstack-weather-condition-codes.zip
            /// </summary>
            public int weather_code { get; set; }

            /// <summary>
            /// Returns the wind degree.
            /// </summary>
            public int wind_degree { get; set; }

            /// <summary>
            /// Returns the wind direction.
            /// </summary>
            public string wind_dir { get; set; } = "";

            /// <summary>
            /// Returns the wind speed in the selected unit. (Default: kilometers/hour)
            /// </summary>
            public double wind_speed { get; set; }

            /// <summary>
            /// Returns one or more PNG weather icons associated with the current weather condition.
            /// </summary>
            public List<string> weather_icons { get; set; } = new List<string>();

            /// <summary>
            /// Returns one or more weather description texts associated with the current weather condition.
            /// </summary>
            public List<string> weather_descriptions { get; set; } = new List<string>();
        }

        public class Location
        {
            /// <summary>
            /// Returns the country name associated with the location used for this request.
            /// </summary>
            public string country { get; set; } = "";

            /// <summary>
            /// Returns the latitude coordinate associated with the location used for this request.
            /// </summary>
            public double lat { get; set; }

            /// <summary>
            /// Returns the local time of the location used for this request. (Example: 2019-09-07 08:14)
            /// </summary>
            public string localtime { get; set; } = "";

            /// <summary>
            /// Returns the local time (as UNIX timestamp) of the location used for this request. (Example: 1567844040)
            /// </summary>
            public int localtime_epoch { get; set; }

            /// <summary>
            /// Returns the longitude coordinate associated with the location used for this request.
            /// </summary>
            public double lon { get; set; }

            /// <summary>
            /// Returns the name of the location used for this request.
            /// </summary>
            public string name { get; set; } = "";

            /// <summary>
            /// Returns the region name associated with the location used for this request.
            /// </summary>
            public string region { get; set; } = "";

            /// <summary>
            /// Returns the timezone ID associated with the location used for this request. (Example: America/New_York)
            /// </summary>
            public string timezone_id { get; set; } = "";

            /// <summary>
            /// Returns the UTC offset (in hours) of the timezone associated with the location used for this request. (Example: -4.0)
            /// </summary>
            public double utc_offset { get; set; }
        }

        public class WeatherModel
        {
            public bool success { get; set; } = true;
            public Current? current { get; set; }
            public Location? location { get; set; }
        }
    }

#pragma warning restore IDE1006 // Naming Styles
}
