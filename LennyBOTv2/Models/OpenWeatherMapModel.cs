using System;
using System.Xml.Serialization;

namespace LennyBOTv2.Models.OpenWeatherMap
{
    [XmlRoot(ElementName = "city")]
    public class City
    {
        [XmlElement(ElementName = "coord")]
        public Coord? Coord { get; set; }

        /// <summary>
        /// Country code (GB, JP etc.)
        /// </summary>
        [XmlElement(ElementName = "country")]
        public string? Country { get; set; }

        /// <summary>
        /// City ID
        /// </summary>
        [XmlAttribute(AttributeName = "id")]
        public int Id { get; set; }

        /// <summary>
        ///  City name
        /// </summary>
        [XmlAttribute(AttributeName = "name")]
        public string? Name { get; set; }

        [XmlElement(ElementName = "sun")]
        public Sun? Sun { get; set; }

        /// <summary>
        /// Shift in seconds from UTC
        /// </summary>
        [XmlElement(ElementName = "timezone")]
        public int Timezone { get; set; }
    }

    [XmlRoot(ElementName = "clouds")]
    public class Clouds
    {
        /// <summary>
        /// Name of the cloudiness
        /// </summary>
        [XmlAttribute(AttributeName = "name")]
        public string? Name { get; set; }

        /// <summary>
        /// Cloudiness
        /// </summary>
        [XmlAttribute(AttributeName = "value")]
        public int Value { get; set; }
    }

    [XmlRoot(ElementName = "coord")]
    public class Coord
    {
        /// <summary>
        /// City geo location, latitude
        /// </summary>
        [XmlAttribute(AttributeName = "lat")]
        public double Lat { get; set; }

        /// <summary>
        /// City geo location, longitude
        /// </summary>
        [XmlAttribute(AttributeName = "lon")]
        public double Lon { get; set; }
    }

    [XmlRoot(ElementName = "direction")]
    public class Direction
    {
        /// <summary>
        ///  Code of the wind direction. Possible value is WSW, N, S etc.
        /// </summary>
        [XmlAttribute(AttributeName = "code")]
        public string? Code { get; set; }

        /// <summary>
        /// Full name of the wind direction.
        /// </summary>
        [XmlAttribute(AttributeName = "name")]
        public string? Name { get; set; }

        /// <summary>
        /// Wind direction, degrees (meteorological)
        /// </summary>
        [XmlAttribute(AttributeName = "value")]
        public int Value { get; set; }
    }

    [XmlRoot(ElementName = "feels_like")]
    public class FeelsLike
    {
        /// <summary>
        /// Unit of measurements. Possible value is Celsius, Kelvin, Fahrenheit. Unit Default: Kelvin
        /// </summary>
        [XmlAttribute(AttributeName = "unit")]
        public string? Unit { get; set; }

        /// <summary>
        /// Temperature. This temperature parameter accounts for the human perception of weather.
        /// </summary>
        [XmlAttribute(AttributeName = "value")]
        public double Value { get; set; }
    }

    [XmlRoot(ElementName = "gusts")]
    public class Gusts
    {
        /// <summary>
        /// Wind gust. Unit Default: meter/sec, Metric: meter/sec, Imperial: miles/hour
        /// </summary>
        [XmlAttribute(AttributeName = "value")]
        public double Value { get; set; }
    }

    [XmlRoot(ElementName = "humidity")]
    public class Humidity
    {
        /// <summary>
        /// Humidity units, %
        /// </summary>
        [XmlAttribute(AttributeName = "unit")]
        public string? Unit { get; set; }

        /// <summary>
        /// Humidity value
        /// </summary>
        [XmlAttribute(AttributeName = "value")]
        public int Value { get; set; }
    }

    [XmlRoot(ElementName = "lastupdate")]
    public class Lastupdate
    {
        /// <summary>
        /// Last time when data was updated
        /// </summary>
        [XmlAttribute(AttributeName = "value")]
        public DateTime Value { get; set; }
    }

    [XmlRoot(ElementName = "current")]
    public class OpenWeatherMapModel
    {
        [XmlElement(ElementName = "city")]
        public City? City { get; set; }

        [XmlElement(ElementName = "clouds")]
        public Clouds? Clouds { get; set; }

        [XmlElement(ElementName = "feels_like")]
        public FeelsLike? FeelsLike { get; set; }

        [XmlElement(ElementName = "humidity")]
        public Humidity? Humidity { get; set; }

        [XmlElement(ElementName = "lastupdate")]
        public Lastupdate? Lastupdate { get; set; }

