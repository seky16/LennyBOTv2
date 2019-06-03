﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Google.Apis.YouTube.v3;
using HtmlAgilityPack;
using LennyBOTv2.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OMDbApiNet;

namespace LennyBOTv2.Modules
{
    // todo: make a service?
    public class SearchModule : LennyModuleBase
    {
        public SearchModule(AsyncOmdbClient omdb, YouTubeService youTube)
        {
            this.Omdb = omdb;
            this.YouTube = youTube;
        }

        public AsyncOmdbClient Omdb { get; }
        public YouTubeService YouTube { get; }

        [Command("amd")]
        public async Task AmdCmdAsync()
        {
            var doc = new HtmlWeb().Load("https://finance.yahoo.com/quote/AMD");
            var header = doc.GetElementbyId("quote-header-info");
            var info = header.LastChild.FirstChild.FirstChild.ChildNodes.Select(n => n.InnerText).ToList();
            await ReplyAsync($"**{info[0]}$**\n{info[1]}\n{info[2]}").ConfigureAwait(false);
        }

        [Command("imdb", RunMode = RunMode.Async)]
        public async Task ImdbCmdAsync([Remainder]string query)
        {
            var author = new EmbedAuthorBuilder()
                .WithName("IMDb")
                .WithIconUrl("http://files.softicons.com/download/social-media-icons/flat-gradient-social-icons-by-guilherme-lima/png/512x512/IMDb.png")
                .WithUrl("https://www.imdb.com/");
            var list = await Omdb.GetSearchListAsync(query).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(list.Error))
            {
                await this.MarkCmdFailedAsync($"OMDB: {list.Error}").ConfigureAwait(false);
                return;
            }
            if (list.SearchResults.Count == 1)
            {
                var item = await Omdb.GetItemByIdAsync(list.SearchResults[0].ImdbId).ConfigureAwait(false);
                var embed = new EmbedBuilder()
                    .WithColor(new Color(248, 231, 28))
                    .WithCurrentTimestamp()
                    .WithTitle($"{item.Title} ({item.Year})")
                    .WithDescription($"{item.Plot} ({item.Runtime})")
                    .WithUrl($"https://www.imdb.com/title/{ item.ImdbId}")
                    .WithImageUrl(item.Poster)
                    .WithAuthor(author)
                    .AddField("Rating", $"{item.ImdbRating}\nMetascore: {item.Metascore}")
                    .AddField("Info", $"Director: {item.Director}\nWriter: {item.Writer}\nCast: {item.Actors}\nGenre: {item.Genre}\nCountry: {item.Country}")
                    .AddField("Release dates", $"Released: {item.Released}\nDVD: {item.Dvd}", true)
                    .AddField("Trivia", $"Box office: {item.BoxOffice}\nAwards: {item.Awards}")
                    .Build();
                await ReplyAsync(embed: embed).ConfigureAwait(false);
                return;
            }
            var pages = new List<string>();
            foreach (var result in list.SearchResults)
            {
                var item = await Omdb.GetItemByIdAsync(result.ImdbId).ConfigureAwait(false);
                var str = $@"[**{item.Title} ({item.Year})**](https://www.imdb.com/title/{ item.ImdbId})
{item.Plot} ({item.Runtime})
**Rating**
{item.ImdbRating}
Metascore: {item.Metascore}
**Info**
Director: {item.Director}
Writer: {item.Writer}
Cast: {item.Actors}
Genre: {item.Genre}
Country: {item.Country}
**Release dates**
Released: {item.Released}
DVD: {item.Dvd}
**Trivia**
Box office: {item.BoxOffice}
Awards: {item.Awards}";
                pages.Add(str);
            }
            var msg = new PaginatedMessage() { Title = $"Search results for *{query}*", Author = author, Color = new Color(248, 231, 28), Pages = pages };

            await this.PagedReplyAsync(msg, false).ConfigureAwait(false);
        }

        [Command("lmgtfy")]
        public Task LmgtfyCmdAsync([Remainder] string search = "How to use Lmgtfy")
            => this.ReplyAsync($"**Your special URL: **<http://lmgtfy.com/?q={ Uri.EscapeUriString(search)}>");

