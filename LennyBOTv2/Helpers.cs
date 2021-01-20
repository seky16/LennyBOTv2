using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;
using LennyBOTv2.Services;
using Newtonsoft.Json;

namespace LennyBOTv2
{
    public static class Helpers
    {
        /// <summary>
        /// Build safe (escaped) url using string interpolation
        /// </summary>
        /// <param name="url">interpolated url string</param>
        /// <returns>Safe (escaped) url</returns>
        /// <remarks>https://medium.com/@j2jensen/c-6s-string-interpolation-feature-is-actually-great-for-encoding-parameters-ab139471b133</remarks>
        public static string BuildSafeUrl(FormattableString url)
        {
            var invariantParameters = url.GetArguments()
              .Select(a => FormattableString.Invariant($"{a}"));
            var escapedParameters = invariantParameters
              .Select(Uri.EscapeDataString)
              .Cast<object?>()
              .ToArray();

            // check for somewhat valid URL
            // not perfect, but good enough https://stackoverflow.com/questions/7578857/how-to-check-whether-a-string-is-a-valid-http-url#comment80682416_7581824
            var uri = new Uri(string.Format(url.Format, escapedParameters), UriKind.Absolute);

            if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
                return uri.AbsoluteUri;

            throw new UriFormatException($"'{url}' is not a valid URL");
        }

        public static async Task<T?> GetFromJsonAsync<T>(FormattableString url) where T : class
        {
            var jsonStr = await GetStringAsync(url).ConfigureAwait(false);

            if (string.IsNullOrEmpty(jsonStr))
            {
                await LoggingService.LogErrorAsync($"'{url}' returned empty string", nameof(GetFromJsonAsync)).ConfigureAwait(false);
                return null;
            }

            T? model;
            try
            {
                model = JsonConvert.DeserializeObject<T>(jsonStr);
            }
            catch (Exception ex)
            {
                await LoggingService.LogExceptionAsync(ex, typeof(T).Name, jsonStr).ConfigureAwait(false);
                return null;
            }

            return model;
        }

        public static async Task<T?> GetFromXmlAsync<T>(FormattableString url) where T : class
        {
            var xmlStr = await GetStringAsync(url).ConfigureAwait(false);

            if (string.IsNullOrEmpty(xmlStr))
            {
                await LoggingService.LogErrorAsync($"'{url}' returned empty string", nameof(GetFromJsonAsync)).ConfigureAwait(false);
                return null;
            }

            T? model;
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                using var reader = new StringReader(xmlStr);
                model = serializer.Deserialize(reader) as T;
            }
            catch (Exception ex)
            {
                await LoggingService.LogExceptionAsync(ex, typeof(T).Name, xmlStr).ConfigureAwait(false);
                return null;
            }

            return model;
        }

        public static async Task<string?> GetStringAsync(FormattableString url)
        {
            var safeUrl = BuildSafeUrl(url);

            using var client = new HttpClient();
            var result = await client.GetAsync(safeUrl).ConfigureAwait(false);
            if (!result.IsSuccessStatusCode)
            {
                await LoggingService.LogErrorAsync($"'{safeUrl}' returned {result.StatusCode}", nameof(GetFromJsonAsync)).ConfigureAwait(false);
                return null;
            }

            return await result.Content.ReadAsStringAsync().ConfigureAwait(false);
        }
    }
}
