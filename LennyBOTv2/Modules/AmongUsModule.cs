using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using LennyBOTv2.Preconditions;
using LennyBOTv2.Services;

namespace LennyBOTv2.Modules
{
    public class AmongUsModule : LennyModuleBase
    {
        private readonly AmongUsService _auService;

        public AmongUsModule(AmongUsService auService)
        {
            _auService = auService;
        }

        [Command("impostor")]
        [AmongUsWriteStats]
        public async Task ImpostorCmdAsync(IUser player, string gameResult)
        {
            try
            {
                _auService.WriteStats(player, gameResult);
            }
            catch (Exception ex)
            {
                await Context.MarkCmdFailedAsync(ex.ToString()).ConfigureAwait(false);
                return;
            }

            await Context.Message.AddReactionAsync(new Emoji("✅")).ConfigureAwait(false);
        }

        [Command("stats")]
        [AmongUsServer]
        public async Task StatsCmdAsync()
        {
            var impostors = _auService.GetStats().OrderByDescending(i => i.Winrate);

            var sb = new StringBuilder();

            for (var i = 1; i <= impostors.Count(); i++)
            {
                var impostor = impostors.ElementAt(i - 1);
                await LoggingService.LogInfoAsync($"{impostor.Nickname} ({impostor.Id}) {impostor.Wins}/{impostor.Wins + impostor.Losses} ({impostor.Winrate:P2})").ConfigureAwait(false);

                var place = i switch
                {
                    1 => ":first_place:",
                    2 => ":second_place:",
                    3 => ":third_place:",
                    _ => $"**{i}**",
                };

                sb.AppendFormat("{0} {1} {2}/{3} ({4:P2})", place, impostor.Nickname, impostor.Wins, impostor.Wins + impostor.Losses, impostor.Winrate).AppendLine();
            }

            var embed = new EmbedBuilder()
                    .WithColor(new Color(255, 0, 0))
                    .WithCurrentTimestamp()
                    .WithTitle("Top impostors")
                    .WithDescription(sb.ToString())
                    .WithThumbnailUrl("https://i.imgur.com/vdPEmSE.png");

            await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
        }
    }
}
