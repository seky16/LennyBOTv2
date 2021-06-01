using System;
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

        [Command("crew")]
        [AmongUsWriteStats]
        public async Task CrewCmdAsync(string gameResult, params IUser[] players)
        {
            try
            {
                _auService.WriteCrewStats(gameResult, players);
            }
            catch (Exception ex)
            {
                await Context.MarkCmdFailedAsync(ex.ToString()).ConfigureAwait(false);
                return;
            }

            await Context.MarkCmdOkAsync().ConfigureAwait(false);
        }

        [Command("impostor")]
        [AmongUsWriteStats]
        public async Task ImpostorCmdAsync(string gameResult, params IUser[] players)
        {
            try
            {
                _auService.WriteImpostorStats(gameResult, players);
            }
            catch (Exception ex)
            {
                await Context.MarkCmdFailedAsync(ex.ToString()).ConfigureAwait(false);
                return;
            }

            await Context.MarkCmdOkAsync().ConfigureAwait(false);
        }

        [Command("stats")]
        [AmongUsServer]
        public async Task StatsCmdAsync()
        {
            await ReplyAsync(embed: _auService.GetStatsEmbed("impostor")).ConfigureAwait(false);
            await ReplyAsync(embed: _auService.GetStatsEmbed("crew")).ConfigureAwait(false);
        }

        [Command("stats crew")]
        [AmongUsServer]
        public async Task StatsCrewCmdAsync()
        {
            await ReplyAsync(embed: _auService.GetStatsEmbed("crew")).ConfigureAwait(false);
        }

        [Command("stats impostor")]
        [AmongUsServer]
        public async Task StatsImpostorCmdAsync()
        {
            await ReplyAsync(embed: _auService.GetStatsEmbed("impostor")).ConfigureAwait(false);
        }
    }
}
