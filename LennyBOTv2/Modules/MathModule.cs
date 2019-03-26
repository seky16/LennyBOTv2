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
                var get = await client.GetAsync(url);
                if (!get.IsSuccessStatusCode)
                {
                    await this.MarkCmdFailedAsync($"math.js API returned {get.StatusCode}");
                    return;
                }

                var result = await get.Content.ReadAsStringAsync();
                await this.ReplyAsync($"`{expression} = {result}`");
            }
        }

        [Command("conv")]
        public async Task ConvCmdAsync(string amount, string from, [Remainder]string to)
        {
            amount = amount.Replace(',', '.');
            if (!double.TryParse(amount, out var amountD))
            {
                await MarkCmdFailedAsync($"Unable to parse {amount} to double");
                return;
            }

            if (to.ToLowerInvariant().StartsWith("to "))
                to = to.Substring(3);

            var result = await Fixer.ConvertAsync(from, to, amountD);
            result = Math.Round(result, 2);
            var builder = new EmbedBuilder()
                .WithColor(Color.DarkGreen)
                .WithAuthor(new EmbedAuthorBuilder().WithName(Context.User.GetNickname()).WithIconUrl(Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl()))
                .WithCurrentTimestamp()
                .WithDescription($"{amountD.ToString(CultureInfo.InvariantCulture)} {from.ToUpper()} = **{result.ToString(CultureInfo.InvariantCulture)} {to.ToUpper()}**");
            await this.ReplyAsync(string.Empty, false, builder.Build());
        }
    }
}