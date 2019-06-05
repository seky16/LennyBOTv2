using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using FixerSharp;

namespace LennyBOTv2.Modules
{
    public class MathModule : LennyModuleBase
    {
        [Command("calc")]
        public async Task CalcCmdAsync([Remainder]string expression)
        {
            expression = expression.Replace("`", string.Empty);
            var url = $"http://api.mathjs.org/v4/?expr={ Uri.EscapeDataString(expression)}";

            using (var client = new HttpClient())
            {
                var get = await client.GetAsync(url).ConfigureAwait(false);
                if (!get.IsSuccessStatusCode)
                {
                    await MarkCmdFailedAsync($"math.js API returned {get.StatusCode}").ConfigureAwait(false);
                    return;
                }

                var result = await get.Content.ReadAsStringAsync().ConfigureAwait(false);
                await ReplyAsync($"`{expression} = {result}`").ConfigureAwait(false);
            }
        }

        [Command("conv")]
        public async Task ConvCmdAsync(string amount, string from, [Remainder]string to)
        {
            amount = amount.Replace(',', '.');
            if (!double.TryParse(amount, out var amountD))
            {
                await MarkCmdFailedAsync($"Unable to parse {amount} to double").ConfigureAwait(false);
                return;
            }

            if (to.StartsWith("to ", StringComparison.OrdinalIgnoreCase))
                to = to.Substring(3);

            var result = await Fixer.ConvertAsync(from, to, amountD).ConfigureAwait(false);
            result = Math.Round(result, 2);
            var embed = new EmbedBuilder()
                .WithColor(Color.DarkGreen)
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithName(Context.User.GetNickname())
                    .WithIconUrl(Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl()))
                .WithCurrentTimestamp()
                .WithDescription($"{amountD.ToString(CultureInfo.InvariantCulture)} {from.ToUpper()} = **{result.ToString(CultureInfo.InvariantCulture)} {to.ToUpper()}**");
            await ReplyEmbedAsync(embed).ConfigureAwait(false);
        }
    }
}