using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using LennyBOTv2.Services;

namespace LennyBOTv2.Modules
{
    public class GeneralModule : LennyModuleBase
    {
        [Command("decide")]
        public async Task DecideCmdAsync(params string[] args)
        {
            var r = LennyServiceProvider.Instance.Rng.Next(0, args.Length);
            await this.ReplyAsync(args[r]);
        }

        [Command("emojify")]
        public async Task EmojifyAsync([Remainder] string text)
        {
            var stringBuilder = new StringBuilder();
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
                        stringBuilder.Append($":regional_indicator_{ch}: ");
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
                        stringBuilder.Append($"**{ch.ToString().ToUpper()}** ");
                        break;
                }
            }
            await this.ReplyAsync(stringBuilder.ToString());
            await this.Context.Message.DeleteAsync();
        }
    }
}