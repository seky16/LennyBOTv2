﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Discord;
using LennyBOTv2.Models;
using LennyBOTv2.Services;

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

        public static async Task<T?> GetFromJsonAsync<T>(FormattableString url) where T : BaseJsonModel<T>
        {
            var jsonStr = await GetStringAsync(url).ConfigureAwait(false);

            if (string.IsNullOrEmpty(jsonStr))
            {
                await LoggingService.LogErrorAsync($"'{url}' returned empty string", nameof(GetFromJsonAsync)).ConfigureAwait(false);
                return null;
            }

            try
            {
                return BaseJsonModel<T>.FromJson(jsonStr);
            }
            catch (Exception ex)
            {
                await LoggingService.LogExceptionAsync(ex, typeof(T).Name, jsonStr).ConfigureAwait(false);
                return null;
            }
        }

        public static async Task<IEnumerable<T?>?> GetFromJsonArrayAsync<T>(FormattableString url) where T : BaseJsonModel<T>
        {
            var jsonStr = await GetStringAsync(url).ConfigureAwait(false);

            if (string.IsNullOrEmpty(jsonStr))
            {
                await LoggingService.LogErrorAsync($"'{url}' returned empty string", nameof(GetFromJsonAsync)).ConfigureAwait(false);
                return null;
            }

            try
            {
                return BaseJsonModel<T>.FromJsonArray(jsonStr);
            }
            catch (Exception ex)
            {
                await LoggingService.LogExceptionAsync(ex, typeof(T).Name, jsonStr).ConfigureAwait(false);
                return null;
            }
        }

        public static async Task<T?> GetFromXmlAsync<T>(FormattableString url) where T : class
        {
            var xmlStr = await GetStringAsync(url).ConfigureAwait(false);

            if (string.IsNullOrEmpty(xmlStr))
            {
                await LoggingService.LogErrorAsync($"'{url}' returned empty string", nameof(GetFromXmlAsync)).ConfigureAwait(false);
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
                await LoggingService.LogErrorAsync($"'{safeUrl}' returned {result.StatusCode}", nameof(GetStringAsync)).ConfigureAwait(false);
                return null;
            }

            return await result.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        private static readonly Regex _emoteRegex = new Regex(@"<a?:\w+:\d+>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static string ReplaceEmotesInText(string text)
        {
            return _emoteRegex.Replace(
                text,
                match => Emote.TryParse(match.Value, out var emote) ? emote.ToString() : match.Value);
        }
    }
}
