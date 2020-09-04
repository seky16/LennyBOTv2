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
            await _auService.WriteStatsAsync(player, gameResult).ConfigureAwait(false);
            await Context.Message.AddReactionAsync(new Emoji("✅")).ConfigureAwait(false);
        }

        [Command("stats")]
        [AmongUsServer]
        public async Task StatsCmdAsync()
        {
            await _auService.GetStats().ConfigureAwait(false);
        }
    }
}
