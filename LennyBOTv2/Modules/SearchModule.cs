using System;
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

        public SearchModule(AsyncOmdbClient omdb, YouTubeService youTube)
        {
            Omdb = omdb;
            YouTube = youTube;
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
        public async Task ImdbCmdAsync([Remainder] string query)
        {
            var author = new EmbedAuthorBuilder()
                .WithName("IMDb")
                .WithIconUrl("http://files.softicons.com/download/social-media-icons/flat-gradient-social-icons-by-guilherme-lima/png/512x512/IMDb.png")
                .WithUrl("https://www.imdb.com/");
            var list = await Omdb.GetSearchListAsync(query).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(list.Error))
            {
                await Context.MarkCmdFailedAsync($"OMDB: {list.Error}").ConfigureAwait(false);
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

            await PagedReplyAsync(msg, false).ConfigureAwait(false);
        }

        [Command("lmgtfy")]
        public Task LmgtfyCmdAsync([Remainder] string search = "How to use Lmgtfy")
            => ReplyAsync($"**Your special URL: **<http://lmgtfy.com/?q={ Uri.EscapeUriString(search)}>");

        [Command("urban")]
        public async Task UrbanCmdAsync([Remainder] string query)
        {
            var jsonString = string.Empty;
            using (var client = new HttpClient())
            {
                var result = await client.GetAsync(string.Format("http://api.urbandictionary.com/v0/define?term={0}", query.Replace(' ', '+'))).ConfigureAwait(false);
                if (!result.IsSuccessStatusCode)
                {
                    await Context.MarkCmdFailedAsync($"UrbanDict API returned {result.StatusCode}").ConfigureAwait(false);
                    return;
                }

                jsonString = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            var urbanModel = UrbanModel.FromJson(jsonString);
            if (urbanModel?.List is null || urbanModel.List.Count == 0)
            {
                await ReplyAsync($"There are no definitions for word: **{query}**.").ConfigureAwait(false);
                return;
            }

            var pages = new List<string>();
            foreach (var item in urbanModel.List.OrderByDescending(x => x.ThumbsUp))
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

            await PagedReplyAsync(msg, false).ConfigureAwait(false);
        }

        [Command("weather")]
        public async Task WeatherCmdAsync()
        {
            if (!_weatherDefaults.TryGetValue(Context.Message.Author.Id, out var location))
            {
                await Context.MarkCmdFailedAsync($"{Context.Message.Author.GetNickname()} ({Context.Message.Author.Id}) doesn't have default location set.").ConfigureAwait(false);
                return;
            }

            await WeatherCmdAsync(location).ConfigureAwait(false);
        }

        [Command("weather")]
        public async Task WeatherCmdAsync([Remainder] string query)
        {
            var jsonString = string.Empty;
            using (var client = new HttpClient())
            {
                var result = await client.GetAsync(string.Format("http://api.weatherstack.com/current?access_key={1}&query={0}", query.Replace(' ', '+'), Config["weatherstackAPIkey"])).ConfigureAwait(false);
                if (!result.IsSuccessStatusCode)
                {
                    await Context.MarkCmdFailedAsync($"Weatherstack API returned {result.StatusCode}").ConfigureAwait(false);
                    return;
                }

                jsonString = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            var model = JsonConvert.DeserializeObject<WeatherstackModel.WeatherModel>(jsonString);

            if (!model.success || model.current is null || model.location is null)
            {
                await Context.MarkCmdFailedAsync(jsonString).ConfigureAwait(false);
                return;
            }

            var updatedOnUtc = DateTimeOffset.FromUnixTimeSeconds(model.location.localtime_epoch).AddHours(-model.location.utc_offset);

            var embed = new EmbedBuilder()
                .WithTitle($"Weather in {model.location.name}, {model.location.region}, {model.location.country}")
                .WithDescription($"{model.current.temperature} °C {model.current.weather_descriptions.FirstOrDefault()}")
                .WithThumbnailUrl(model.current.weather_icons.FirstOrDefault())
                .WithFooter($"Last update: {updatedOnUtc.ToPragueTimeString()}")
                .WithAuthor(author =>
                {
                    author
                        .WithName("weatherstack")
                        .WithUrl("https://weatherstack.com")
                        .WithIconUrl("https://weatherstack.com/site_images/weatherstack_icon.png");
                })
                .AddField("Details",
                $"Feels like: {model.current.feelslike} °C\n" +
                $"Cloud coverage: {model.current.cloudcover} %\n" +
                $"Precipitation: {model.current.precip} mm\n" +
                $"Humidity: {model.current.humidity} %\n" +
                $"Pressure: {model.current.pressure} mBar\n" +
                $"Wind: {model.current.wind_speed} km/h {model.current.wind_dir}");
            await ReplyEmbedAsync(embed).ConfigureAwait(false);
        }

        [Command("wiki")]
        public async Task WikiCmdAsync([Remainder] string query)
        {
            using var client = new HttpClient();
            var getResult = await client.GetAsync($"https://en.wikipedia.org/w/api.php?action=opensearch&search={ Uri.EscapeUriString(query)}").ConfigureAwait(false);

            if (!getResult.IsSuccessStatusCode)
            {
                await Context.MarkCmdFailedAsync($"Wikipedia API returned {getResult.StatusCode}").ConfigureAwait(false);
                return;
            }

            var getContent = await getResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var responseObject = JsonConvert.DeserializeObject<JArray>(getContent);
            var titles = responseObject[1].ToObject<List<string>>();
            var descriptions = responseObject[2].ToObject<List<string>>();
            var urls = responseObject[3].ToObject<List<string>>();

            var pages = new List<string>();
            for (var i = 0; i < titles.Count; i++)
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

            await PagedReplyAsync(msg, false).ConfigureAwait(false);
        }

        [Command("youtube"), Alias("yt")]
        public async Task YouTubeCmdAsync([Remainder] string query)
        {
            var request = YouTube.Search.List("snippet");
            request.Q = query;
            request.MaxResults = 1;
            request.SafeSearch = SearchResource.ListRequest.SafeSearchEnum.None;
            request.Type = "video";
            request.RelevanceLanguage = "en";
            var response = await request.ExecuteAsync().ConfigureAwait(false);

            if (response.Items.Count == 0)
            {
                await ReplyAsync("No results.").ConfigureAwait(false);
                return;
            }
            var video = $"https://www.youtube.com/watch?v={ response.Items[0].Id.VideoId}";
            await ReplyAsync(video).ConfigureAwait(false);
        }
    }
}