        [Command("urban")]
        public async Task UrbanCmdAsync([Remainder] string query)
        {
            string jsonString = string.Empty;
            using (var client = new HttpClient())
            {
                var result = await client.GetAsync(string.Format("http://api.urbandictionary.com/v0/define?term={0}", query.Replace(' ', '+'))).ConfigureAwait(false);
                if (!result.IsSuccessStatusCode)
                {
                    await this.MarkCmdFailedAsync($"UrbanDict API returned {result.StatusCode}").ConfigureAwait(false);
                    return;
                }

                jsonString = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            var urbanModel = UrbanModel.FromJson(jsonString);
            if (urbanModel?.List?.Count == 0)
            {
                await this.ReplyAsync($"There are no definitions for word: **{query}**.").ConfigureAwait(false);
                return;
            }

            var pages = new List<string>();
            foreach (var item in urbanModel.List)
            {
                pages.Add(new StringBuilder()
                    .AppendLine(item.Definition.Replace("[", "").Replace("]", ""))
                    .AppendLine()
                    .AppendLine("*Example:*")
                    .AppendLine(item.Example.Replace("[", "").Replace("]", ""))
                    .ToString());
            }
            var author = new EmbedAuthorBuilder()
                .WithName("Urban Dictionary")
                .WithIconUrl("https://d2gatte9o95jao.cloudfront.net/assets/apple-touch-icon-55f1ee4ebfd5444ef5f8d5ba836a2d41.png")
                .WithUrl("https://urbandictionary.com");
            var msg = new PaginatedMessage() { Title = $"Definitions for *{query}*", Author = author, Color = new Color(255, 84, 33), Pages = pages };

            await this.PagedReplyAsync(msg, false).ConfigureAwait(false);
        }

        [Command("weather")]
        public async Task WeatherCmdAsync([Remainder]string query)
        {
            string jsonString = string.Empty;
            using (var client = new HttpClient())
            {
                var result = await client.GetAsync(string.Format("https://api.apixu.com/v1/forecast.json?key=4cc6123614b84851a8995240190306&q={0}", query.Replace(' ', '+'))).ConfigureAwait(false);
                if (!result.IsSuccessStatusCode)
                {
                    await this.MarkCmdFailedAsync($"Apixu weather API returned {result.StatusCode}").ConfigureAwait(false);
                    return;
                }

                jsonString = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            var model = JsonConvert.DeserializeObject<ApixuWeatherModel.WeatherModel>(jsonString);
            var builder = new EmbedBuilder()
    .WithTitle($"Weather in {model.location.name}, {model.location.country}")
    .WithDescription($"{model.current.temp_c} °C, {model.current.condition.text}")
    .WithThumbnailUrl($"https:{model.current.condition.icon}")
    .WithFooter($"Last update: {model.current.last_updated}")
    /*.WithAuthor(author => {
        author
            .WithName("APIXU")
            .WithUrl("https://www.apixu.com")
            .WithIconUrl("https://cdn.apixu.com/v4/images/logo.png");
    })*/
    .AddField("Details",
    $"Feels like:{model.current.feelslike_c} °C\n" +
    $"Cloud coverage: {model.current.cloud} %\n" +
    $"Precipitation: {model.current.precip_mm} mm\n" +
    $"Humidity: {model.current.humidity} %\n" +
    $"Pressure: {model.current.pressure_mb} mBar\n" +
    $"Wind: {model.current.wind_kph} km/h {model.current.wind_dir}");
            await Context.Channel.SendMessageAsync(
                null,
                embed: builder.Build())
                .ConfigureAwait(false);
        }

        [Command("wiki")]
        public async Task WikiCmdAsync([Remainder] string query)
        {
            using (var client = new HttpClient())
            {
                var getResult = await client.GetAsync($"https://en.wikipedia.org/w/api.php?action=opensearch&search={ Uri.EscapeUriString(query)}").ConfigureAwait(false);

                if (!getResult.IsSuccessStatusCode)
                {
                    await this.MarkCmdFailedAsync($"Wikipedia API returned {getResult.StatusCode}").ConfigureAwait(false);
                    return;
                }

                var getContent = await getResult.Content.ReadAsStringAsync().ConfigureAwait(false);
                JArray responseObject = JsonConvert.DeserializeObject<JArray>(getContent);
                var titles = responseObject[1].ToObject<List<string>>();
                var descriptions = responseObject[2].ToObject<List<string>>();
                var urls = responseObject[3].ToObject<List<string>>();

                var pages = new List<string>();
                for (int i = 0; i < titles.Count; i++)
                {
                    pages.Add(new StringBuilder()
                    .Append("[**").Append(titles[i]).Append("**](").Append(urls[i]).AppendLine(")")
                    .AppendLine(descriptions[i])
                    .ToString());
                }

                var author = new EmbedAuthorBuilder()
                    .WithName("Wikipedia")
                    .WithIconUrl("https://upload.wikimedia.org/wikipedia/commons/d/de/Wikipedia_Logo_1.0.png")
                    .WithUrl("https://en.wikipedia.org/wiki/Main_Page");
                var msg = new PaginatedMessage() { Title = $"Search results for *{query}*", Author = author, Color = new Color(255, 255, 255), Pages = pages };

                await this.PagedReplyAsync(msg, false).ConfigureAwait(false);
            }
        }

        [Command("yt")]
        public async Task YouTubeCmdAsync([Remainder]string query)
        {
            var request = this.YouTube.Search.List("snippet");
            request.Q = query;
            request.MaxResults = 5;
            request.SafeSearch = SearchResource.ListRequest.SafeSearchEnum.None;
            request.Type = "video";
            request.RelevanceLanguage = "en";
            var response = await request.ExecuteAsync().ConfigureAwait(false);

            if (response.Items.Count == 0)
            {
                await ReplyAsync("No results.").ConfigureAwait(false);
                return;
            }
            var sb = new StringBuilder();
            var first = true;
            // todo: better presentation
            foreach (var item in response.Items)
            {
                var title = $"{item.Snippet.Title} *({item.Snippet.ChannelTitle})*";
                var url = $"https://www.youtube.com/watch?v={ item.Id.VideoId}";
                if (first)
                {
                    //sb.AppendLine(title);
                    sb.AppendLine(url);
                    first = false;
                }
                else
                {
                    sb.AppendLine(title);
                    sb.Append('<').Append(url).AppendLine(">");
                }
                break; // todo: dirty fix because of sean
            }
            await ReplyAsync(sb.ToString()).ConfigureAwait(false);
        }
    }
}