using System;
using System.Linq;
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
            var clapped = $"{Context.Message.Author.Mention}:{Environment.NewLine}";
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
            var stringBuilder = new StringBuilder(Context.Message.Author.Mention);
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

        [Command("fibonacci"), Alias("fib")]
        public async Task FibonacciSpacingCmdAsync([Remainder] string text)
        {
            text = text.Replace(" ", "").Replace("\n", "");

            if (text.Length > 16)
            {
                await ReplyAsync("too long for discord 😦").ConfigureAwait(false);
                return;
            }

            var fibs = new[] { 0, 1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89,
                                144, 233, 377, 610, 987, 1597, 2584, 4181, 6765, 10946, 17711,
                                28657, 46368, 75025, 121393, 196418, 317811, 514229, 832040,
                                1346269, 2178309, 3524578, 5702887, 9227465, 14930352, 24157817,
                                39088169, 63245986, 102334155, 165580141, 267914296, 433494437,
                                701408733, 1134903170, 1836311903};

            int Fib(int n)
            {
                if (n < fibs.Length) return fibs[n];
                return Fib(n - 1) + Fib(n - 2);
            }

            var sb = new StringBuilder();
            for (var i = 0; i < text.Length; i++)
            {
                var spaces = Enumerable.Repeat(' ', Fib(i));
                sb.Append(spaces.ToArray());
                sb.Append(text[i]);
            }

            await ReplyAsync(sb.ToString()).ConfigureAwait(false);
        }

        [Command("radical")]
        public async Task RadicalCmdAsync([Remainder] string text)
        {
            var radical = $"{Context.Message.Author.Mention}:{Environment.NewLine}";
            var split = text.ToUpperInvariant().Split(" ", StringSplitOptions.RemoveEmptyEntries);

            radical += string.Join(" <:radicalmeme:269806756589207553> ", split) + " <:radicalmeme:269806756589207553>";
            await ReplyAsync(radical).ConfigureAwait(false);
            await Context.Message.DeleteAsync().ConfigureAwait(false);
        }
    }
}
