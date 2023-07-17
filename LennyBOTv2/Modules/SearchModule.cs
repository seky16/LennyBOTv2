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
using LennyBOTv2.Models.Definition;
using LennyBOTv2.Models.UrbanDictionary;
using LennyBOTv2.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OMDbApiNet;

namespace LennyBOTv2.Modules
{
    // todo: make a service?
    public class SearchModule : LennyModuleBase
    {
        private readonly AsyncOmdbClient _omdb;
        private readonly WeatherService _weather;
        private readonly YouTubeService _youTube;

        public SearchModule(AsyncOmdbClient omdb, YouTubeService youTube, WeatherService weather)
        {
            _omdb = omdb;
            _youTube = youTube;
            _weather = weather;
        }

        [Command("amd")]
        public async Task AmdCmdAsync()
        {
            var doc = new HtmlWeb().Load("https://finance.yahoo.com/quote/AMD");
            var header = doc.GetElementbyId("quote-header-info");
            var info = header.LastChild.FirstChild.FirstChild.ChildNodes.Select(n => n.InnerText).ToList();
            await ReplyAsync($"{(info[0] + "$").Bold()}\n{info[1]}\n{info[2]}").ConfigureAwait(false);
        }

        [Command("imdb", RunMode = RunMode.Async)]
        public async Task ImdbCmdAsync([Remainder] string query)
        {
            var author = new EmbedAuthorBuilder()
                .WithName("IMDb")
                .WithIconUrl("http://files.softicons.com/download/social-media-icons/flat-gradient-social-icons-by-guilherme-lima/png/512x512/IMDb.png")
                .WithUrl("https://www.imdb.com/");
            var list = await _omdb.GetSearchListAsync(query).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(list.Error))
            {
                await Context.MarkCmdFailedAsync($"OMDB: {list.Error}").ConfigureAwait(false);
                return;
            }
            if (list.SearchResults.Count == 1)
            {
                var item = await _omdb.GetItemByIdAsync(list.SearchResults[0].ImdbId).ConfigureAwait(false);
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
                var item = await _omdb.GetItemByIdAsync(result.ImdbId).ConfigureAwait(false);
                var str = $@"{$"{item.Title} ({item.Year})".Bold().Hyperlink(Helpers.BuildSafeUrl($"https://www.imdb.com/title/{item.ImdbId}"))}
{item.Plot} ({item.Runtime})
{"Rating".Bold()}
{item.ImdbRating}
Metascore: {item.Metascore}
{"Info".Bold()}
Director: {item.Director}
Writer: {item.Writer}
Cast: {item.Actors}
Genre: {item.Genre}
Country: {item.Country}
{"Release dates".Bold()}
Released: {item.Released}
DVD: {item.Dvd}
{"Trivia".Bold()}
Box office: {item.BoxOffice}
Awards: {item.Awards}";
                pages.Add(str);
            }
            var msg = new PaginatedMessage() { Title = $"Search results for {query.Italics()}", Author = author, Color = new Color(248, 231, 28), Pages = pages };

            await PagedReplyAsync(msg, false).ConfigureAwait(false);
        }

        [Command("lmgtfy")]
        public Task LmgtfyCmdAsync([Remainder] string search = "How to use Lmgtfy")
            => ReplyAsync("Your special URL: ".Bold() + Helpers.BuildSafeUrl($"http://lmgtfy.com/?q={Uri.EscapeDataString(search)}").HideLinkPreview());