        [XmlElement(ElementName = "precipitation")]
        public Precipitation? Precipitation { get; set; }

        [XmlElement(ElementName = "pressure")]
        public Pressure? Pressure { get; set; }

        [XmlElement(ElementName = "temperature")]
        public Temperature? Temperature { get; set; }

        [XmlElement(ElementName = "visibility")]
        public Visibility? Visibility { get; set; }

        [XmlElement(ElementName = "weather")]
        public Weather? Weather { get; set; }

        [XmlElement(ElementName = "wind")]
        public Wind? Wind { get; set; }
    }

    [XmlRoot(ElementName = "precipitation")]
    public class Precipitation
    {
        /// <summary>
        /// Possible values are 'no", name of weather phenomena as 'rain', 'snow'
        /// </summary>
        [XmlAttribute(AttributeName = "mode")]
        public string? Mode { get; set; }

        /// <summary>
        /// Precipitation, mm
        /// </summary>
        [XmlAttribute(AttributeName = "value")]
        public int Value { get; set; }
    }

    [XmlRoot(ElementName = "pressure")]
    public class Pressure
    {
        /// <summary>
        /// Pressure units, hPa
        /// </summary>
        [XmlAttribute(AttributeName = "unit")]
        public string? Unit { get; set; }

        /// <summary>
        /// Pressure value
        /// </summary>
        [XmlAttribute(AttributeName = "value")]
        public int Value { get; set; }
    }

    [XmlRoot(ElementName = "speed")]
    public class Speed
    {
        /// <summary>
        /// Type of the wind
        /// </summary>
        [XmlAttribute(AttributeName = "name")]
        public string? Name { get; set; }

        /// <summary>
        /// Wind speed units, m/s
        /// </summary>
        [XmlAttribute(AttributeName = "unit")]
        public string? Unit { get; set; }

        /// <summary>
        /// Wind speed
        /// </summary>
        [XmlAttribute(AttributeName = "value")]
        public double Value { get; set; }
    }

    [XmlRoot(ElementName = "sun")]
    public class Sun
    {
        /// <summary>
        /// Sunrise time
        /// </summary>
        [XmlAttribute(AttributeName = "rise")]
        public DateTime Rise { get; set; }

        /// <summary>
        ///  Sunset time
        /// </summary>
        [XmlAttribute(AttributeName = "set")]
        public DateTime Set { get; set; }
    }

    [XmlRoot(ElementName = "temperature")]
    public class Temperature
    {
        /// <summary>
        /// Maximum temperature at the moment of calculation. This is maximal currently observed
        /// temperature (within large megalopolises and urban areas), use this parameter optionally.
        /// </summary>
        [XmlAttribute(AttributeName = "max")]
        public double Max { get; set; }

        /// <summary>
        /// Minimum temperature at the moment of calculation. This is minimal currently observed
        /// temperature (within large megalopolises and urban areas), use this parameter optionally.
        /// </summary>
        [XmlAttribute(AttributeName = "min")]
        public double Min { get; set; }

        /// <summary>
        /// Unit of measurements. Possible value is Celsius, Kelvin, Fahrenheit.
        /// </summary>
        [XmlAttribute(AttributeName = "unit")]
        public string? Unit { get; set; }

        /// <summary>
        /// Temperature
        /// </summary>
        [XmlAttribute(AttributeName = "value")]
        public double Value { get; set; }
    }

    [XmlRoot(ElementName = "visibility")]
    public class Visibility
    {
        /// <summary>
        /// Visibility, meter
        /// </summary>
        [XmlAttribute(AttributeName = "value")]
        public int Value { get; set; }
    }

    [XmlRoot(ElementName = "weather")]
    public class Weather
    {
        /// <summary>
        /// Weather icon id
        /// </summary>
        [XmlAttribute(AttributeName = "icon")]
        public string? Icon { get; set; }

        /// <summary>
        /// Weather condition id
        /// </summary>
        [XmlAttribute(AttributeName = "number")]
        public int Number { get; set; }

        /// <summary>
        /// Weather condition name
        /// </summary>
        [XmlAttribute(AttributeName = "value")]
        public string? Value { get; set; }
    }

    [XmlRoot(ElementName = "wind")]
    public class Wind
    {
        [XmlElement(ElementName = "direction")]
        public Direction? Direction { get; set; }

        [XmlElement(ElementName = "gusts")]
        public Gusts? Gusts { get; set; }

        [XmlElement(ElementName = "speed")]
        public Speed? Speed { get; set; }
    }
}
