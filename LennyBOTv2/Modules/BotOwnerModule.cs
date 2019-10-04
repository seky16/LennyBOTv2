using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using LennyBOTv2.Preconditions;

namespace LennyBOTv2.Modules
{
    public class BotOwnerModule : LennyModuleBase
    {
        [Command("botnick")]
        [IsBotOwner]
        public async Task BotNickCmdAsync([Remainder]string name)
            => await Context.Guild.CurrentUser.ModifyAsync(x => x.Nickname = name).ConfigureAwait(false);

        [Command("exit", RunMode = RunMode.Async)]
        [IsBotOwner]
        public async Task ExitCmdAsync()
        {
            await ReplyAsync("Shutting down... :zzz:").ConfigureAwait(false);
            await Context.Client.SetStatusAsync(UserStatus.Invisible).ConfigureAwait(false);
            await Context.Client.StopAsync().ConfigureAwait(false);
            Environment.Exit(0);
        }

        [Command("playing")]
        [IsBotOwner]
        public async Task PlayingCmdAsync([Remainder]string game)
            => await Context.Client.SetGameAsync(game).ConfigureAwait(false);

        [Command("restart", RunMode = RunMode.Async)]
        [IsBotOwner]
        public async Task RestartCmdAsync()
        {
            var msg = await ReplyAsync("Restarting... :arrows_counterclockwise:").ConfigureAwait(false);
            await Context.Client.StopAsync().ConfigureAwait(false);
            await Context.Client.LogoutAsync().ConfigureAwait(false);
            await Context.Client.LoginAsync(TokenType.Bot, Config["token"]).ConfigureAwait(false);
            await Context.Client.StartAsync().ConfigureAwait(false);
            // TODO fix null ref exception?
            await msg.ModifyAsync(m => m.Content = "Restarted :white_check_mark:").ConfigureAwait(false);
        }

        [Command("say"), Alias("s")]
        [IsBotOwner]
        public async Task SayCmdAsync([Remainder]string text)
            => await ReplyAsync(text).ConfigureAwait(false);
    }
}