        [Command("urban", RunMode = RunMode.Async)]
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
                await ReplyAsync($"There are no definitions for word: {query.Bold()}.").ConfigureAwait(false);
                return;
            }

            var pages = new List<string>();
            foreach (var item in urbanModel.List.Where(x => !string.IsNullOrEmpty(x?.Definition) && !string.IsNullOrEmpty(x.Example)).OrderByDescending(x => x.ThumbsUp))
            {
                pages.Add(new StringBuilder()
                    .AppendLine(item!.Definition!.Replace("[", "").Replace("]", ""))
                    .AppendLine()
                    .AppendLine("Example:".Italics())
                    .AppendLine(item.Example!.Replace("[", "").Replace("]", ""))
                    .ToString());
            }
            var author = new EmbedAuthorBuilder()
                .WithName("Urban Dictionary")
                .WithIconUrl("https://d2gatte9o95jao.cloudfront.net/assets/apple-touch-icon-55f1ee4ebfd5444ef5f8d5ba836a2d41.png")
                .WithUrl("https://urbandictionary.com");
            var msg = new PaginatedMessage() { Title = $"Definitions for {query.Italics()}", Author = author, Color = new Color(255, 84, 33), Pages = pages };

            await PagedReplyAsync(msg, false).ConfigureAwait(false);
        }

        [Command("weather", RunMode = RunMode.Async)]
        public async Task WeatherCmdAsync()
        {
            if (!_weather.TryGetDefaultLocation(Context.Message.Author.Id, out var location))
            {
                await Context.MarkCmdFailedAsync($"{Context.Message.Author.GetNickname()} ({Context.Message.Author.Id}) doesn't have default location set.").ConfigureAwait(false);
                return;
            }

            await WeatherCmdAsync(location).ConfigureAwait(false);
        }

        [Command("weather", RunMode = RunMode.Async)]
        public async Task WeatherCmdAsync([Remainder] string location)
        {
            var result = await _weather.GetWeatherForLocationAsync(location).ConfigureAwait(false);

            if (!result.Any())
            {
                await Context.MarkCmdFailedAsync("No results").ConfigureAwait(false);
                return;
            }

            await PagedReplyAsync(result).ConfigureAwait(false);
        }

        [Command("wiki", RunMode = RunMode.Async)]
        public async Task WikiCmdAsync([Remainder] string query)
        {
            using var client = new HttpClient();
            var getResult = await client.GetAsync($"https://en.wikipedia.org/w/api.php?action=opensearch&search={ Uri.EscapeDataString(query)}").ConfigureAwait(false);

            if (!getResult.IsSuccessStatusCode)
            {
                await Context.MarkCmdFailedAsync($"Wikipedia API returned {getResult.StatusCode}").ConfigureAwait(false);
                return;
            }

            var getContent = await getResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var responseObject = JsonConvert.DeserializeObject<JArray>(getContent);
            var titles = responseObject?[1].ToObject<List<string>>();
            var descriptions = responseObject?[2].ToObject<List<string>>();
            var urls = responseObject?[3].ToObject<List<string>>();

            var pages = new List<string>();
            for (var i = 0; i < titles?.Count; i++)
            {
                pages.Add(new StringBuilder()
                .Append('[').Append(titles[i].Bold()).Append("](").Append(urls?[i]).AppendLine(")")
                .AppendLine(descriptions?[i])
                .ToString());
            }

            var author = new EmbedAuthorBuilder()
                .WithName("Wikipedia")
                .WithIconUrl("https://upload.wikimedia.org/wikipedia/commons/d/de/Wikipedia_Logo_1.0.png")
                .WithUrl("https://en.wikipedia.org/wiki/Main_Page");
            var msg = new PaginatedMessage() { Title = $"Search results for {query.Italics()}", Author = author, Color = new Color(255, 255, 255), Pages = pages };

            await PagedReplyAsync(msg, false).ConfigureAwait(false);
        }

        [Command("youtube", RunMode = RunMode.Async), Alias("yt")]
        public async Task YouTubeCmdAsync([Remainder] string query)
        {
            var request = _youTube.Search.List("snippet");
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

        [Command("definition", RunMode = RunMode.Async), Alias("def")]
        public async Task DefinitionCmdAsync([Remainder] string query)
        {
            var model = await Helpers.GetFromJsonArrayAsync<DefinitionModel>($"https://api.dictionaryapi.dev/api/v2/entries/en/{query}").ConfigureAwait(false);
            if (model?.All(d => d is null) != false)
            {
                await Context.MarkCmdFailedAsync($"No definition for '{query}'").ConfigureAwait(false);
                return;
            }

            var pages = new List<string>();
            foreach (var item in model.Where(d => !(d is null)))
            {
                var sb = new StringBuilder(item!.Word);
                if (item.Phonetics?.Count > 0)
                {
                    item.Phonetics?.ForEach(p =>
                    {
                        if (!string.IsNullOrEmpty(p?.Text))
                            sb.AppendFormat(@" [\[{0}\]]({1})", p.Text, p.Audio);
                    });
                }
                sb.AppendLine();
                if (!string.IsNullOrEmpty(item.Origin))
                    sb.AppendLine(item.Origin);
                item.Meanings?.ForEach(m =>
                {
                    sb.AppendLine();
                    if (!string.IsNullOrEmpty(m.PartOfSpeech))
                        sb.AppendLine(m.PartOfSpeech.Bold());
                    if (m?.Definitions?.Count > 0)
                    {
                        m.Definitions?.ForEach(d =>
                        {
                            if (!string.IsNullOrEmpty(d.Definition))
                                sb.Append("- ").AppendLine(d.Definition);
                            if (!string.IsNullOrEmpty(d.Example))
                                sb.Append('\t').AppendLine(d.Example.Italics());
                            if (d?.Synonyms?.Count > 0)
                                sb.Append("Synonyms: ").AppendJoin(", ", d.Synonyms!).AppendLine();
                            if (d?.Antonyms?.Count > 0)
                                sb.Append("Antonyms: ").AppendJoin(", ", d.Antonyms!).AppendLine();
                        });
                    }
                });
                pages.Add(sb.ToString().Truncate(2048));
            }
            var author = new EmbedAuthorBuilder()
                .WithName("freeDictionaryApi")
                .WithIconUrl("https://media.discordapp.net/attachments/360705292385910784/890171034689482782/favicon.png")
                .WithUrl("https://github.com/meetDeveloper/freeDictionaryAPI");
            var msg = new PaginatedMessage() { Title = $"Definitions for {query.Italics()}", Author = author, Color = new Color(217, 66, 53), Pages = pages };

            await PagedReplyAsync(msg, false).ConfigureAwait(false);
        }
    }
}
