using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using LennyBOTv2.Preconditions;
using LennyBOTv2.Services;

namespace LennyBOTv2.Modules
{
    public class BotOwnerModule : LennyModuleBase
    {
        private readonly MessageHandlingService _messageHandlingService;

        public BotOwnerModule(MessageHandlingService messageHandlingService)
        {
            _messageHandlingService = messageHandlingService;
        }

        [Command("botnick")]
        [IsBotOwner]
        public async Task BotNickCmdAsync([Remainder] string name)
            => await Context.Guild.CurrentUser.ModifyAsync(x => x.Nickname = name).ConfigureAwait(false);

        [Command("count")]
        [IsBotOwner]
        public async Task CountMsgsAsync()
        {
            await ReplyAsync(_messageHandlingService.GetMessageCount().ToString("N0")).ConfigureAwait(false);
        }

        [Command("exit", RunMode = RunMode.Async)]
        [IsBotOwner]
        public async Task ExitCmdAsync()
        {
            await ReplyAsync("Shutting down... :zzz:").ConfigureAwait(false);
            await Context.Client.SetStatusAsync(UserStatus.Invisible).ConfigureAwait(false);
            await Context.Client.StopAsync().ConfigureAwait(false);
            Environment.Exit(0);
        }

        [Command("listening")]
        [IsBotOwner]
        public async Task ListeningCmdAsync([Remainder] string name)
            => await Context.Client.SetGameAsync(name, null, ActivityType.Listening).ConfigureAwait(false);

        [Command("playing")]
        [IsBotOwner]
        public async Task PlayingCmdAsync([Remainder] string name)
            => await Context.Client.SetGameAsync(name, null, ActivityType.Playing).ConfigureAwait(false);

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
        public async Task SayCmdAsync([Remainder] string text)
            => await ReplyAsync(text).ConfigureAwait(false);

        [Command("status")]
        [IsBotOwner]
        public async Task StatusCmdAsync([Remainder] string name)
            => await Context.Client.SetGameAsync(name, null, ActivityType.CustomStatus).ConfigureAwait(false);

        [Command("watching")]
        [IsBotOwner]
        public async Task WatchingCmdAsync([Remainder] string name)
            => await Context.Client.SetGameAsync(name, null, ActivityType.Watching).ConfigureAwait(false);
    }
}
