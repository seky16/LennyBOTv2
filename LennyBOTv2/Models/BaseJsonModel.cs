using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LennyBOTv2.Models
{
    public abstract class BaseJsonModel<T> where T : BaseJsonModel<T>
    {
        private static readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };

        public static T FromJson(string json) => JsonConvert.DeserializeObject<T>(json, _settings) ?? throw new ArgumentException("Wrong input", nameof(json));
        public static IEnumerable<T?> FromJsonArray(string json) => JsonConvert.DeserializeObject<IEnumerable<T>>(json, _settings) ?? throw new ArgumentException("Wrong input", nameof(json));

        public string ToJson() => JsonConvert.SerializeObject(this, _settings);
    }
}
