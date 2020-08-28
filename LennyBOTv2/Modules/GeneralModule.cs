using System;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Seky16.Extensions;

namespace LennyBOTv2.Modules
{
    public class GeneralModule : LennyModuleBase
    {
        [Command("clap")]
        public async Task ClapCmdAsync([Remainder] string text)
        {
            var clapped = $"{Context.Message.Author.GetNickname()}:{Environment.NewLine}";
            var split = text.ToUpperInvariant().Split(" ", StringSplitOptions.RemoveEmptyEntries);
            clapped += string.Join(" :clap: ", split) + " :clap:";
            await ReplyAsync(clapped).ConfigureAwait(false);
            await Context.Message.DeleteAsync().ConfigureAwait(false);
        }

        [Command("decide")]
        public async Task DecideCmdAsync(params string[] args)
        {
            await ReplyAsync(args.Random()).ConfigureAwait(false);
        }

        [Command("emojify")]
        public async Task EmojifyCmdAsync([Remainder] string text)
        {
            var stringBuilder = new StringBuilder(Context.Message.Author.GetNickname());
            stringBuilder.AppendLine(":");
            foreach (var ch in text.ToLowerInvariant())
            {
                switch (ch)
                {
                    case 'a':
                    case 'b':
                    case 'c':
                    case 'd':
                    case 'e':
                    case 'f':
                    case 'g':
                    case 'h':
                    case 'i':
                    case 'j':
                    case 'k':
                    case 'l':
                    case 'm':
                    case 'n':
                    case 'o':
                    case 'p':
                    case 'q':
                    case 'r':
                    case 's':
                    case 't':
                    case 'u':
                    case 'v':
                    case 'w':
                    case 'x':
                    case 'y':
                    case 'z':
                        stringBuilder.Append(":regional_indicator_").Append(ch).Append(": ");
                        break;

                    case '0':
                        stringBuilder.Append(":zero: ");
                        break;

                    case '1':
                        stringBuilder.Append(":one: ");
                        break;

                    case '2':
                        stringBuilder.Append(":two: ");
                        break;

                    case '3':
                        stringBuilder.Append(":three: ");
                        break;

                    case '4':
                        stringBuilder.Append(":four: ");
                        break;

                    case '5':
                        stringBuilder.Append(":five: ");
                        break;

                    case '6':
                        stringBuilder.Append(":six: ");
                        break;

                    case '7':
                        stringBuilder.Append(":seven: ");
                        break;

                    case '8':
                        stringBuilder.Append(":eight: ");
                        break;

                    case '9':
                        stringBuilder.Append(":nine: ");
                        break;

                    case '!':
                        stringBuilder.Append(":exclamation: ");
                        break;

                    case '?':
                        stringBuilder.Append(":question: ");
                        break;

                    case '+':
                        stringBuilder.Append(":heavy_plus_sign: ");
                        break;

                    case '-':
                        stringBuilder.Append(":heavy_minus_sign: ");
                        break;

                    case '$':
                        stringBuilder.Append(":heavy_dollar_sign: ");
                        break;

                    default:
                        stringBuilder.Append("**").Append(ch.ToString().ToUpper()).Append("** ");
                        break;
                }
            }
            await ReplyAsync(stringBuilder.ToString()).ConfigureAwait(false);
            await Context.Message.DeleteAsync().ConfigureAwait(false);
        }

        [Command("radical")]
        public async Task RadicalCmdAsync([Remainder] string text)
        {
            var radical = $"{Context.Message.Author.GetNickname()}:{Environment.NewLine}";
            var split = text.ToUpperInvariant().Split(" ", StringSplitOptions.RemoveEmptyEntries);
            radical += string.Join(" :radicalmeme: ", split) + " :radicalmeme:";
            await ReplyAsync(radical).ConfigureAwait(false);
            await Context.Message.DeleteAsync().ConfigureAwait(false);
        }
    }
}
